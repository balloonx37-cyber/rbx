using BrightIdeasSoftware;
using Newtonsoft.Json;
using RBX_Alt_Manager.Classes;
using RBX_Alt_Manager.Nexus;
using RBX_Alt_Manager.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;
using WebSocketSharp.Net.WebSockets;
using WebSocketSharp.Server;

namespace RBX_Alt_Manager.Forms
{
    public partial class AccountControl : Form
    {
        private static readonly string ACFile = Path.Combine(Environment.CurrentDirectory, "AccountControlData.json");
        private static readonly object SaveLock = new object();

        // Guards read-modify-write access to Accounts. WebsocketServer.OnOpen runs on the
        // WebSocket server's own thread pool, so two connections for the same username can
        // fire concurrently (e.g. a client reconnecting) - without this lock, both could see
        // Accounts.FirstOrDefault(...) == null at the same time and each add their own
        // ControlledAccount, producing duplicate rows in the grid for one account.
        private static readonly object AccountsLock = new object();

        public static AccountControl Instance;
        public static HttpServer Server;

        public Dictionary<WebSocketContext, ControlledAccount> ContextList = new Dictionary<WebSocketContext, ControlledAccount>();
        public List<ControlledAccount> Accounts = new List<ControlledAccount>();

        private readonly Dictionary<string, Control> CustomElements = new Dictionary<string, Control>();
        private Control LastControl;

        private bool SettingsLoaded;

        // Auto Re-join: stored per-account (ControlledAccount.AutoRejoin / AutoRejoinJobId)
        // and edited via the selected grid rows, rather than a separate account-picker dialog.
        // A single timer checks every connected account that has AutoRejoin enabled and
        // teleports it back to its own AutoRejoinJobId if its current Job ID doesn't match.
        private System.Timers.Timer AutoRejoinTimer;
        private volatile bool AutoRejoinCheckInProgress = false;

        public AccountControl()
        {
            Instance = this;

            AccountManager.SetDarkBar(Handle);

            InitializeComponent();
            this.Rescale();
        }

        #region Save File

        internal void SaveAccounts()
        {
            lock (SaveLock)
                File.WriteAllText(ACFile, JsonConvert.SerializeObject(Accounts));
        }

        private void LoadAccounts()
        {
            if (File.Exists(ACFile))
            {
                try
                {
                    string JSON = File.ReadAllText(ACFile);

                    if (string.IsNullOrEmpty(JSON)) return;

                    Accounts = JsonConvert.DeserializeObject<List<ControlledAccount>>(JSON);

                    foreach (ControlledAccount cAccount in Accounts)
                    {
                        Account account = AccountManager.AccountsList.FirstOrDefault(x => x.Username == cAccount.Username);

                        if (account != null)
                            cAccount.LinkedAccount = account;
                    }

                    Accounts.RemoveAll(x => x.LinkedAccount == null);
                }
                catch { }
            }

            AccountsView.SetObjects(Accounts);

            UpdateStatusSummary();
        }

        // Recomputes and displays the Online/Offline/Total counts shown under the grid.
        // Called whenever accounts are added, removed, loaded, or change connection status
        // (Connect/Disconnect in ControlledAccount), so the summary always reflects the
        // current state of the Accounts list without the user having to count rows manually.
        public void UpdateStatusSummary()
        {
            this.InvokeIfRequired(() =>
            {
                int online = Accounts.Count(x => x.Status == AccountStatus.Online);
                int total = Accounts.Count;

                StatusSummaryLabel.Text = $"Online: {online}  Offline: {total - online}  Total: {total}";
            });
        }

