using Newtonsoft.Json;
using RBX_Alt_Manager.Classes;
using RBX_Alt_Manager.Forms;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp.Net.WebSockets;
using static RBX_Alt_Manager.Program;

namespace RBX_Alt_Manager.Nexus
{
    public class ControlledAccount
    {
        [JsonIgnore] public Account LinkedAccount;
        [JsonIgnore] public AccountStatus Status;

        [JsonIgnore] public DateTime LastPing;

        public string Username;

        public long PlaceId;
        [JsonIgnore] public string PlaceName = "";
        public string JobId;
        [JsonIgnore] private string _InGameJobId;
        [JsonIgnore]
        public string InGameJobId
        {
            get => _InGameJobId;
            set
            {
                if (_InGameJobId == value) return;

                _InGameJobId = value;
                RefreshServerInfo();
            }
        }
        [JsonIgnore] public int PlayerCount = -1;
        [JsonIgnore] public int MaxPlayers = -1;
        [JsonIgnore] public string ServerPlayers => PlayerCount >= 0 && MaxPlayers >= 0 ? $"{PlayerCount}/{MaxPlayers}" : "";
        public double RelaunchDelay = 30;

        public bool AutoRejoin;
        public string AutoRejoinJobId = "";

        // Stamped whenever AutoRejoin is turned on for this account (see
        // AutoRejoinCheckbox_CheckedChanged and SetAutoRejoin) - lets GetOrAddAccount's
        // "adopt the newest Auto Re-join Job ID" feature pick whichever account turned Auto
        // Re-join on most recently, when several accounts have it enabled with different Job IDs.
        [JsonIgnore] public DateTime AutoRejoinEnabledAt;

        public bool IsChecked;
        public bool ClientCanReceive;

        [JsonIgnore] public WebSocketContext Context;

        // Bumped every time this account moves to a different server/connection, so a poll
        // loop started for a previous server can tell it's been superseded and stop instead of
        // continuing to poll a server this account already left.
        [JsonIgnore] private int _pollGeneration;

        private static readonly Random PollJitter = new Random();

        public ControlledAccount(Account account)
        {
            LinkedAccount = account;
            Username = LinkedAccount?.Username;
            Status = AccountStatus.Offline;
            LastPing = DateTime.Now.AddSeconds(-20);
        }

        public void Connect(WebSocketContext Context)
        {
            if (Status == AccountStatus.Online)
            {
                Logger.Warn($"{Username} was already connected, disconnecting...");
                Disconnect();
            }

            Status = AccountStatus.Online;
            LastPing = DateTime.Now;

            this.Context = Context;

            AccountControl.Instance.ContextList.Add(Context, this);
            // BeginInvokeIfRequired, not InvokeIfRequired - see its own comment in Utilities.cs.
            // Connect/Disconnect/UpdatePlaceId/RefreshServerInfo all run on WebSocket message
            // threads, one per connected account - a bulk Web Control action (teleporting or
            // enabling Auto Re-join for many accounts at once) makes many of those threads call
            // back into these UI touch-ups within the same short window, and a blocking Invoke
            // from each of them in a row is what made the main window freeze.
            AccountControl.Instance.BeginInvokeIfRequired(() =>
            {
                AccountControl.Instance.AccountsView.RefreshObject(this);
                AccountControl.Instance.UpdateStatusSummary();
            });

            Logger.Info($"{Username} has connected");
        }

        public void UpdatePlaceId(long NewPlaceId)
        {
            if (NewPlaceId <= 0 || (PlaceId == NewPlaceId && !string.IsNullOrEmpty(PlaceName))) return;

            PlaceId = NewPlaceId;
            PlaceName = "";

            // BeginInvokeIfRequired - see Connect's comment above.
            AccountControl.Instance.BeginInvokeIfRequired(() => AccountControl.Instance.AccountsView.RefreshObject(this));

            Task.Run(async () =>
            {
                try
                {
                    string Name = await Batch.GetGameName(NewPlaceId);

                    if (PlaceId != NewPlaceId || string.IsNullOrEmpty(Name)) return;

                    PlaceName = Name;

                    AccountControl.Instance.BeginInvokeIfRequired(() => AccountControl.Instance.AccountsView.RefreshObject(this));
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to resolve game name for PlaceId {NewPlaceId}: {ex.Message}");
                }
            });

            RefreshServerInfo();
        }

