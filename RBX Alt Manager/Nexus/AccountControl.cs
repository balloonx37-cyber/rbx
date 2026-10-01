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
using System.Threading;
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
        private readonly object AutoRejoinTimerLock = new object();

        // Int, not bool: Interlocked.CompareExchange needs a type it has an overload for, and
        // this needs to be checked-and-set atomically (see AutoRejoinTimer_Elapsed) - a plain
        // volatile bool only guarantees visibility across threads, not that two threads can't
        // both read it as false before either one sets it to true. 0 = not running, 1 = running.
        private int AutoRejoinCheckInProgress = 0;

        public AccountControl()
        {
            Instance = this;

            // SetDarkBar reads ThemeEditor.FormsBackground for the window border color -
            // without loading the theme first, that field is still stuck on its class-level
            // default (SystemColors.Control, a light gray) whenever this form's constructor
            // runs before AccountManager's does. LoadTheme() is idempotent (ThemeIni ??= ...),
            // so calling it again here if AccountManager already loaded it is harmless.
            ThemeEditor.LoadTheme();

            AccountManager.SetDarkBar(Handle);

            InitializeComponent();
            this.Rescale();

            ShowTabPage(ControlPage, ControlPageButton);
            HeaderPanel_Resize(this, EventArgs.Empty);
        }

        // ACTabs has no Dock/Anchor of its own - its Location/Size are fully recomputed here
        // on every resize of HeaderPanel (which does have Dock=Fill, so it always matches the
        // form). ACTabs is made HeaderHeight pixels taller than HeaderPanel and shifted up by
        // that same amount, so its native tab header row (which NBTabControl reserves space
        // for regardless of the owner-draw rendering issues elsewhere) sits above y=0 and
        // gets clipped by HeaderPanel's bounds - only the tab body (now exactly HeaderPanel's
        // size) remains visible. Recomputing on every resize (rather than a fixed offset) is
        // what keeps this correct as the window is resized/maximized.
        private void HeaderPanel_Resize(object sender, EventArgs e)
        {
            int HeaderHeight = ACTabs.ItemSize.Height > 0 ? ACTabs.ItemSize.Height + 4 : 24;

            ACTabs.Location = new Point(0, -HeaderHeight);
            ACTabs.Size = new Size(HeaderPanel.Width, HeaderPanel.Height + HeaderHeight);
        }

        // Replaces ACTabs' native tab header (pushed off-screen in the designer - see
        // HeaderPanel's comment) since that header could not be made to reliably render its
        // captions in this app - a WM_DRAWITEM-based repaint never fired once inside this
        // form's real control tree, for reasons that didn't reproduce in isolation. Driving
        // page switches from ordinary button Click handlers side-steps that entirely: no
        // owner-draw, no WM_DRAWITEM, just SelectedTab plus manually highlighting the active
        // button so the current page is still visually obvious.
        private void ShowTabPage(TabPage Page, Button ActiveButton)
        {
            ACTabs.SelectedTab = Page;

            foreach (Control control in TabButtonsPanel.Controls)
                if (control is Button b)
                    b.FlatStyle = b == ActiveButton ? FlatStyle.Flat : ThemeEditor.ButtonStyle;
        }

        private void TabNavButton_Click(object sender, EventArgs e)
        {
            if (sender == ControlPageButton) ShowTabPage(ControlPage, ControlPageButton);
            else if (sender == SettingsTabButton) ShowTabPage(SettingsTab, SettingsTabButton);
            else if (sender == WebControlTabButton) ShowTabPage(WebControlTab, WebControlTabButton);
            else if (sender == HelpPageButton) ShowTabPage(HelpPage, HelpPageButton);
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

                // Opt-in via AutoAdoptRejoinJobIdCB (Settings tab): if some other account
                // currently has Auto Re-join on, have this brand-new account join the
                // SAME Job ID and start Auto Re-join too, instead of sitting idle until the
                // user configures it manually. Different accounts can each have Auto Re-join
                // pointed at a different Job ID, so "whichever was turned on most recently"
                // (AutoRejoinEnabledAt) is the tie-breaker when more than one is active.
                // No-op (and no AutoRejoinEnabledAt stamp) if nothing currently has it on.
                if (AccountManager.AccountControl.Get<bool>("AutoAdoptRejoinJobId"))
                {
                    ControlledAccount Newest = Accounts
                        .Where(a => a.AutoRejoin && !string.IsNullOrEmpty(a.AutoRejoinJobId))
                        .OrderByDescending(a => a.AutoRejoinEnabledAt)
                        .FirstOrDefault();

                    if (Newest != null)
                    {
                        newAccount.AutoRejoin = true;
                        newAccount.AutoRejoinJobId = Newest.AutoRejoinJobId;
                        newAccount.AutoRejoinEnabledAt = Newest.AutoRejoinEnabledAt;
                    }
                }

                Accounts.Add(newAccount);
                Utilities.InvokeIfRequired(Instance, () =>
                {
                    AccountsView.AddObject(newAccount);
                    SaveAccounts();
                    UpdateStatusSummary();
                });

                if (newAccount.AutoRejoin)
                    EnsureAutoRejoinTimerRunning();

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

            // /relay is this server acting as a hub for OTHER machines (and this same machine,
            // via RelayClient below) to report their Accounts list and receive teleport
            // commands - see RelayHub.cs for the full protocol. Only relevant when this
            // machine is the one a shared Web Control link points at.
            Server.AddWebSocketService<RelayServerBehavior>("/relay");

            Server.OnGet += (sender, e) =>
            {
                string path = e.Request.Url.AbsolutePath;

                if (path == "/Nexus.lua")
                {
                    ServeNexusLua(e);
                    return;
                }

                if (path == "/control")
                {
                    ServeWebControlPage(e);
                    return;
                }

                if (path == "/control/api/accounts")
                {
                    ServeWebControlAccounts(e);
                    return;
                }

                e.Response.StatusCode = 404;
            };

            Server.OnPost += (sender, e) =>
            {
                if (e.Request.Url.AbsolutePath == "/control/api/teleport")
                {
                    HandleWebControlTeleport(e);
                    return;
                }

                if (e.Request.Url.AbsolutePath == "/control/api/autorejoin")
                {
                    HandleWebControlAutoRejoin(e);
                    return;
                }

                e.Response.StatusCode = 404;
            };

            Server.Start();

            Process.GetCurrentProcess().WaitForExit();
        }

        private void ServeNexusLua(HttpRequestEventArgs e) // e.Request/e.Response are WebSocketSharp.Net types
        {
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
        }

        // Random shared secret required by every /control endpoint. Generated once and
        // persisted, since AllowExternalConnections can expose this same HttpServer to the
        // whole LAN (or further, if port-forwarded) - without this, anyone who can reach the
        // port could list accounts and issue teleport commands with no login of any kind.
        // Kept as a simple shared token (not a full user/permission system) because it
        // matches the existing trust model of this feature: whoever holds this token has the
        // same power as someone sitting at the desktop app's Account Control window.
        internal static string EnsureWebControlToken()
        {
            string Token = AccountManager.AccountControl.Get("WebControlToken");

            if (!string.IsNullOrEmpty(Token)) return Token;

            Token = GenerateWebControlToken();
            AccountManager.AccountControl.Set("WebControlToken", Token);
            AccountManager.IniSettings.Save("RAMSettings.ini");

            return Token;
        }

        internal static string GenerateWebControlToken()
        {
            byte[] Bytes = new byte[24];

            using (var Rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                Rng.GetBytes(Bytes);

            // Base64Url (no padding, no +/ characters) so the token drops cleanly into a URL
            // query string without needing to be percent-encoded.
            return Convert.ToBase64String(Bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private bool IsWebControlAuthorized(HttpRequestEventArgs e)
        {
            string Expected = AccountManager.AccountControl.Get("WebControlToken");

            if (string.IsNullOrEmpty(Expected)) return false;

            string Provided = e.Request.Headers["X-Control-Token"];

            if (string.IsNullOrEmpty(Provided))
                Provided = e.Request.QueryString["token"];

            return !string.IsNullOrEmpty(Provided) && Provided == Expected;
        }

        private void WriteJson(HttpRequestEventArgs e, object Value)
        {
            byte[] Content = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Value));

            e.Response.ContentType = "application/json";
            e.Response.ContentLength64 = Content.LongLength;
            e.Response.Close(Content, true);
        }

        private void ServeWebControlPage(HttpRequestEventArgs e)
        {
            byte[] Content = System.Text.Encoding.UTF8.GetBytes(WebControlPage.Html);

            e.Response.ContentType = "text/html; charset=utf-8";
            e.Response.ContentLength64 = Content.LongLength;
            e.Response.Close(Content, true);
        }

        private void ServeWebControlAccounts(HttpRequestEventArgs e)
        {
            if (!IsWebControlAuthorized(e))
            {
                e.Response.StatusCode = 401;
                return;
            }

            // RelayHub.GetAllAccounts() includes this same machine's own accounts too, since
            // enabling relay mode (see RelayCheckbox_CheckedChanged) always connects a
            // RelayClient back to this machine's own /relay alongside any other machines - so
            // once relay mode is on, this is a strict superset of the plain Accounts list
            // below, just with a "machine" field added to every row.
            var Result = RelayManager.IsEnabled
                ? RelayHub.GetAllAccounts().Select(a => new
                {
                    machine = a.Machine,
                    username = a.Username,
                    status = a.Status,
                    placeId = a.PlaceId,
                    placeName = a.PlaceName,
                    jobId = a.JobId,
                    players = a.Players,
                    maxPlayers = a.MaxPlayers,
                    autoRejoin = a.AutoRejoin,
                    autoRejoinJobId = a.AutoRejoinJobId
                }).ToList<object>()
                : Accounts.Select(a => new
                {
                    machine = (string)null,
                    username = a.Username,
                    status = a.Status.ToString(),
                    placeId = a.PlaceId,
                    placeName = a.PlaceName,
                    jobId = a.InGameJobId,
                    players = a.PlayerCount,
                    maxPlayers = a.MaxPlayers,
                    autoRejoin = a.AutoRejoin,
                    autoRejoinJobId = a.AutoRejoinJobId
                }).ToList<object>();

            WriteJson(e, Result);
        }

        private void HandleWebControlTeleport(HttpRequestEventArgs e)
        {
            if (!IsWebControlAuthorized(e))
            {
                e.Response.StatusCode = 401;
                return;
            }

            string Body;

            using (var Reader = new StreamReader(e.Request.InputStream, System.Text.Encoding.UTF8))
                Body = Reader.ReadToEnd();

            string Username = null;
            string JobId = null;
            string Machine = null;

            try
            {
                var Payload = JsonConvert.DeserializeObject<Dictionary<string, string>>(Body);
                Payload.TryGetValue("username", out Username);
                Payload.TryGetValue("jobId", out JobId);
                Payload.TryGetValue("machine", out Machine);
            }
            catch (Exception ex)
            {
                WriteJson(e, new { success = false, message = $"Invalid request body: {ex.Message}" });
                return;
            }

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(JobId))
            {
                WriteJson(e, new { success = false, message = "username and jobId are both required" });
                return;
            }

            // In relay mode the account could belong to a different machine entirely - route
            // the command over that machine's /relay connection instead of handling it here.
            // Falls through to the plain local-only path below if a machine wasn't specified
            // (e.g. an older client page) or relay mode isn't on.
            if (RelayManager.IsEnabled && !string.IsNullOrEmpty(Machine))
            {
                // TryTeleport's own callback fires asynchronously (it waits on the target
                // machine's response, or a 15s timeout) - block this request thread on it with
                // a wait handle so the HTTP response is written while the request is still
                // open, the same way the local TeleportToServer().Result call below does.
                var ResultReady = new System.Threading.ManualResetEventSlim(false);
                string RelayResult = null;

                bool Routed = RelayHub.TryTeleport(Machine, Username, JobId, Result =>
                {
                    RelayResult = Result;
                    ResultReady.Set();
                });

                if (!Routed)
                {
                    WriteJson(e, new { success = false, message = $"Machine \"{Machine}\" is not currently connected" });
                    return;
                }

                ResultReady.Wait(TimeSpan.FromSeconds(16));

                string FinalResult = RelayResult ?? "ERROR: Timed out waiting for the target machine to respond";

                LogAutoRejoin($"[Web Control] Teleporting {Username} on {Machine} to Job ID {JobId}: {FinalResult}");
                WriteJson(e, new { success = FinalResult.Contains("Success"), message = FinalResult });
                return;
            }

            ControlledAccount ControlledAcc;

            // AccountsLock - see SetAutoRejoin's comment. A Web Control bulk teleport calls this
            // from many HTTP request threads at once.
            lock (AccountsLock)
                ControlledAcc = Accounts.FirstOrDefault(a => a.Username == Username);

            if (ControlledAcc == null)
            {
                WriteJson(e, new { success = false, message = $"No account named {Username} found" });
                return;
            }

            if (ControlledAcc.PlaceId <= 0)
            {
                WriteJson(e, new { success = false, message = "Cannot teleport: Place ID is empty" });
                return;
            }

            Account LinkedAcc = AccountManager.AccountsList.FirstOrDefault(a => a.Username == Username);

            if (LinkedAcc == null)
            {
                WriteJson(e, new { success = false, message = $"Cannot find Account object for {Username}" });
                return;
            }

            // TeleportToServer is async, but this handler runs synchronously on the
            // HttpServer's own request thread (mirrors how the existing WebsocketServer
            // handlers block on their own thread pool) - .Result is safe here since there's
            // no UI-thread SynchronizationContext to deadlock against on this thread.
            string Result;

            try
            {
                Result = LinkedAcc.TeleportToServer(ControlledAcc.PlaceId, JobId).Result;
            }
            catch (Exception ex)
            {
                Result = $"ERROR: {ex.Message}";
            }

            LogAutoRejoin($"[Web Control] Teleporting {Username} to Job ID {JobId}: {Result}");

            WriteJson(e, new { success = Result.Contains("Success"), message = Result });
        }

        private class AutoRejoinRequest
        {
            public string Username;
            public string Machine;
            public string JobId;
            public bool Enabled;
        }

        private void HandleWebControlAutoRejoin(HttpRequestEventArgs e)
        {
            if (!IsWebControlAuthorized(e))
            {
                e.Response.StatusCode = 401;
                return;
            }

            string Body;

            using (var Reader = new StreamReader(e.Request.InputStream, System.Text.Encoding.UTF8))
                Body = Reader.ReadToEnd();

            string Username = null;
            string Machine = null;
            string JobId = null;
            bool Enabled = false;

            try
            {
                var Payload = JsonConvert.DeserializeObject<AutoRejoinRequest>(Body);

                Username = Payload.Username;
                Machine = Payload.Machine;
                JobId = Payload.JobId;
                Enabled = Payload.Enabled;
            }
            catch (Exception ex)
            {
                WriteJson(e, new { success = false, message = $"Invalid request body: {ex.Message}" });
                return;
            }

            if (string.IsNullOrEmpty(Username))
            {
                WriteJson(e, new { success = false, message = "username is required" });
                return;
            }

            if (Enabled && string.IsNullOrEmpty(JobId))
            {
                WriteJson(e, new { success = false, message = "jobId is required to enable Auto Re-join" });
                return;
            }

            // Same routing split as HandleWebControlTeleport - a relay-connected account could
            // belong to a different machine than the one actually serving this HTTP request.
            if (RelayManager.IsEnabled && !string.IsNullOrEmpty(Machine))
            {
                var ResultReady = new System.Threading.ManualResetEventSlim(false);
                string RelayResult = null;

                bool Routed = RelayHub.TrySetAutoRejoin(Machine, Username, Enabled, JobId, Result =>
                {
                    RelayResult = Result;
                    ResultReady.Set();
                });

                if (!Routed)
                {
                    WriteJson(e, new { success = false, message = $"Machine \"{Machine}\" is not currently connected" });
                    return;
                }

                ResultReady.Wait(TimeSpan.FromSeconds(16));

                string FinalResult = RelayResult ?? "ERROR: Timed out waiting for the target machine to respond";

                LogAutoRejoin($"[Web Control] Setting Auto Re-join for {Username} on {Machine} to {Enabled}: {FinalResult}");
                WriteJson(e, new { success = FinalResult.Contains("Success"), message = FinalResult });
                return;
            }

            string LocalResult = SetAutoRejoin(Username, Enabled, JobId);

            LogAutoRejoin($"[Web Control] Setting Auto Re-join for {Username} to {Enabled}: {LocalResult}");
            WriteJson(e, new { success = LocalResult.Contains("Success"), message = LocalResult });
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

            // Load the saved Accounts list BEFORE starting the websocket listener below.
            // OpenServer() can start accepting Nexus connections (Roblox clients reconnect
            // almost immediately) while LoadAccounts() replaces the Accounts list wholesale
            // (Accounts = JsonConvert.DeserializeObject<...>(JSON)) - any ControlledAccount
            // that GetOrAddAccount created for an early connection in that window would get
            // silently discarded when the list reference is replaced, leaving an orphaned
            // "ghost" row once that same account reconnects and gets Auto-added again as if
            // new. Loading first means the listener only ever mutates the final list.
            SettingsLoaded = true;

            LoadAccounts();

            // Make sure a token exists before OpenServer starts accepting /control requests
            // below, so the very first request never race-hits a not-yet-generated token.
            EnsureWebControlToken();

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
            WebControlTokenBox.Text = AccountManager.AccountControl.Get("WebControlToken");
            PortNumber.Value = AccountManager.AccountControl.Get<decimal>("NexusPort");

            // Populated before the checkboxes below are restored, since checking either one
            // triggers its CheckedChanged handler immediately (SettingsLoaded is already true
            // by this point) and both handlers read these text values right away.
            NgrokAuthTokenBox.Text = AccountManager.AccountControl.Get("NgrokAuthToken");
            RelayUrlBox.Text = AccountManager.AccountControl.Get("RelayUrl");
            RelayTokenBox.Text = AccountManager.AccountControl.Get("RelayToken");
            RelayMachineBox.Text = AccountManager.AccountControl.Get("RelayMachine") ?? Environment.MachineName;

            // Restoring these as "checked" (if they were last left on) re-triggers their
            // CheckedChanged handlers, which is what actually (re)starts ngrok/the relay
            // connection on launch - this mirrors AllowExternalConnectionsCB's own restore
            // above, just with handlers that do more than persist a setting.
            PublicAccessCB.Checked = AccountManager.AccountControl.Get<bool>("PublicAccessEnabled");
            RelayEnabledCB.Checked = AccountManager.AccountControl.Get<bool>("RelayEnabled");
            RelaunchDelayNumber.Value = AccountManager.AccountControl.Get<decimal>("RelaunchDelay");
            LauncherDelayNumber.Value = AccountManager.AccountControl.Get<decimal>("LauncherDelayNumber");
            AutoAdoptRejoinJobIdCB.Checked = AccountManager.AccountControl.Get<bool>("AutoAdoptRejoinJobId");
            AutoMinimizeCB.Checked = AccountManager.AccountControl.Get<bool>("AutoMinimizeEnabled");
            AutoCloseCB.Checked = AccountManager.AccountControl.Get<bool>("AutoCloseEnabled");
            InternetCheckCB.Checked = AccountManager.AccountControl.Get<bool>("InternetCheck");
            UsePresenceCB.Checked = AccountManager.AccountControl.Get<bool>("UsePresence");
            AutoMinIntervalNum.Value = Math.Max(Math.Min(AccountManager.AccountControl.Get<decimal>("AutoMinimizeInterval"), AutoMinIntervalNum.Minimum), AutoMinIntervalNum.Maximum);
            AutoCloseIntervalNum.Value = Math.Max(Math.Min(AccountManager.AccountControl.Get<decimal>("AutoCloseInterval"), AutoCloseIntervalNum.Maximum), AutoCloseIntervalNum.Minimum);
            MaxInstancesNum.Value = Math.Max(Math.Min(AccountManager.AccountControl.Get<int>("MaxInstances"), MaxInstancesNum.Maximum), MaxInstancesNum.Minimum);
            AutoCloseType.SelectedIndex = AccountManager.AccountControl.Get<int>("AutoCloseType");

            VersionLabel.Text = $"v{Utilities.CurrentVersion}";

            // Checked here (independent of AccountManager's own startup check) so the button
            // reflects reality even if "CheckForUpdates" is off or the main form's check
            // already ran and was dismissed - this is a manual, always-available way to see/
            // grab the latest release without digging through Settings.
            if (AccountManager.General.Get<bool>("CheckForUpdates"))
            {
                Task.Run(() =>
                {
                    (bool HasUpdate, string LatestVersion) = Utilities.CheckForUpdate();

                    if (!HasUpdate) return;

                    this.InvokeIfRequired(() =>
                    {
                        UpdateButton.Text = $"Update to {LatestVersion}";
                        // VersionLabel is left-anchored and UpdateButton is right-anchored, so
                        // they no longer collide once the button auto-sizes to fit
                        // "Update to X.X.X.X" - both stay visible at the same time now.
                        UpdateButton.Visible = true;
                    });
                });
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            // Plain MessageBox, not YesNoPrompt: YesNoPrompt can be dismissed with "don't show
            // again" and its answer gets remembered in RAMSettings.ini keyed by the exact prompt
            // text - if anyone ever answers No with that box checked, every future click of this
            // button would silently do nothing forever with zero UI shown.
            if (MessageBox.Show(this, "Would you like to update now?", "An update is available", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Utilities.TriggerUpdate();
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
            {
                account.AutoRejoin = AutoRejoinCheckbox.Checked;

                // Stamped on enable only, so AutoAdoptRejoinJobIdCB can tell which account's
                // Job ID is the most recently-turned-on one (see GetOrAddAccount).
                if (AutoRejoinCheckbox.Checked)
                    account.AutoRejoinEnabledAt = DateTime.UtcNow;
            }

            SaveAccounts();

            if (AutoRejoinCheckbox.Checked)
                EnsureAutoRejoinTimerRunning();
        }

        private void AutoRejoinJobIdTextBox_Leave(object sender, EventArgs e)
        {
            foreach (ControlledAccount account in AccountsView.SelectedObjects)
                account.AutoRejoinJobId = AutoRejoinJobIdTextBox.Text.Trim();

            SaveAccounts();
        }

        // Opt-in: when ON, a brand-new account that shows up via a Nexus connection (see
        // GetOrAddAccount) automatically adopts whichever Job ID was most recently turned on
        // for Auto Re-join (if any account currently has it enabled) and starts Auto Re-join
        // itself. When OFF (default), new accounts are left alone exactly like before.
        private void AutoAdoptRejoinJobIdCB_CheckedChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("AutoAdoptRejoinJobId", AutoAdoptRejoinJobIdCB.Checked ? "true" : "false");
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        // Shared by the checkbox handler above and the Web Control /control/api/autorejoin
        // endpoint below - either path needs the periodic check actually running once ANY
        // account has Auto Re-join enabled, not just when toggled from this form's own grid.
        //
        // This no longer touches LogWindow at all (it used to BeginInvoke a Show() here) - Auto
        // Re-join enabling a popup on its own wasn't wanted, and LogAutoRejoin's own AppendLog
        // call into that window turned out to throw once the window's handle went away ("The
        // destination thread no longer exists"), repeating every 12s and contributing to the
        // freeze/failure symptoms seen from the Web Control path. See LogAutoRejoin's comment.
        private void EnsureAutoRejoinTimerRunning()
        {
            // A Web Control bulk action can call this from many HTTP request threads within
            // the same instant (one per account being enabled at once) - without this lock,
            // every one of them could see AutoRejoinTimer == null at the same time and each
            // create+wire its own Timer, leaking the earlier ones (still running, still ticking
            // AutoRejoinTimer_Elapsed every 12s) while only the last one survives in the field.
            // isFirstStart also only comes out true for whichever thread actually wins the
            // creation, so the "trigger first check immediately" Task.Run below only fires once
            // per app run instead of once per account in the bulk batch.
            bool isFirstStart;

            lock (AutoRejoinTimerLock)
            {
                isFirstStart = AutoRejoinTimer == null;

                if (isFirstStart)
                {
                    AutoRejoinTimer = new System.Timers.Timer(12000); // Check every 12 seconds
                    AutoRejoinTimer.Elapsed += AutoRejoinTimer_Elapsed;
                }

                AutoRejoinTimer.Start();
            }

            // No longer auto-shown (LogWindow.GetInstance().Show() used to run here) - per
            // request, enabling Auto Re-join shouldn't pop up a window on its own. LogAutoRejoin
            // below only writes to log.txt now (see its own comment).
            LogAutoRejoin("Auto Re-join enabled. Checking every 12 seconds...");

            if (!isFirstStart) return;

            // Trigger first check immediately. This must happen on a background thread -
            // EnsureAutoRejoinTimerRunning is called directly from AutoRejoinCheckbox_CheckedChanged
            // on the UI thread, and RunAutoRejoinCheckAsync's synchronous prefix (up to its first
            // await) runs TeleportAccount -> TeleportToServer -> ControlledAccount.SendMessage,
            // which calls the WebSocket's blocking Context.WebSocket.Send(). Calling
            // AutoRejoinTimer_Elapsed(null, null) directly here ran that blocking send on the UI
            // thread itself, freezing the window ("Not Responding") whenever the socket send was
            // slow. Task.Run moves that synchronous prefix onto a thread pool thread, matching how
            // every later tick already runs (Timer.Elapsed never fires on the UI thread).
            Task.Run(() => AutoRejoinTimer_Elapsed(null, null));
        }

        // Returns a user-facing result string (mirrors TeleportAccount's own "ERROR: .../
        // Success: ..." convention) so Web Control's HandleWebControlAutoRejoin can relay it
        // straight back to the browser the same way it already does for teleport results.
        internal string SetAutoRejoin(string Username, bool Enabled, string JobId)
        {
            ControlledAccount Acc;

            // AccountsLock (shared with GetOrAddAccount, and the other Accounts access points
            // below) - a Web Control bulk action calls this from many HTTP request threads at
            // once, and without this lock those reads/writes race the UI thread's own
            // removeToolStripMenuItem_Click (Accounts.Remove) and GetOrAddAccount's Accounts.Add
            // on the same underlying List<T>, which isn't safe for concurrent mutation.
            lock (AccountsLock)
            {
                Acc = Accounts.FirstOrDefault(a => a.Username == Username);

                if (Acc == null)
                    return $"ERROR: No account named {Username} found on this machine";

                Acc.AutoRejoin = Enabled;

                if (Enabled)
                {
                    Acc.AutoRejoinJobId = JobId;
                    Acc.AutoRejoinEnabledAt = DateTime.UtcNow;
                }
            }

            SaveAccounts();

            if (Enabled)
                EnsureAutoRejoinTimerRunning();

            return Enabled ? $"Success: Auto Re-join enabled for {Username}" : $"Success: Auto Re-join disabled for {Username}";
        }

        private void AutoRejoinTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Prevent overlapping ticks: if a previous check is still running (e.g. many
            // accounts, slow network), skip this tick instead of piling up on top of it.
            // Interlocked.CompareExchange (not a plain bool check-then-set) because this can
            // now be entered from multiple threads close together - EnsureAutoRejoinTimerRunning
            // calls this directly (via Task.Run) for the very first account enabled in a Web
            // Control bulk batch, while the 12s Timer.Elapsed could legitimately fire around the
            // same moment too. Two threads both reading AutoRejoinCheckInProgress as "not in
            // progress" before either writes "in progress" would let two RunAutoRejoinCheckAsync
            // batches run at once, doubling up every teleport/SendMessage in that batch.
            // CompareExchange(ref, 1, 0) atomically does both the read and the write in one step
            // and only one caller can ever see the pre-change value of 0.
            if (Interlocked.CompareExchange(ref AutoRejoinCheckInProgress, 1, 0) != 0) return;

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
                // Snapshot under AccountsLock (see SetAutoRejoin's comment) and release it
                // immediately - ToList() copies out everything this loop needs, so the lock
                // doesn't need to stay held while the actual teleports run below.
                List<ControlledAccount> targets;

                lock (AccountsLock)
                    targets = Accounts.Where(a => a.AutoRejoin && !string.IsNullOrEmpty(a.AutoRejoinJobId)).ToList();

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
                Interlocked.Exchange(ref AutoRejoinCheckInProgress, 0);
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

        // Writes to the regular log4net logger (log.txt) only. This used to also forward into
        // LogWindow (a separate "Auto Re-join Logs" popup) via AppendLog, which did two bad
        // things: it auto-showed that window every time Auto Re-join was turned on (removed per
        // request - see EnsureAutoRejoinTimerRunning), and AppendLog's Control.Invoke is a
        // blocking cross-thread call that threw "The destination thread no longer exists" once
        // that window's handle went away - LogAutoRejoin runs on every single auto-rejoin check
        // (every 12s, from a background thread), so that exception/stall repeated continuously
        // once it started, which is what the lingering freeze/"0/2 succeeded" symptoms traced
        // back to. log.txt already captures everything this call needs to report.
        private static void LogAutoRejoin(string message, bool isError = false)
        {
            string tagged = $"[Auto Re-join] {message}";

            if (isError) Program.Logger.Error(tagged);
            else Program.Logger.Info(tagged);
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
            // Both are no-ops if never started - avoids leaving an orphaned ngrok.exe process
            // or a reconnect loop running after the app itself has exited.
            NgrokManager.Stop();
            RelayManager.Disable();

            // Account Control is now the only window shown on launch (AccountManager runs hidden
            // in the background), so closing it via the X button needs to actually exit the whole
            // program instead of just hiding this window - otherwise there'd be no way to quit.
            Application.Exit();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Account Control", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // AccountsLock - see SetAutoRejoin's comment. Removing here races any Web
                // Control request thread that's mid-read of Accounts (e.g. a bulk action still
                // in flight for a row the user is deleting at the same time).
                lock (AccountsLock)
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

        private string BuildWebControlUrl()
        {
            string Token = AccountManager.AccountControl.Get("WebControlToken");
            string Port = AccountManager.AccountControl.Get("NexusPort");

            // ngrok's public URL already resolves from anywhere on the internet, so it takes
            // priority over the LAN-IP/localhost logic below the moment it's available.
            if (!string.IsNullOrEmpty(NgrokPublicUrl))
                return $"{NgrokPublicUrl}/control?token={Token}";

            // Only bother resolving a LAN IP when external connections are actually allowed -
            // otherwise the server only accepts loopback connections (see OpenServer's
            // IPAddress.Loopback branch) and a LAN IP link here would just fail to connect.
            string Host = "localhost";

            if (AccountManager.AccountControl.Get<bool>("AllowExternalConnections"))
            {
                try
                {
                    Host = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                        .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        ?.ToString() ?? "localhost";
                }
                catch { /* fall back to localhost below */ }
            }

            return $"http://{Host}:{Port}/control?token={Token}";
        }

        private void CopyWebControlUrlButton_Click(object sender, EventArgs e)
        {
            string Url = BuildWebControlUrl();

            Clipboard.SetText(Url);

            MessageBox.Show(this, $"Copied to clipboard:\n{Url}\n\nAnyone with this link can view and teleport your connected accounts. Only share it with people you trust.", "Web Control URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RegenerateTokenButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "This invalidates any web control links you've already shared. Continue?", "Regenerate Token", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            string NewToken = GenerateWebControlToken();

            AccountManager.AccountControl.Set("WebControlToken", NewToken);
            AccountManager.IniSettings.Save("RAMSettings.ini");

            WebControlTokenBox.Text = NewToken;
        }

        // Set once ngrok reports a public URL (see PublicAccessCB_CheckedChanged) -
        // BuildWebControlUrl above prefers this over the LAN-IP/localhost link whenever it's
        // non-null, since it's reachable from anywhere, not just this network.
        private string NgrokPublicUrl;

        private void NgrokAuthTokenBox_TextChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("NgrokAuthToken", NgrokAuthTokenBox.Text);
            AccountManager.IniSettings.Save("RAMSettings.ini");
        }

        private async void PublicAccessCB_CheckedChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("PublicAccessEnabled", PublicAccessCB.Checked ? "true" : "false");
            AccountManager.IniSettings.Save("RAMSettings.ini");

            if (!PublicAccessCB.Checked)
            {
                NgrokManager.Stop();
                NgrokPublicUrl = null;
                PublicAccessStatusLabel.Text = "";
                return;
            }

            string Token = NgrokAuthTokenBox.Text;

            if (string.IsNullOrWhiteSpace(Token))
            {
                MessageBox.Show(this, "Enter an ngrok auth token first.\n\nGet a free one at https://dashboard.ngrok.com/get-started/your-authtoken", "Public Access", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PublicAccessCB.Checked = false;
                return;
            }

            if (!int.TryParse(AccountManager.AccountControl.Get("NexusPort"), out int Port))
            {
                PublicAccessCB.Checked = false;
                return;
            }

            PublicAccessCB.Enabled = false;
            PublicAccessStatusLabel.Text = "Starting ngrok...";

            try
            {
                NgrokPublicUrl = await NgrokManager.StartAsync(Port, Token);
                PublicAccessStatusLabel.Text = $"Public URL: {NgrokPublicUrl}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to start ngrok: {ex.Message}", "Public Access", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PublicAccessStatusLabel.Text = "";
                PublicAccessCB.Checked = false;
            }
            finally
            {
                PublicAccessCB.Enabled = true;
            }
        }

        private void RelayEnabledCB_CheckedChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("RelayEnabled", RelayEnabledCB.Checked ? "true" : "false");
            AccountManager.IniSettings.Save("RAMSettings.ini");

            if (!RelayEnabledCB.Checked)
            {
                RelayManager.Disable();
                RelayStatusLabel.Text = "";
                return;
            }

            if (string.IsNullOrWhiteSpace(RelayUrlBox.Text) || string.IsNullOrWhiteSpace(RelayTokenBox.Text) || string.IsNullOrWhiteSpace(RelayMachineBox.Text))
            {
                MessageBox.Show(this, "Fill in the Relay URL, Relay Token, and this machine's name first.", "Relay", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RelayEnabledCB.Checked = false;
                return;
            }

            string TrimmedUrl = RelayUrlBox.Text.Trim();

            RelayManager.Enable(TrimmedUrl, RelayTokenBox.Text.Trim(), RelayMachineBox.Text.Trim(), Connected =>
                this.InvokeIfRequired(() => RelayStatusLabel.Text = Connected
                    ? $"Connected to {TrimmedUrl}"
                    : $"Reconnecting to {TrimmedUrl}..."));

            RelayStatusLabel.Text = $"Connecting to {TrimmedUrl}...";
        }

        private void RelaySettings_TextChanged(object sender, EventArgs e)
        {
            if (!SettingsLoaded) return;

            AccountManager.AccountControl.Set("RelayUrl", RelayUrlBox.Text);
            AccountManager.AccountControl.Set("RelayToken", RelayTokenBox.Text);
            AccountManager.AccountControl.Set("RelayMachine", RelayMachineBox.Text);
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
            TopStrip.BackColor = ThemeEditor.FormsBackground;

            if (AccountsView.BackColor != ThemeEditor.AccountBackground || AccountsView.ForeColor != ThemeEditor.AccountForeground)
            {
                AccountsView.BackColor = ThemeEditor.AccountBackground;
                AccountsView.ForeColor = ThemeEditor.AccountForeground;

                AccountsView.BuildList(true);
                AccountsView.BuildGroups();
            }

            ApplyTheme(Controls);

            // ApplyTheme runs after ControlForm.Show(), so ACTabs' initial owner-draw pass
            // (OnDrawItem) already painted the tab captions using .NET's default TabPage
            // colors - before the loop above just finished overwriting them with this
            // control's actual theme colors. Property setters like BackColor/ForeColor
            // normally self-invalidate, but that doesn't reach a WM_DRAWITEM-painted header,
            // so without this the tab captions stay stuck showing the pre-theme colors (in
            // practice: dark-on-dark, unreadable) until something else forces a repaint.
            ACTabs.Invalidate();

            // The loop above just reset every nav button's FlatStyle to the theme default via
            // the generic Button branch, wiping out whichever one ShowTabPage had marked
            // active (Flat) - restore that highlight now that theming is done.
            ShowTabPage(ACTabs.SelectedTab, ButtonForTab(ACTabs.SelectedTab));
        }

        private Button ButtonForTab(TabPage Page)
        {
            if (Page == SettingsTab) return SettingsTabButton;
            if (Page == WebControlTab) return WebControlTabButton;
            if (Page == HelpPage) return HelpPageButton;
            return ControlPageButton;
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