        // Atomically finds an existing ControlledAccount for this username, or creates and
        // registers a new one if none exists yet. Called from WebsocketServer.OnOpen, which
        // can run concurrently for the same username (e.g. Roblox reconnecting) - doing the
        // find-or-create under a single lock instead of a separate check-then-add avoids
        // ending up with two ControlledAccount entries for one account.
        // Returns (account, wasCreated) so the caller only logs/saves when it actually created one.
        internal (ControlledAccount account, bool wasCreated) GetOrAddAccount(string username, long userId)
        {
            lock (AccountsLock)
            {
                ControlledAccount existing = Accounts.FirstOrDefault(x => x.Username == username);

                if (existing != null)
                    return (existing, false);

                Account linkedAccount = AccountManager.AccountsList.FirstOrDefault(x => x.Username == username);

                if (linkedAccount == null)
                {
                    linkedAccount = new Account { Username = username, UserID = userId };

                    AccountManager.AccountsList.Add(linkedAccount);
                    Utilities.InvokeIfRequired(AccountManager.Instance, () => AccountManager.Instance.RefreshView(linkedAccount));
                    AccountManager.SaveAccounts(true, true);
                }

                ControlledAccount newAccount = new ControlledAccount(linkedAccount);

                Accounts.Add(newAccount);
                Utilities.InvokeIfRequired(Instance, () =>
                {
                    AccountsView.AddObject(newAccount);
                    SaveAccounts();
                    UpdateStatusSummary();
                });

                return (newAccount, true);
            }
        }

        #endregion

        #region Server

        private void OpenServer()
        {
            if (!int.TryParse(AccountManager.AccountControl.Get("NexusPort"), out int Port))
                throw new Exception("Failed to start server, invalid Port setting");

            if (Port < 1 || Port > 65535)
                throw new Exception("Port can not be less than 1 or more than 65535");

            // Using HttpServer instead of WebSocketServer so the same listener can also serve
            // Nexus.lua's contents over plain HTTP GET (see OnGet below). The old NexusLoader
            // resource always re-downloaded Nexus.lua from GitHub on every run, which meant local
            // edits to Nexus.lua (e.g. adding the placeId field) never reached the running game
            // client. Serving the file that ships with this build over localhost removes that
            // GitHub dependency entirely - HttpServer supports WebSocket services the same way
            // WebSocketServer does, so /Nexus keeps working unchanged.
            Server = new HttpServer(AccountManager.AccountControl.Get<bool>("AllowExternalConnections") ? IPAddress.Any : IPAddress.Loopback, Port, false);

#if DEBUG
            Server.Log.Level = LogLevel.Debug;
#endif

            Server.AddWebSocketService<WebsocketServer>("/Nexus");

            Server.OnGet += (sender, e) =>
            {
                if (e.Request.RawUrl != "/Nexus.lua")
                {
                    e.Response.StatusCode = 404;
                    return;
                }

                string path = Path.Combine(Environment.CurrentDirectory, "Nexus.lua");

                if (!File.Exists(path))
                {
                    e.Response.StatusCode = 404;
                    return;
                }

                byte[] content = File.ReadAllBytes(path);

                e.Response.ContentType = "text/plain";
                e.Response.ContentLength64 = content.LongLength;
                e.Response.Close(content, true);
            };

            Server.Start();

            Process.GetCurrentProcess().WaitForExit();
        }

        public void EmitMessage(string Message, bool ToAll = false)
        {
            foreach (ControlledAccount account in ToAll ? AccountsView.Objects : AccountsView.CheckedObjects)
                account.SendMessage(Message);
        }

        #endregion

        #region Custom Controls

        public void AddCustomButton(string Name, string Text, Size size, Padding margin)
        {
            if (CustomElements.ContainsKey(Name))
                return;

            Button button = new Button
            {
                Name = Name,
                Size = size,
                Margin = margin,
                Text = Text,
                UseVisualStyleBackColor = true
            };

            button.Click += CustomButton_Click;

            LastControl = button;

            ControlsPanel.Controls.Add(button);

            CustomElements.Add(Name, button);

            ApplyTheme(ControlsPanel.Controls);
        }

        public void AddCustomTextBox(string Name, string Text, Size size, Padding margin)
        {
            if (CustomElements.ContainsKey(Name))
                return;

            TextBox textBox = new TextBox
            {
                Name = Name,
                Size = size,
                Margin = margin,
                Text = Text
            };

            LastControl = textBox;

            ControlsPanel.Controls.Add(textBox);

            CustomElements.Add(Name, textBox);

            ApplyTheme(ControlsPanel.Controls);
        }

