using Newtonsoft.Json;
using RBX_Alt_Manager.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RBX_Alt_Manager.Nexus
{
    // Lets multiple machines, each running their own copy of this app, show up on ONE shared
    // /control web page instead of each exposing a separate page for just their own accounts.
    // One machine (whichever one the user points ngrok at) acts as the relay - every machine,
    // including that same one, connects to it as a RelayClient and reports its own Accounts
    // list; the relay (RelayServer below) keeps the latest snapshot per machine and answers
    // /control/api/accounts with all of them combined. Teleport commands flow the opposite
    // direction: the web page's teleport request names a machine, the relay forwards that
    // command down that machine's own /relay socket, and that machine (not the relay) actually
    // calls TeleportToServer - the relay itself never needs direct access to any account's
    // live Nexus connection, only whichever machine is actually connected to it.
    internal static class RelayHub
    {
        public class MachineAccountRow
        {
            public string Machine;
            public string Username;
            public string Status;
            public long PlaceId;
            public string PlaceName;
            public string JobId;
            public int Players;
            public int MaxPlayers;
            public bool AutoRejoin;
            public string AutoRejoinJobId;
        }

        // Keyed by machine name - last-write-wins per machine, which is fine since each
        // machine only ever has one active relay connection at a time (a reconnect replaces
        // the previous entry for that name rather than appending another).
        private static readonly ConcurrentDictionary<string, List<MachineAccountRow>> MachineAccounts = new ConcurrentDictionary<string, List<MachineAccountRow>>();
        private static readonly ConcurrentDictionary<string, RelayServerBehavior> MachineConnections = new ConcurrentDictionary<string, RelayServerBehavior>();

        public static List<MachineAccountRow> GetAllAccounts() =>
            MachineAccounts.Values.SelectMany(x => x).ToList();

        // Called by the relay's own HTTP /control/api/teleport handler. Returns false
        // immediately (without waiting) if the named machine isn't currently connected -
        // there is nothing to route the command to.
        public static bool TryTeleport(string Machine, string Username, string JobId, Action<string> OnResult)
        {
            if (!MachineConnections.TryGetValue(Machine, out RelayServerBehavior Connection))
                return false;

            Connection.SendTeleportCommand(Username, JobId, OnResult);
            return true;
        }

        // Same shape as TryTeleport, for the /control/api/autorejoin handler - lets the web
        // page toggle Auto Re-join on an account that belongs to a different machine.
        public static bool TrySetAutoRejoin(string Machine, string Username, bool Enabled, string JobId, Action<string> OnResult)
        {
            if (!MachineConnections.TryGetValue(Machine, out RelayServerBehavior Connection))
                return false;

            Connection.SendSetAutoRejoinCommand(Username, Enabled, JobId, OnResult);
            return true;
        }

        internal static void Register(string Machine, RelayServerBehavior Connection)
        {
            MachineConnections[Machine] = Connection;
        }

        internal static void Unregister(string Machine, RelayServerBehavior Connection)
        {
            // Only remove if this is still the connection that's registered - an old
            // connection's OnClose firing after a newer reconnect already replaced it
            // shouldn't delete the newer one's data.
            if (MachineConnections.TryGetValue(Machine, out RelayServerBehavior Current) && Current == Connection)
            {
                MachineConnections.TryRemove(Machine, out _);
                MachineAccounts.TryRemove(Machine, out _);
            }
        }

        internal static void UpdateAccounts(string Machine, List<MachineAccountRow> Rows)
        {
            MachineAccounts[Machine] = Rows;
        }
    }

    // Server side of the relay protocol - this is what runs on whichever machine the user
    // points ngrok at. Every connected machine (RelayClient, including this same machine's own
    // client half) talks to one instance of this per connection.
    public class RelayServerBehavior : WebSocketBehavior
    {
        private string Machine;
        private readonly Dictionary<string, TaskCompletionSource<string>> PendingTeleports = new Dictionary<string, TaskCompletionSource<string>>();

        protected override void OnOpen()
        {
            string Token = Context.QueryString["token"];
            string Expected = AccountManager.AccountControl.Get("WebControlToken");

            if (string.IsNullOrEmpty(Expected) || Token != Expected)
            {
                Context.WebSocket.Close(CloseStatusCode.PolicyViolation, "Invalid token");
                return;
            }

            Machine = Context.QueryString["machine"];

            if (string.IsNullOrEmpty(Machine))
            {
                Context.WebSocket.Close(CloseStatusCode.InvalidData, "Missing machine name");
                return;
            }

            RelayHub.Register(Machine, this);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(Machine) || string.IsNullOrEmpty(e.Data)) return;

            Dictionary<string, object> Msg;

            try { Msg = JsonConvert.DeserializeObject<Dictionary<string, object>>(e.Data); }
            catch { return; }

            if (Msg == null || !Msg.TryGetValue("type", out object TypeObj)) return;

            string Type = TypeObj.ToString();

            if (Type == "accounts")
            {
                string RowsJson = Msg["rows"].ToString();
                var Rows = JsonConvert.DeserializeObject<List<RelayHub.MachineAccountRow>>(RowsJson);

                foreach (var Row in Rows) Row.Machine = Machine;

                RelayHub.UpdateAccounts(Machine, Rows);
            }
            else if (Type == "teleportResult" || Type == "setAutoRejoinResult")
            {
                string RequestId = Msg["requestId"].ToString();
                string Result = Msg["result"].ToString();

                lock (PendingTeleports)
                {
                    if (PendingTeleports.TryGetValue(RequestId, out TaskCompletionSource<string> Tcs))
                    {
                        PendingTeleports.Remove(RequestId);
                        Tcs.TrySetResult(Result);
                    }
                }
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (!string.IsNullOrEmpty(Machine))
                RelayHub.Unregister(Machine, this);
        }

        public void SendTeleportCommand(string Username, string JobId, Action<string> OnResult)
        {
            string RequestId = Guid.NewGuid().ToString("N");
            var Tcs = new TaskCompletionSource<string>();

            lock (PendingTeleports)
                PendingTeleports[RequestId] = Tcs;

            Send(JsonConvert.SerializeObject(new
            {
                type = "teleport",
                requestId = RequestId,
                username = Username,
                jobId = JobId
            }));

            // Fire-and-forget with a timeout - the HTTP request that triggered this must
            // eventually respond either way, even if the target machine never answers (e.g.
            // it disconnected between the TryTeleport check and now).
            Task.Run(async () =>
            {
                Task Completed = await Task.WhenAny(Tcs.Task, Task.Delay(15000));

                lock (PendingTeleports)
                    PendingTeleports.Remove(RequestId);

                OnResult(Completed == Tcs.Task ? Tcs.Task.Result : "ERROR: Timed out waiting for the target machine to respond");
            });
        }

        public void SendSetAutoRejoinCommand(string Username, bool Enabled, string JobId, Action<string> OnResult)
        {
            string RequestId = Guid.NewGuid().ToString("N");
            var Tcs = new TaskCompletionSource<string>();

            // Reuses the same PendingTeleports map as SendTeleportCommand - it's just a
            // RequestId -> result lookup, not actually teleport-specific, and GUIDs from both
            // call sites never collide.
            lock (PendingTeleports)
                PendingTeleports[RequestId] = Tcs;

            Send(JsonConvert.SerializeObject(new
            {
                type = "setAutoRejoin",
                requestId = RequestId,
                username = Username,
                enabled = Enabled,
                jobId = JobId
            }));

            Task.Run(async () =>
            {
                Task Completed = await Task.WhenAny(Tcs.Task, Task.Delay(15000));

                lock (PendingTeleports)
                    PendingTeleports.Remove(RequestId);

                OnResult(Completed == Tcs.Task ? Tcs.Task.Result : "ERROR: Timed out waiting for the target machine to respond");
            });
        }
    }

    // Owns the single RelayClient instance this machine uses, and tracks whether relay mode
    // is currently on - checked by AccountControl's ServeWebControlAccounts/HandleWebControlTeleport
    // to decide whether to answer from the plain local Accounts list or the aggregated
    // RelayHub view.
    internal static class RelayManager
    {
        private static RelayClient Client;

        public static bool IsEnabled { get; private set; }

        public static void Enable(string RelayUrl, string Token, string Machine, Action<bool> OnConnectionStateChanged = null)
        {
            Disable();

            Client = new RelayClient { OnConnectionStateChanged = OnConnectionStateChanged };
            Client.Connect(RelayUrl, Token, Machine);
            Client.StartHeartbeat();

            IsEnabled = true;
        }

        public static void Disable()
        {
            Client?.Disconnect();
            Client = null;
            IsEnabled = false;
        }

        // Called whenever AccountControl's local Accounts list changes, so a connected relay
        // (if any) gets the update immediately instead of waiting for the next heartbeat tick.
        public static void NotifyAccountsChanged()
        {
            Client?.PushAccounts();
        }
    }

    // Client side of the relay protocol - every machine (including the one acting as the
    // relay itself, connecting to its own /relay over localhost) runs one of these to report
    // its Accounts list and execute teleport commands the relay forwards back down.
    internal class RelayClient
    {
        private WebSocket Socket;
        private System.Timers.Timer HeartbeatTimer;
        private string RelayUrl;
        private string Token;
        private string Machine;
        private volatile bool ShouldReconnect;

        public bool IsConnected => Socket != null && Socket.ReadyState == WebSocketState.Open;

        // Lets AccountControl reflect the real connection state in RelayStatusLabel instead of
        // being stuck on "Connecting..." forever once a connection actually succeeds (or drops).
        public Action<bool> OnConnectionStateChanged;

        public void Connect(string RelayUrl, string Token, string Machine)
        {
            // The per-WebSocket SslConfiguration.EnabledSslProtocols below isn't always enough
            // on .NET Framework 4.7.2 - this WebSocketSharp fork's SslStream setup can still fall
            // back to the process-wide default, which can be below TLS 1.2 depending on the OS.
            // ngrok's edge requires TLS 1.2+ and rejects anything lower during the handshake,
            // which this library surfaces as a generic "exception has occurred while connecting"
            // close rather than a clear TLS error. Setting this process-wide is the standard fix.
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            this.RelayUrl = RelayUrl.TrimEnd('/');
            this.Token = Token;
            this.Machine = Machine;
            ShouldReconnect = true;

            OpenSocket();
        }

        private void OpenSocket()
        {
            string Url = $"{this.RelayUrl.Replace("http://", "ws://").Replace("https://", "wss://")}/relay?token={Uri.EscapeDataString(Token)}&machine={Uri.EscapeDataString(Machine)}";

            Socket = new WebSocket(Url);

            // ngrok's edge requires TLS 1.2+. The process-wide default under .NET Framework
            // 4.7.2 can still be TLS 1.0 depending on the OS, which makes the wss:// handshake
            // fail before OnOpen/OnError ever fire - it just lands straight on OnClose.
            Socket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            Socket.OnOpen += (s, e) =>
            {
                Program.Logger.Info($"[Relay] Connected to {this.RelayUrl} as \"{Machine}\"");
                OnConnectionStateChanged?.Invoke(true);
                PushAccounts();
            };

            Socket.OnMessage += (s, e) => HandleMessage(e.Data);

            Socket.OnError += (s, e) => Program.Logger.Warn($"[Relay] Connection error: {e.Message}");

            Socket.OnClose += (s, e) =>
            {
                Program.Logger.Warn($"[Relay] Closed (code={e.Code}, reason=\"{e.Reason}\", clean={e.WasClean})");
                OnConnectionStateChanged?.Invoke(false);

                if (!ShouldReconnect) return;

                // ngrok's free tier (and plain network blips) drop long-lived WebSocket
                // connections every so often - without a retry here, the user would need to
                // notice the relay stopped updating and manually re-check the enable box to
                // get back online.
                Program.Logger.Warn("[Relay] Disconnected, retrying in 5s...");
                Task.Delay(5000).ContinueWith(_ => { if (ShouldReconnect) OpenSocket(); });
            };

            // ConnectAsync (not Connect) - Connect() blocks the calling thread until the TCP+TLS
            // handshake finishes or times out, and RelayEnabledCB_CheckedChanged calls this
            // directly on the UI thread, which froze the whole window ("Not Responding") whenever
            // the relay host was slow to answer (e.g. a cold ngrok tunnel or a flaky network).
            try { Socket.ConnectAsync(); }
            catch (Exception ex) { Program.Logger.Warn($"[Relay] Failed to connect: {ex.Message}"); }
        }

        public void Disconnect()
        {
            ShouldReconnect = false;
            HeartbeatTimer?.Stop();
            HeartbeatTimer?.Dispose();
            HeartbeatTimer = null;

            try { Socket?.Close(); } catch { }
            Socket = null;
        }

        // Called whenever AccountControl's own Accounts list changes, plus on a periodic
        // heartbeat (StartHeartbeat below) so a newly (re)connected relay side always gets a
        // full picture even if nothing changed between connects.
        public void PushAccounts()
        {
            if (!IsConnected) return;

            var Rows = AccountControl.Instance.Accounts.Select(a => new
            {
                username = a.Username,
                status = a.Status.ToString(),
                placeId = a.PlaceId,
                placeName = a.PlaceName,
                jobId = a.InGameJobId,
                players = a.PlayerCount,
                maxPlayers = a.MaxPlayers,
                autoRejoin = a.AutoRejoin,
                autoRejoinJobId = a.AutoRejoinJobId
            }).ToList();

            string Payload = JsonConvert.SerializeObject(new
            {
                type = "accounts",
                rows = JsonConvert.SerializeObject(Rows)
            });

            // IsConnected above and Send() here aren't atomic - the heartbeat timer (a
            // ThreadPool thread) can race a drop that happens between the check and the call,
            // which would otherwise throw out of a bare timer callback with nothing to catch it.
            try { Socket?.Send(Payload); }
            catch (Exception ex) { Program.Logger.Warn($"[Relay] Failed to send accounts: {ex.Message}"); }
        }

        public void StartHeartbeat()
        {
            HeartbeatTimer = new System.Timers.Timer(5000);
            HeartbeatTimer.Elapsed += (s, e) => PushAccounts();
            HeartbeatTimer.Start();
        }

        private void HandleMessage(string Data)
        {
            Dictionary<string, object> Msg;

            try { Msg = JsonConvert.DeserializeObject<Dictionary<string, object>>(Data); }
            catch { return; }

            if (Msg == null || !Msg.TryGetValue("type", out object TypeObj)) return;

            string Type = TypeObj.ToString();

            if (Type != "teleport" && Type != "setAutoRejoin") return;

            string RequestId = Msg["requestId"].ToString();
            string Username = Msg["username"].ToString();

            if (Type == "teleport")
            {
                string JobId = Msg["jobId"].ToString();

                Task.Run(async () =>
                {
                    string Result;

                    try
                    {
                        RBX_Alt_Manager.Account LinkedAcc = AccountManager.AccountsList.FirstOrDefault(a => a.Username == Username);
                        ControlledAccount ControlledAcc = AccountControl.Instance.Accounts.FirstOrDefault(a => a.Username == Username);

                        if (LinkedAcc == null || ControlledAcc == null)
                            Result = $"ERROR: No account named {Username} found on this machine";
                        else if (ControlledAcc.PlaceId <= 0)
                            Result = "ERROR: Cannot teleport: Place ID is empty";
                        else
                            Result = await LinkedAcc.TeleportToServer(ControlledAcc.PlaceId, JobId);
                    }
                    catch (Exception ex)
                    {
                        Result = $"ERROR: {ex.Message}";
                    }

                    if (IsConnected)
                    {
                        Socket.Send(JsonConvert.SerializeObject(new
                        {
                            type = "teleportResult",
                            requestId = RequestId,
                            result = Result
                        }));
                    }
                });
            }
            else // setAutoRejoin
            {
                bool Enabled = Convert.ToBoolean(Msg["enabled"]);
                string JobId = Msg["jobId"]?.ToString();

                string Result;

                try { Result = AccountControl.Instance.SetAutoRejoin(Username, Enabled, JobId); }
                catch (Exception ex) { Result = $"ERROR: {ex.Message}"; }

                if (IsConnected)
                {
                    Socket.Send(JsonConvert.SerializeObject(new
                    {
                        type = "setAutoRejoinResult",
                        requestId = RequestId,
                        result = Result
                    }));
                }
            }
        }
    }
}
