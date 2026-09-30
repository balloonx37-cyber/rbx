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
        public bool IsChecked;
        public bool ClientCanReceive;

        [JsonIgnore] public WebSocketContext Context;

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
            AccountControl.Instance.InvokeIfRequired(() =>
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

            AccountControl.Instance.InvokeIfRequired(() => AccountControl.Instance.AccountsView.RefreshObject(this));

            Task.Run(async () =>
            {
                try
                {
                    string Name = await Batch.GetGameName(NewPlaceId);

                    if (PlaceId != NewPlaceId || string.IsNullOrEmpty(Name)) return;

                    PlaceName = Name;

                    AccountControl.Instance.InvokeIfRequired(() => AccountControl.Instance.AccountsView.RefreshObject(this));
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
        private void RefreshServerInfo()
        {
            long TargetPlaceId = PlaceId;
            string TargetJobId = InGameJobId;

            if (TargetPlaceId <= 0 || string.IsNullOrEmpty(TargetJobId)) return;

            PlayerCount = -1;
            MaxPlayers = -1;

            Task.Run(async () =>
            {
                try
                {
                    (int Playing, int Max) = await Batch.GetServerInfo(TargetPlaceId, TargetJobId);

                    // Bail if the account has already moved on to a different place/server
                    // by the time this request comes back - don't stamp stale numbers onto
                    // whatever it's actually in now.
                    if (PlaceId != TargetPlaceId || InGameJobId != TargetJobId) return;

                    PlayerCount = Playing;
                    MaxPlayers = Max;

                    AccountControl.Instance.InvokeIfRequired(() => AccountControl.Instance.AccountsView.RefreshObject(this));
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to resolve server info for PlaceId {TargetPlaceId} JobId {TargetJobId}: {ex.Message}");
                }
            });
        }

        public void Disconnect()
        {
            Logger.Info($"{Username} has disconnected");

            Status = AccountStatus.Offline;
            ClientCanReceive = false;
            PlayerCount = -1;
            MaxPlayers = -1;

            if (Context != null) AccountControl.Instance.ContextList.Remove(Context);

            AccountControl.Instance.InvokeIfRequired(() =>
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
                    JobId = command.Payload["Content"];
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