        public void AddCustomNumericUpDown(string Name, decimal DefaultValue, int DecimalPlaces, decimal Increment, Size size, Padding margin)
        {
            if (CustomElements.ContainsKey(Name))
                return;

            NumericUpDown control = new NumericUpDown
            {
                DecimalPlaces = DecimalPlaces,
                Increment = Increment,
                Name = Name,
                Margin = margin,
                Size = size,
                Value = DefaultValue,
                Minimum = decimal.MinValue,
                Maximum = decimal.MaxValue
            };

            LastControl = control;

            ControlsPanel.Controls.Add(control);

            CustomElements.Add(Name, control);

            ApplyTheme(ControlsPanel.Controls);
        }

        public void AddCustomLabel(string Name, string Text, Padding margin)
        {
            if (CustomElements.ContainsKey(Name))
                return;

            Label label = new Label
            {
                Name = Name,
                Margin = margin,
                Text = Text
            };

            LastControl = label;

            ControlsPanel.Controls.Add(label);

            CustomElements.Add(Name, label);

            ApplyTheme(ControlsPanel.Controls);
        }

        public void NewLine()
        {
            if (LastControl != null)
                ControlsPanel.SetFlowBreak(LastControl, true);
        }

        public string GetTextFromElement(string Name)
        {
            if (CustomElements.TryGetValue(Name, out Control control))
                return control.Text;

            return string.Empty;
        }

        private void CustomButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            EmitMessage($"ButtonClicked:{btn.Name}");
        }

        #endregion

        #region Events