        // Pulls the current/max player count for whatever server (PlaceId+JobId) this
        // account is presently in. Called whenever either PlaceId or InGameJobId changes -
        // both are needed to identify a specific server, so this is a no-op until both are
        // known (e.g. JobId usually arrives via a separate SetJobId message after PlaceId).
        // Also kicks off a self-rescheduling poll loop so the Players column keeps reflecting
        // this server's population while the account just sits in it, not only at the moment
        // it was joined.
        private void RefreshServerInfo()
        {
            long TargetPlaceId = PlaceId;
            string TargetJobId = InGameJobId;

            if (TargetPlaceId <= 0 || string.IsNullOrEmpty(TargetJobId)) return;

            PlayerCount = -1;
            MaxPlayers = -1;

            int Generation = ++_pollGeneration;

            Task.Run(() => PollServerInfoLoop(TargetPlaceId, TargetJobId, Generation));
        }

        // Runs one lookup, applies the result, then reschedules itself after a randomized delay
        // - as long as this account hasn't moved to a different server/generation in the
        // meantime. The random spread (rather than a fixed interval) keeps multiple watched
        // accounts from all polling in lockstep and stacking their requests together, which is
        // what actually trips Roblox's rate limit on this endpoint (GetServerInfo's own cache
        // and request gate handle the rest).
        private async Task PollServerInfoLoop(long TargetPlaceId, string TargetJobId, int Generation)
        {
            try
            {
                // A server that was just joined (teleport/rejoin) can take a few seconds to show
                // up in Roblox's own public server listing - GetServerInfo walks that listing
                // looking for this exact JobId, so an immediate lookup right after joining can
                // legitimately come back "not found" even though the server is live. A few
                // spaced-out retries absorb that indexing delay instead of leaving the Players
                // column permanently blank for a normal public server.
                (int Playing, int Max) = (-1, -1);

                for (int Attempt = 0; Attempt < 4; Attempt++)
                {
                    if (_pollGeneration != Generation) return;

                    (Playing, Max) = await Batch.GetServerInfo(TargetPlaceId, TargetJobId);

                    if (Playing >= 0 && Max >= 0) break;

                    await Task.Delay(5000);
                }

                // Bail if the account has already moved on to a different place/server by the
                // time this request comes back - don't stamp stale numbers onto whatever it's
                // actually in now, and don't reschedule a poll for a server it already left.
                if (_pollGeneration != Generation) return;

                PlayerCount = Playing;
                MaxPlayers = Max;

                // BeginInvokeIfRequired - see Connect's comment above. This poll loop runs
                // concurrently for every connected account, so this matters even outside bulk
                // Web Control actions.
                AccountControl.Instance.BeginInvokeIfRequired(() => AccountControl.Instance.AccountsView.RefreshObject(this));
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to resolve server info for PlaceId {TargetPlaceId} JobId {TargetJobId}: {ex.Message}");
            }

            // Reschedule the next poll for this same server, 60-90s out with per-account jitter.
            // Still gated on the generation check so a teleport/disconnect that happens during
            // the wait cancels this chain instead of it firing a stale lookup later.
            int DelayMs;

            lock (PollJitter)
                DelayMs = PollJitter.Next(60_000, 90_000);

            await Task.Delay(DelayMs);

            if (_pollGeneration != Generation) return;

            await PollServerInfoLoop(TargetPlaceId, TargetJobId, Generation);
        }

        public void Disconnect()
        {
            Logger.Info($"{Username} has disconnected");

            Status = AccountStatus.Offline;
            ClientCanReceive = false;
            PlayerCount = -1;
            MaxPlayers = -1;

            // Invalidate any pending poll loop from PollServerInfoLoop so it stops rescheduling
            // itself for a server this account isn't even connected to anymore.
            _pollGeneration++;

            if (Context != null) AccountControl.Instance.ContextList.Remove(Context);

            // BeginInvokeIfRequired - see Connect's comment above.
            AccountControl.Instance.BeginInvokeIfRequired(() =>
            {
                AccountControl.Instance.AccountsView.RefreshObject(this);
                AccountControl.Instance.UpdateStatusSummary();
            });
        }

        public void HandleMessage(string Message)
        {
            if (string.IsNullOrEmpty(Message)) return;

            if (Message.TryParseJson(out Command command))
            {
#if DEBUG
                if (command.Name != "ping")
                    Console.WriteLine($"{command.Name}: {Message}");
#endif

                if (command.Name == "ping")
                {
                    LastPing = DateTime.Now;
                    ClientCanReceive = true;
                }
                else if (command.Name == "GetText")
                    SendMessage($"ElementText:{AccountControl.Instance.GetTextFromElement(command.Payload["Name"])}");
                else if (command.Name == "SetRelaunch" && double.TryParse(command.Payload["Seconds"], out double Delay))
                {
                    Logger.Info($"Relaunch Delay for {Username} has been set to {Delay} through Nexus");
                    RelaunchDelay = Delay;
                }
                else if (command.Name == "SetPlaceId" && !string.IsNullOrEmpty(command.Payload["Content"]) && long.TryParse(command.Payload["Content"], out long lPlaceId))
                    UpdatePlaceId(lPlaceId);
                else if (command.Name == "SetJobId" && !string.IsNullOrEmpty(command.Payload["Content"]))
                {
                    // InGameJobId (not the plain JobId field) is what the grid's Job ID column
                    // displays and what RefreshServerInfo keys off of - without this, a
                    // teleport/rejoin never refreshes the player count or Job ID shown, since
                    // InGameJobId otherwise only gets set once, at initial websocket connect.
                    JobId = command.Payload["Content"];
                    InGameJobId = command.Payload["Content"];
                }
                else if (command.Name == "Echo" && !string.IsNullOrEmpty(command.Payload["Content"]))
                    AccountControl.Instance.EmitMessage(command.Payload["Content"], true);
                else if (Enum.TryParse(command.Name, out CommandCreateElement elementType))
                {
                    if (elementType != CommandCreateElement.NewLine && !(command.Payload.ContainsKey("Name") && command.Payload.ContainsKey("Content")))
                        return;

                    Size size = new Size(75, 22);
                    Padding margin = new Padding(3, 2, 3, 3);

                    if (command.Payload != null)
                    {
                        if (command.Payload.TryGetValue("Margin", out string Margins))
                        {
                            int[] i = Margins.Split(',').Select(int.Parse).ToArray();

                            if (i.Count() == 4)
                                margin = new Padding(i[0], i[1], i[2], i[3]);
                        }

                        if (command.Payload.TryGetValue("Size", out string Size))
                        {
                            int[] i = Size.Split(',').Select(int.Parse).ToArray();

                            if (i.Count() == 2)
                                size = new Size(i[0], i[1]);
                        }
                    }

                    switch (elementType)
                    {
                        case CommandCreateElement.CreateButton:
                            Utilities.InvokeIfRequired(AccountControl.Instance, () => AccountControl.Instance.AddCustomButton(command.Payload["Name"], command.Payload["Content"], size, margin));
                            return;

                        case CommandCreateElement.CreateTextBox:
                            Utilities.InvokeIfRequired(AccountControl.Instance, () => AccountControl.Instance.AddCustomTextBox(command.Payload["Name"], command.Payload["Content"], size, margin));
                            return;

                        case CommandCreateElement.CreateNumeric:
                            if (decimal.TryParse(command.Payload["Content"], out decimal DefaultValue) && int.TryParse(command.Payload["DecimalPlaces"], out int Decimals) && decimal.TryParse(command.Payload["Increment"], out decimal Increment))
                                Utilities.InvokeIfRequired(AccountControl.Instance, () => AccountControl.Instance.AddCustomNumericUpDown(command.Payload["Name"], DefaultValue, Decimals, Increment, size, margin));

                            return;

                        case CommandCreateElement.CreateLabel:
                            Utilities.InvokeIfRequired(AccountControl.Instance, () => AccountControl.Instance.AddCustomLabel(command.Payload["Name"], command.Payload["Content"], margin));
                            return;

                        case CommandCreateElement.NewLine:
                            Utilities.InvokeIfRequired(AccountControl.Instance, () => AccountControl.Instance.NewLine());
                            return;
                    }
                }
            }
        }

        public void SendMessage(string Message)
        {
            if (Status == AccountStatus.Offline) return;

            Context.WebSocket.Send(Message);
        }
    }
}