        public void Initialize()
        {
            // Guard against running twice: AccountManager_Shown now calls this directly (so
            // the Nexus server/account grid are ready before Account Control is ever shown),
            // but AccountControl_Load also calls Initialize() the first time this form is
            // shown - without this guard that meant OpenServer() ran twice and the second
            // HttpServer.Start() on the same port threw (address already in use), which is
            // what the "Unhandled exception" dialog on launch was.
            if (SettingsLoaded) return;

            cStatus.AspectGetter = delegate (object row)
            {
                ControlledAccount acc = (ControlledAccount)row;

                return acc.Status;
            };
            cStatus.Renderer = new MappedImageRenderer(new object[] {
                AccountStatus.Online, Resources.online,
                AccountStatus.Offline, Resources.offline
            });

            Task Listener = new Task(new Action(OpenServer));
            Listener.Start();

            try { Listener.Wait(50); }
            catch (InvalidOperationException x)
            {
                MessageBox.Show($"{x.Message} {x.StackTrace}", "Account Control", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Hide();
            }

            AllowExternalConnectionsCB.Checked = AccountManager.AccountControl.Get<bool>("AllowExternalConnections");
            StartOnLaunch.Checked = AccountManager.AccountControl.Get<bool>("StartOnLaunch");
            PortNumber.Value = AccountManager.AccountControl.Get<decimal>("NexusPort");
            RelaunchDelayNumber.Value = AccountManager.AccountControl.Get<decimal>("RelaunchDelay");
            LauncherDelayNumber.Value = AccountManager.AccountControl.Get<decimal>("LauncherDelayNumber");
            AutoMinimizeCB.Checked = AccountManager.AccountControl.Get<bool>("AutoMinimizeEnabled");
            AutoCloseCB.Checked = AccountManager.AccountControl.Get<bool>("AutoCloseEnabled");
            InternetCheckCB.Checked = AccountManager.AccountControl.Get<bool>("InternetCheck");
            UsePresenceCB.Checked = AccountManager.AccountControl.Get<bool>("UsePresence");
            AutoMinIntervalNum.Value = Math.Max(Math.Min(AccountManager.AccountControl.Get<decimal>("AutoMinimizeInterval"), AutoMinIntervalNum.Minimum), AutoMinIntervalNum.Maximum);
            AutoCloseIntervalNum.Value = Math.Max(Math.Min(AccountManager.AccountControl.Get<decimal>("AutoCloseInterval"), AutoCloseIntervalNum.Maximum), AutoCloseIntervalNum.Minimum);
            MaxInstancesNum.Value = Math.Max(Math.Min(AccountManager.AccountControl.Get<int>("MaxInstances"), MaxInstancesNum.Maximum), MaxInstancesNum.Minimum);
            AutoCloseType.SelectedIndex = AccountManager.AccountControl.Get<int>("AutoCloseType");

            SettingsLoaded = true;

            LoadAccounts();
        }

        private void AccountControl_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void AccountsView_DragOver(object sender, DragEventArgs e)
        {
            if (sender != null && sender is ObjectListView)
                e.Effect = DragDropEffects.Copy;
        }

        private void AccountsView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.CommaSeparatedValue))
            {
                string Data = (string)e.Data.GetData(DataFormats.CommaSeparatedValue);

                foreach (string Line in Data.Split('\n'))
                {
                    string Username = string.Empty;

                    Match match = Regex.Match(Line, @"""(\w+)""");

                    if (match.Success)
                        Username = match.Groups[1].Value;

                    if (!string.IsNullOrEmpty(Username))
                    {
                        Account account = AccountManager.AccountsList.FirstOrDefault(x => x.Username == Username);

                        if (account != null && !Accounts.Exists(x => x.LinkedAccount == account))
                        {
                            ControlledAccount cAccount = new ControlledAccount(account);

                            Accounts.Add(cAccount);
                            AccountsView.AddObject(cAccount);

                            SaveAccounts();
                            UpdateStatusSummary();
                        }
                    }
                }
            }
        }

        private void AutoMinimizeCB_CheckedChanged(object sender, EventArgs e)
        {
            MinimzeTimer.Enabled = AutoMinimizeCB.Checked;

            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("AutoMinimizeEnabled", AutoMinimizeCB.Checked ? "true" : "false");
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void AutoRejoinCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoRejoinCheckbox.CheckState == CheckState.Indeterminate) return;

            foreach (ControlledAccount account in AccountsView.SelectedObjects)
                account.AutoRejoin = AutoRejoinCheckbox.Checked;

            SaveAccounts();

            if (AutoRejoinCheckbox.Checked)
            {
                if (AutoRejoinTimer == null)
                {
                    AutoRejoinTimer = new System.Timers.Timer(12000); // Check every 12 seconds
                    AutoRejoinTimer.Elapsed += AutoRejoinTimer_Elapsed;
                }

                AutoRejoinTimer.Start();
                LogWindow.GetInstance().Show();
                LogAutoRejoin("Auto Re-join enabled. Checking every 12 seconds...");

                // Trigger first check immediately
                AutoRejoinTimer_Elapsed(null, null);
            }
        }

        private void AutoRejoinJobIdTextBox_Leave(object sender, EventArgs e)
        {
            foreach (ControlledAccount account in AccountsView.SelectedObjects)
                account.AutoRejoinJobId = AutoRejoinJobIdTextBox.Text.Trim();

            SaveAccounts();
        }

        private void AutoRejoinTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Prevent overlapping ticks: if a previous check is still running (e.g. many
            // accounts, slow network), skip this tick instead of piling up on top of it.
            if (AutoRejoinCheckInProgress) return;

            AutoRejoinCheckInProgress = true;

            // This already runs on a ThreadPool thread (Timer.Elapsed doesn't fire on the UI
            // thread). The check logic doesn't touch any UI controls directly - the account and
            // teleport calls marshal their own UI touch-ups when needed - so there's no reason
            // to bounce onto the UI thread here.
            RunAutoRejoinCheckAsync();
        }

        private async void RunAutoRejoinCheckAsync()
        {
            try
            {
                var targets = Accounts.Where(a => a.AutoRejoin && !string.IsNullOrEmpty(a.AutoRejoinJobId)).ToList();

                if (targets.Count == 0)
                {
                    LogAutoRejoin("Check ran: no accounts have Auto Re-join enabled with a Job ID set.");
                    return;
                }

                var teleportTasks = new List<Task>();

                foreach (ControlledAccount account in targets)
                {
                    string currentJobId = account.InGameJobId ?? account.JobId;

                    if (string.IsNullOrEmpty(currentJobId) || currentJobId != account.AutoRejoinJobId)
                        teleportTasks.Add(TeleportAccount(account, account.AutoRejoinJobId));
                    else
                        LogAutoRejoin($"{account.Username} is already in the target Job ID, no action needed.");
                }

                if (teleportTasks.Count > 0)
                    await Task.WhenAll(teleportTasks);
            }
            catch (Exception ex)
            {
                LogAutoRejoin($"Error: {ex.Message} Trace: {ex.StackTrace}", true);
            }
            finally
            {
                AutoRejoinCheckInProgress = false;
            }
        }

        private async Task TeleportAccount(ControlledAccount controlledAccount, string jobId)
        {
            try
            {
                if (controlledAccount.PlaceId <= 0)
                {
                    LogAutoRejoin($"Cannot teleport {controlledAccount.Username}: Place ID is empty");
                    return;
                }

                Account account = AccountManager.AccountsList.FirstOrDefault(a => a.Username == controlledAccount.Username);
                if (account == null)
                {
                    LogAutoRejoin($"Cannot find Account object for {controlledAccount.Username}");
                    return;
                }

                LogAutoRejoin($"Teleporting {account.Username} to Job ID {jobId}...");

                string result = await account.TeleportToServer(controlledAccount.PlaceId, jobId);

                LogAutoRejoin($"Teleport {(result.Contains("Success") ? "succeeded" : "failed")} for {account.Username}: {result}");
            }
            catch (Exception ex)
            {
                LogAutoRejoin($"Failed to teleport {controlledAccount.Username}: {ex.Message}", true);
            }
        }

        // Writes to both the regular log4net logger (log.txt) and the "Auto Re-join Logs" window
        // that's shown when Auto Re-join is enabled - the window used to just sit empty because
        // nothing ever called LogWindow.AppendLog, only Program.Logger (which only goes to file).
        private static void LogAutoRejoin(string message, bool isError = false)
        {
            string tagged = $"[Auto Re-join] {message}";

            if (isError) Program.Logger.Error(tagged);
            else Program.Logger.Info(tagged);

            LogWindow.GetInstance().AppendLog(tagged);
        }

        private void AccountsView_SelectionChanged(object sender, EventArgs e)
        {
            List<ControlledAccount> Objects = AccountsView.SelectedObjects.OfType<ControlledAccount>().ToList();

            if (Objects.Count == 0) return;

            ControlledAccount main = Objects[0];

            AutoRejoinCheckbox.CheckState = Objects.Exists(x => x.AutoRejoin != main.AutoRejoin) ? CheckState.Indeterminate : (main.AutoRejoin ? CheckState.Checked : CheckState.Unchecked);
            AutoRejoinJobIdTextBox.Text = Objects[0].AutoRejoinJobId;
        }

        private void NexusDL_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Nexus.lua");

            // NexusLoader fetches the actual Nexus script from this program's own HTTP endpoint
            // (added in OpenServer/OnGet above) rather than downloading it from GitHub, so local
            // changes made to Nexus.lua in this install always reach the game client - the port
            // is baked in here since it's user-configurable in Settings.
            string port = AccountManager.AccountControl.Get("NexusPort");
            string loader = Resources.NexusLoader.Replace("{NEXUS_PORT}", port);

            File.WriteAllText(path, loader);

            Process.Start("explorer.exe", "/select, " + path);
        }

        private void NexusDocsButton_Click(object sender, EventArgs e) => Process.Start("https://github.com/ic3w0lf22/Roblox-Account-Manager/blob/master/RBX%20Alt%20Manager/Nexus/NexusDocs.md");

        private async void AutoRelaunchTimer_Tick(object sender, EventArgs e)
        {
            // The relaunch-a-new-Roblox-client and dead-process-cleanup behavior below have been
            // disabled by request, so this program no longer touches running Roblox processes
            // outside of the Auto Re-join teleport flow in AccountManager.cs (which talks to the
            // game over the existing Nexus WebSocket connection instead of the OS process).
            // Presence lookups are kept since they only read status from Roblox's web API and
            // don't affect any running process.
            if (AccountManager.AccountControl.Get<bool>("InternetCheck") && !Utilities.IsConnectedToInternet()) return;

            try
            {
                if (AccountManager.AccountControl.Get<bool>("UsePresence"))
                    await Presence.UpdatePresence(Accounts.Select(a => a.LinkedAccount.UserID).ToArray());
            }
            catch (Exception x) { Program.Logger.Error($"An error occured updating presence from auto relaunch: {x.Message} Trace: {x.StackTrace}"); }
        }

        private void AccountControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Account Control is now the only window shown on launch (AccountManager runs hidden
            // in the background), so closing it via the X button needs to actually exit the whole
            // program instead of just hiding this window - otherwise there'd be no way to quit.
            Application.Exit();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Account Control", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (ControlledAccount acc in AccountsView.SelectedObjects)
                    Accounts.Remove(acc);

                AccountsView.SetObjects(Accounts);
                SaveAccounts();
                UpdateStatusSummary();
            }
        }

        // Disabled by request: the program should not touch running Roblox processes at all.
        private void closeRobloxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature has been disabled so the program doesn't interact with running Roblox windows.", "Account Control", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void copyJobIdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AccountsView.SelectedObject != null)
                Clipboard.SetText(((ControlledAccount)AccountsView.SelectedObject).InGameJobId);
        }

        private void AllowExternalConnectionsCB_CheckedChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("AllowExternalConnections", AllowExternalConnectionsCB.Checked ? "true" : "false");
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void StartOnLaunch_CheckedChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("StartOnLaunch", StartOnLaunch.Checked ? "true" : "false");
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void RelaunchDelayNumber_ValueChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("RelaunchDelay", RelaunchDelayNumber.Value.ToString());
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void LauncherDelayNumber_ValueChanged(object sender, EventArgs e)
        {
            AutoRelaunchTimer.Interval = (int)LauncherDelayNumber.Value * 1000;

            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("LauncherDelay", LauncherDelayNumber.Value.ToString());
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void PortNumber_ValueChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("NexusPort", PortNumber.Value.ToString());
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        // Disabled by request: the program should not touch running Roblox processes at all
        // (kill, minimize, force-close) outside of the Auto Re-join teleport flow in
        // AccountManager.cs, which talks to the game via Nexus.lua rather than the OS process.
        private void MinimizeRoblox_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature has been disabled so the program doesn't interact with running Roblox windows.", "Account Control", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CloseRoblox_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature has been disabled so the program doesn't interact with running Roblox windows.", "Account Control", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MinimzeTimer_Tick(object sender, EventArgs e)
        {
            // Intentionally does nothing - see disabled note above.
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            // Intentionally does nothing - see disabled note above.
        }

        private void AutoCloseCB_CheckedChanged(object sender, EventArgs e)
        {
            CloseTimer.Enabled = AutoCloseCB.Checked;

            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("AutoCloseEnabled", AutoCloseCB.Checked ? "true" : "false");
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void InternetCheckCB_CheckedChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("InternetCheck", InternetCheckCB.Checked ? "true" : "false");
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void UsePresenceCB_CheckedChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("UsePresence", UsePresenceCB.Checked ? "true" : "false");
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void AutoMinIntervalNum_ValueChanged(object sender, EventArgs e)
        {
            MinimzeTimer.Interval = (int)AutoMinIntervalNum.Value * 1000;

            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("AutoMinimizeInterval", AutoMinIntervalNum.Value.ToString());
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void AutoCloseIntervalNum_ValueChanged(object sender, EventArgs e)
        {
            CloseTimer.Interval = AutoCloseType.SelectedIndex == 0 ? 3000 : (int)AutoCloseIntervalNum.Value * 60 * 1000;

            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("AutoCloseInterval", AutoCloseIntervalNum.Value.ToString());
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void MaxInstancesNum_ValueChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("MaxInstances", ((int)MaxInstancesNum.Value).ToString());
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private void AutoCloseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            CloseTimer.Interval = AutoCloseType.SelectedIndex == 0 ? 3000 : (int)AutoCloseIntervalNum.Value * 60 * 1000; // Per Instance = 0 | Global = 1

            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("AutoCloseType", AutoCloseType.SelectedIndex.ToString());
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        #endregion

        #region Themes

        public void ApplyTheme()
        {
            BackColor = ThemeEditor.FormsBackground;
            ForeColor = ThemeEditor.FormsForeground;

            if (AccountsView.BackColor != ThemeEditor.AccountBackground || AccountsView.ForeColor != ThemeEditor.AccountForeground)
            {
                AccountsView.BackColor = ThemeEditor.AccountBackground;
                AccountsView.ForeColor = ThemeEditor.AccountForeground;

                AccountsView.BuildList(true);
                AccountsView.BuildGroups();
            }

            ApplyTheme(Controls);
        }

        public void ApplyTheme(Control.ControlCollection _Controls)
        {
            foreach (Control control in _Controls)
            {
                if (control is Button || control is CheckBox)
                {
                    if (control is Button)
                    {
                        Button b = control as Button;
                        b.FlatStyle = ThemeEditor.ButtonStyle;
                        b.FlatAppearance.BorderColor = ThemeEditor.ButtonsBorder;
                    }

                    if (!(control is CheckBox)) control.BackColor = ThemeEditor.ButtonsBackground;
                    control.ForeColor = ThemeEditor.ButtonsForeground;
                }
                else if (control is TextBox || control is RichTextBox)
                {
                    if (control is Classes.BorderedTextBox)
                    {
                        Classes.BorderedTextBox b = control as Classes.BorderedTextBox;
                        b.BorderColor = ThemeEditor.TextBoxesBorder;
                    }

                    if (control is Classes.BorderedRichTextBox)
                    {
                        Classes.BorderedRichTextBox b = control as Classes.BorderedRichTextBox;
                        b.BorderColor = ThemeEditor.TextBoxesBorder;
                    }

                    control.BackColor = ThemeEditor.TextBoxesBackground;
                    control.ForeColor = ThemeEditor.TextBoxesForeground;
                }
                else if (control is Label)
                {
                    control.BackColor = ThemeEditor.LabelTransparent ? Color.Transparent : ThemeEditor.LabelBackground;
                    control.ForeColor = ThemeEditor.LabelForeground;
                }
                else if (control is ListBox || control is ObjectListView)
                {
                    if (control is ObjectListView view) view.HeaderStyle = ThemeEditor.ShowHeaders ? ColumnHeaderStyle.Clickable : ColumnHeaderStyle.None;
                    control.BackColor = ThemeEditor.ButtonsBackground;
                    control.ForeColor = ThemeEditor.ButtonsForeground;
                }
                else if (control is TabPage)
                {
                    ApplyTheme(control.Controls);

                    control.BackColor = ThemeEditor.ButtonsBackground;
                    control.ForeColor = ThemeEditor.ButtonsForeground;
                }
                else if (control is FastColoredTextBoxNS.FastColoredTextBox)
                    control.ForeColor = Color.Black;
                else if (control is FlowLayoutPanel || control is Panel || control is TabControl)
                    ApplyTheme(control.Controls);
            }
        }

        #endregion

        #region Methods

        // Disabled by request: no longer killing Roblox processes automatically. Left as a no-op
        // rather than removed in case anything still calls it.
        private void ClearDeadProcesses()
        {
        }

        #endregion
    }
}