using RBX_Alt_Manager.Classes;
using System.Windows.Forms;

namespace RBX_Alt_Manager.Forms
{
    partial class AccountControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccountControl));
            this.ControlsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.AutoRejoinCheckbox = new System.Windows.Forms.CheckBox();
            this.AutoRejoinJobIdLabel = new System.Windows.Forms.Label();
            this.AutoRejoinJobIdTextBox = new RBX_Alt_Manager.Classes.BorderedTextBox();
            this.AutoAdoptRejoinJobIdCB = new System.Windows.Forms.CheckBox();
            this.ACTabs = new RBX_Alt_Manager.Classes.NBTabControl();
            this.HeaderPanel = new System.Windows.Forms.Panel();
            this.TabButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ControlPageButton = new System.Windows.Forms.Button();
            this.SettingsTabButton = new System.Windows.Forms.Button();
            this.WebControlTabButton = new System.Windows.Forms.Button();
            this.HelpPageButton = new System.Windows.Forms.Button();
            this.TopStrip = new System.Windows.Forms.Panel();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.ControlPage = new System.Windows.Forms.TabPage();
            this.StatusSummaryLabel = new System.Windows.Forms.Label();
            this.CPanel = new System.Windows.Forms.Panel();
            this.AccountsView = new BrightIdeasSoftware.ObjectListView();
            this.cCheckBoxes = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cStatus = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cPlayers = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.StatusRenderer = new BrightIdeasSoftware.MultiImageRenderer();
            this.cUsername = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cJobId = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cPlaceId = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cPlaceName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cAlive = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cMoney = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cBank = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.ACStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyJobIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeRobloxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingsTab = new System.Windows.Forms.TabPage();
            this.SettingsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.StartOnLaunch = new System.Windows.Forms.CheckBox();
            this.AllowExternalConnectionsCB = new System.Windows.Forms.CheckBox();
            this.InternetCheckCB = new System.Windows.Forms.CheckBox();
            this.UsePresenceCB = new System.Windows.Forms.CheckBox();
            this.RLLabel = new System.Windows.Forms.Label();
            this.RelaunchDelayNumber = new System.Windows.Forms.NumericUpDown();
            this.LDLabel = new System.Windows.Forms.Label();
            this.LauncherDelayNumber = new System.Windows.Forms.NumericUpDown();
            this.PortLabel = new System.Windows.Forms.Label();
            this.PortNumber = new System.Windows.Forms.NumericUpDown();
            this.MinimizeRoblox = new System.Windows.Forms.Button();
            this.AutoMinimizeCB = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.AutoMinIntervalNum = new System.Windows.Forms.NumericUpDown();
            this.CloseRoblox = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.AutoCloseType = new System.Windows.Forms.ComboBox();
            this.ACLabel = new System.Windows.Forms.Label();
            this.AutoCloseIntervalNum = new System.Windows.Forms.NumericUpDown();
            this.MaxInstanceLabel = new System.Windows.Forms.Label();
            this.MaxInstancesNum = new System.Windows.Forms.NumericUpDown();
            this.AutoCloseCB = new System.Windows.Forms.CheckBox();
            this.WebControlTab = new System.Windows.Forms.TabPage();
            this.WebControlLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.WebControlLabel = new System.Windows.Forms.Label();
            this.WebControlTokenBox = new System.Windows.Forms.TextBox();
            this.CopyWebControlUrlButton = new System.Windows.Forms.Button();
            this.RegenerateTokenButton = new System.Windows.Forms.Button();
            this.PublicAccessCB = new System.Windows.Forms.CheckBox();
            this.NgrokAuthTokenLabel = new System.Windows.Forms.Label();
            this.NgrokAuthTokenBox = new System.Windows.Forms.TextBox();
            this.PublicAccessStatusLabel = new System.Windows.Forms.Label();
            this.RelayLabel = new System.Windows.Forms.Label();
            this.RelayEnabledCB = new System.Windows.Forms.CheckBox();
            this.RelayUrlLabel = new System.Windows.Forms.Label();
            this.RelayUrlBox = new System.Windows.Forms.TextBox();
            this.RelayTokenLabel = new System.Windows.Forms.Label();
            this.RelayTokenBox = new System.Windows.Forms.TextBox();
            this.RelayMachineLabel = new System.Windows.Forms.Label();
            this.RelayMachineBox = new System.Windows.Forms.TextBox();
            this.RelayStatusLabel = new System.Windows.Forms.Label();
            this.HelpPage = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.NexusDocsButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.NexusDL = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.AutoRelaunchTimer = new System.Windows.Forms.Timer(this.components);
            this.MinimzeTimer = new System.Windows.Forms.Timer(this.components);
            this.CloseTimer = new System.Windows.Forms.Timer(this.components);
            this.Helper = new System.Windows.Forms.ToolTip(this.components);
            this.ControlsPanel.SuspendLayout();
            this.TopStrip.SuspendLayout();
            this.HeaderPanel.SuspendLayout();
            this.TabButtonsPanel.SuspendLayout();
            this.ACTabs.SuspendLayout();
            this.ControlPage.SuspendLayout();
            this.CPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AccountsView)).BeginInit();
            this.ACStrip.SuspendLayout();
            this.SettingsTab.SuspendLayout();
            this.SettingsLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RelaunchDelayNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LauncherDelayNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AutoMinIntervalNum)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AutoCloseIntervalNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxInstancesNum)).BeginInit();
            this.WebControlTab.SuspendLayout();
            this.WebControlLayoutPanel.SuspendLayout();
            this.HelpPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // ControlsPanel
            // 
            this.ControlsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ControlsPanel.AutoSize = true;
            this.ControlsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ControlsPanel.Controls.Add(this.AutoRejoinCheckbox);
            this.ControlsPanel.Controls.Add(this.AutoRejoinJobIdLabel);
            this.ControlsPanel.Controls.Add(this.AutoRejoinJobIdTextBox);
            this.ControlsPanel.Controls.Add(this.AutoAdoptRejoinJobIdCB);
            this.ControlsPanel.Location = new System.Drawing.Point(0, 0);
            this.ControlsPanel.Name = "ControlsPanel";
            this.ControlsPanel.Padding = new System.Windows.Forms.Padding(5);
            this.ControlsPanel.Size = new System.Drawing.Size(284, 503);
            this.ControlsPanel.TabIndex = 0;
            //
            // AutoRejoinCheckbox
            //
            this.AutoRejoinCheckbox.AutoSize = true;
            this.ControlsPanel.SetFlowBreak(this.AutoRejoinCheckbox, true);
            this.AutoRejoinCheckbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.AutoRejoinCheckbox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(211)))), ((int)(((byte)(238)))));
            this.AutoRejoinCheckbox.Location = new System.Drawing.Point(8, 108);
            this.AutoRejoinCheckbox.Name = "AutoRejoinCheckbox";
            this.AutoRejoinCheckbox.Size = new System.Drawing.Size(105, 19);
            this.AutoRejoinCheckbox.TabIndex = 20;
            this.AutoRejoinCheckbox.Text = "🔄 Auto Re-join";
            this.Helper.SetToolTip(this.AutoRejoinCheckbox, "Automatically teleport the selected accounts below back to this Job ID if they leave or disconnect");
            this.AutoRejoinCheckbox.UseVisualStyleBackColor = true;
            this.AutoRejoinCheckbox.CheckedChanged += new System.EventHandler(this.AutoRejoinCheckbox_CheckedChanged);
            //
            // AutoRejoinJobIdLabel
            //
            this.AutoRejoinJobIdLabel.AutoSize = true;
            this.AutoRejoinJobIdLabel.Location = new System.Drawing.Point(8, 133);
            this.AutoRejoinJobIdLabel.Margin = new System.Windows.Forms.Padding(3, 5, 9, 0);
            this.AutoRejoinJobIdLabel.Name = "AutoRejoinJobIdLabel";
            this.AutoRejoinJobIdLabel.Size = new System.Drawing.Size(38, 13);
            this.AutoRejoinJobIdLabel.TabIndex = 21;
            this.AutoRejoinJobIdLabel.Text = "Job ID";
            //
            // AutoRejoinJobIdTextBox
            //
            this.AutoRejoinJobIdTextBox.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))), ((int)(((byte)(122)))));
            this.ControlsPanel.SetFlowBreak(this.AutoRejoinJobIdTextBox, true);
            this.AutoRejoinJobIdTextBox.Location = new System.Drawing.Point(68, 131);
            this.AutoRejoinJobIdTextBox.Name = "AutoRejoinJobIdTextBox";
            this.AutoRejoinJobIdTextBox.Size = new System.Drawing.Size(207, 20);
            this.AutoRejoinJobIdTextBox.TabIndex = 22;
            this.Helper.SetToolTip(this.AutoRejoinJobIdTextBox, "Job ID for Auto Re-join. Selected accounts below will be teleported back to this Job ID if they leave or disconnect.");
            this.AutoRejoinJobIdTextBox.Leave += new System.EventHandler(this.AutoRejoinJobIdTextBox_Leave);
            //
            // AutoAdoptRejoinJobIdCB
            //
            this.AutoAdoptRejoinJobIdCB.AutoSize = true;
            this.ControlsPanel.SetFlowBreak(this.AutoAdoptRejoinJobIdCB, true);
            this.AutoAdoptRejoinJobIdCB.Location = new System.Drawing.Point(8, 157);
            this.AutoAdoptRejoinJobIdCB.Name = "AutoAdoptRejoinJobIdCB";
            this.AutoAdoptRejoinJobIdCB.Size = new System.Drawing.Size(268, 30);
            this.AutoAdoptRejoinJobIdCB.TabIndex = 23;
            this.AutoAdoptRejoinJobIdCB.Text = "Auto-join new accounts to the active\r\nAuto Re-join Job ID";
            this.Helper.SetToolTip(this.AutoAdoptRejoinJobIdCB, "When a new account shows up (e.g. a Nexus connection) and some other account currently has Auto Re-join on, automatically set this new account to Auto Re-join the same Job ID. Does nothing if no account currently has Auto Re-join enabled.");
            this.AutoAdoptRejoinJobIdCB.UseVisualStyleBackColor = true;
            this.AutoAdoptRejoinJobIdCB.CheckedChanged += new System.EventHandler(this.AutoAdoptRejoinJobIdCB_CheckedChanged);
            //
            // TopStrip
            //
            // ACTabs is Dock=Fill, so it always claims the entire client area at runtime
            // regardless of the Size/Location set here at design time - any sibling placed
            // at fixed coordinates ends up underneath it and gets painted over (NBTabControl
            // clears its whole bounds every OnPaint). Docking this strip Top and adding it to
            // the form BEFORE ACTabs reserves a slice ACTabs' Fill layout can't claim, so the
            // button/label actually stay on screen instead of being covered.
            this.TopStrip.BackColor = System.Drawing.SystemColors.Control;
            this.TopStrip.Controls.Add(this.VersionLabel);
            this.TopStrip.Controls.Add(this.UpdateButton);
            this.TopStrip.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopStrip.Location = new System.Drawing.Point(0, 0);
            this.TopStrip.Name = "TopStrip";
            this.TopStrip.Size = new System.Drawing.Size(593, 26);
            this.TopStrip.TabIndex = 5;
            //
            // UpdateButton
            //
            this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UpdateButton.AutoSize = true;
            this.UpdateButton.Location = new System.Drawing.Point(510, 2);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.UpdateButton.Size = new System.Drawing.Size(75, 23);
            this.UpdateButton.TabIndex = 2;
            this.UpdateButton.Text = "Update";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Visible = false;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            //
            // VersionLabel
            //
            // Anchored Top|Left (not Top|Right) so it never overlaps UpdateButton, which is
            // also right-anchored and grows wider once its text becomes "Update to X.X.X.X" -
            // the two used to sit side by side at fixed right-anchored offsets and the button's
            // growth would cover this label entirely.
            this.VersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Location = new System.Drawing.Point(8, 6);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(60, 15);
            this.VersionLabel.TabIndex = 4;
            this.VersionLabel.Text = "v0.0.0.0";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TabButtonsPanel
            //
            this.TabButtonsPanel.Controls.Add(this.ControlPageButton);
            this.TabButtonsPanel.Controls.Add(this.SettingsTabButton);
            this.TabButtonsPanel.Controls.Add(this.WebControlTabButton);
            this.TabButtonsPanel.Controls.Add(this.HelpPageButton);
            this.TabButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TabButtonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.TabButtonsPanel.Location = new System.Drawing.Point(0, 26);
            this.TabButtonsPanel.Name = "TabButtonsPanel";
            this.TabButtonsPanel.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.TabButtonsPanel.Size = new System.Drawing.Size(593, 32);
            this.TabButtonsPanel.TabIndex = 6;
            //
            // ControlPageButton
            //
            this.ControlPageButton.Location = new System.Drawing.Point(4, 4);
            this.ControlPageButton.Name = "ControlPageButton";
            this.ControlPageButton.Size = new System.Drawing.Size(100, 24);
            this.ControlPageButton.TabIndex = 0;
            this.ControlPageButton.Text = "Control Panel";
            this.ControlPageButton.UseVisualStyleBackColor = true;
            this.ControlPageButton.Click += new System.EventHandler(this.TabNavButton_Click);
            //
            // SettingsTabButton
            //
            // Hidden by request - Settings is still reachable in code (ShowTabPage/SettingsTab
            // still exist, e.g. for ApplyTheme's bookkeeping) but no longer has a nav button.
            this.SettingsTabButton.Location = new System.Drawing.Point(110, 4);
            this.SettingsTabButton.Name = "SettingsTabButton";
            this.SettingsTabButton.Size = new System.Drawing.Size(100, 24);
            this.SettingsTabButton.TabIndex = 1;
            this.SettingsTabButton.Text = "Settings";
            this.SettingsTabButton.UseVisualStyleBackColor = true;
            this.SettingsTabButton.Visible = false;
            this.SettingsTabButton.Click += new System.EventHandler(this.TabNavButton_Click);
            //
            // WebControlTabButton
            //
            this.WebControlTabButton.Location = new System.Drawing.Point(216, 4);
            this.WebControlTabButton.Name = "WebControlTabButton";
            this.WebControlTabButton.Size = new System.Drawing.Size(100, 24);
            this.WebControlTabButton.TabIndex = 2;
            this.WebControlTabButton.Text = "Web Control";
            this.WebControlTabButton.UseVisualStyleBackColor = true;
            this.WebControlTabButton.Click += new System.EventHandler(this.TabNavButton_Click);
            //
            // HelpPageButton
            //
            // Hidden by request - same as SettingsTabButton above.
            this.HelpPageButton.Location = new System.Drawing.Point(322, 4);
            this.HelpPageButton.Name = "HelpPageButton";
            this.HelpPageButton.Size = new System.Drawing.Size(100, 24);
            this.HelpPageButton.TabIndex = 3;
            this.HelpPageButton.Text = "Help";
            this.HelpPageButton.UseVisualStyleBackColor = true;
            this.HelpPageButton.Visible = false;
            this.HelpPageButton.Click += new System.EventHandler(this.TabNavButton_Click);
            //
            // ACTabs
            //
            // NBTabControl's native tab header row is unreliable to theme/render (see
            // TabNavButton_Click/ShowTabPage in AccountControl.cs for the full story), so it's
            // replaced entirely by TabButtonsPanel's buttons above. The header itself is
            // pushed above HeaderPanel's visible area and clipped by it - HeaderPanel.Resize
            // in AccountControl.cs recomputes ACTabs' Location/Size on every resize (instead
            // of a fixed offset here) so this keeps working as the form/panel resizes.
            this.ACTabs.Controls.Add(this.ControlPage);
            this.ACTabs.Controls.Add(this.SettingsTab);
            this.ACTabs.Controls.Add(this.WebControlTab);
            this.ACTabs.Controls.Add(this.HelpPage);
            this.ACTabs.Name = "ACTabs";
            this.ACTabs.SelectedIndex = 0;
            this.ACTabs.TabIndex = 1;
            //
            // HeaderPanel
            //
            this.HeaderPanel.Controls.Add(this.ACTabs);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.TabIndex = 7;
            this.HeaderPanel.Resize += new System.EventHandler(this.HeaderPanel_Resize);
            // 
            // ControlPage
            //
            this.ControlPage.Controls.Add(this.CPanel);
            this.ControlPage.Controls.Add(this.AccountsView);
            this.ControlPage.Controls.Add(this.StatusSummaryLabel);
            this.ControlPage.Location = new System.Drawing.Point(4, 25);
            this.ControlPage.Name = "ControlPage";
            this.ControlPage.Padding = new System.Windows.Forms.Padding(3);
            this.ControlPage.Size = new System.Drawing.Size(585, 365);
            this.ControlPage.TabIndex = 0;
            this.ControlPage.Text = "Control Panel";
            this.ControlPage.UseVisualStyleBackColor = true;
            //
            // StatusSummaryLabel
            //
            this.StatusSummaryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StatusSummaryLabel.AutoSize = true;
            this.StatusSummaryLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.StatusSummaryLabel.Location = new System.Drawing.Point(8, 344);
            this.StatusSummaryLabel.Name = "StatusSummaryLabel";
            this.StatusSummaryLabel.Size = new System.Drawing.Size(150, 15);
            this.StatusSummaryLabel.TabIndex = 3;
            this.StatusSummaryLabel.Text = "Online: 0  Offline: 0  Total: 0";
            //
            // CPanel
            //
            this.CPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CPanel.AutoScroll = true;
            this.CPanel.Controls.Add(this.ControlsPanel);
            this.CPanel.Location = new System.Drawing.Point(279, 7);
            this.CPanel.Name = "CPanel";
            this.CPanel.Size = new System.Drawing.Size(298, 353);
            this.CPanel.TabIndex = 2;
            //
            // AccountsView
            // 
            this.AccountsView.AllColumns.Add(this.cCheckBoxes);
            this.AccountsView.AllColumns.Add(this.cStatus);
            this.AccountsView.AllColumns.Add(this.cUsername);
            this.AccountsView.AllColumns.Add(this.cJobId);
            this.AccountsView.AllColumns.Add(this.cPlaceId);
            this.AccountsView.AllColumns.Add(this.cPlaceName);
            this.AccountsView.AllColumns.Add(this.cPlayers);
            this.AccountsView.AllColumns.Add(this.cAlive);
            this.AccountsView.AllColumns.Add(this.cMoney);
            this.AccountsView.AllColumns.Add(this.cBank);
            this.AccountsView.AllowColumnReorder = true;
            this.AccountsView.AllowDrop = true;
            this.AccountsView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            // Default ListView border (Fixed3D) renders as a light bevel that doesn't respect
            // the dark theme - it used to blend in next to the native tab header's own light
            // chrome, but stands out now that the header is hidden. None + BorderColor removes
            // that mismatched border entirely.
            this.AccountsView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.AccountsView.CellEditUseWholeCell = false;
            this.AccountsView.CheckBoxes = true;
            this.AccountsView.CheckedAspectName = "IsChecked";
            this.AccountsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cCheckBoxes,
            this.cStatus,
            this.cUsername,
            this.cJobId,
            this.cPlaceId,
            this.cPlaceName,
            this.cPlayers,
            this.cAlive,
            this.cMoney,
            this.cBank});
            this.AccountsView.ContextMenuStrip = this.ACStrip;
            this.AccountsView.Cursor = System.Windows.Forms.Cursors.Default;
            this.AccountsView.FullRowSelect = true;
            this.AccountsView.HideSelection = false;
            this.AccountsView.Location = new System.Drawing.Point(8, 7);
            this.AccountsView.Name = "AccountsView";
            this.AccountsView.ShowGroups = false;
            this.AccountsView.ShowImagesOnSubItems = true;
            this.AccountsView.Size = new System.Drawing.Size(265, 333);
            this.AccountsView.TabIndex = 1;
            this.AccountsView.UseCompatibleStateImageBehavior = false;
            this.AccountsView.UseSubItemCheckBoxes = true;
            this.AccountsView.View = System.Windows.Forms.View.Details;
            this.AccountsView.SelectionChanged += new System.EventHandler(this.AccountsView_SelectionChanged);
            this.AccountsView.DragDrop += new System.Windows.Forms.DragEventHandler(this.AccountsView_DragDrop);
            this.AccountsView.DragOver += new System.Windows.Forms.DragEventHandler(this.AccountsView_DragOver);
            // 
            // cCheckBoxes
            // 
            this.cCheckBoxes.AspectName = "IsChecked";
            this.cCheckBoxes.HeaderCheckBox = true;
            this.cCheckBoxes.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.cCheckBoxes.Text = "";
            this.cCheckBoxes.Width = 20;
            // 
            // cStatus
            // 
            this.cStatus.AspectName = "";
            this.cStatus.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.cStatus.Renderer = this.StatusRenderer;
            this.cStatus.Text = "";
            this.cStatus.Width = 20;
            // 
            // StatusRenderer
            // 
            this.StatusRenderer.ImageName = "offline";
            this.StatusRenderer.MaxNumberImages = 2;
            // 
            // cUsername
            // 
            this.cUsername.AspectName = "Username";
            this.cUsername.Text = "Username";
            this.cUsername.Width = 125;
            // 
            // cJobId
            // 
            this.cJobId.AspectName = "InGameJobId";
            this.cJobId.Text = "Job ID";
            this.cJobId.Width = 89;
            //
            // cPlaceId
            //
            this.cPlaceId.AspectName = "PlaceId";
            this.cPlaceId.Text = "Place ID";
            this.cPlaceId.Width = 89;
            //
            // cPlaceName
            //
            this.cPlaceName.AspectName = "PlaceName";
            this.cPlaceName.Text = "Map";
            this.cPlaceName.Width = 140;
            //
            // cPlayers
            //
            this.cPlayers.AspectName = "ServerPlayers";
            this.cPlayers.Text = "Players";
            this.cPlayers.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.cPlayers.Width = 70;
            //
            // cAlive
            //
            this.cAlive.AspectName = "AliveDisplay";
            this.cAlive.Text = "Alive";
            this.cAlive.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.cAlive.Width = 50;
            //
            // cMoney
            //
            this.cMoney.AspectName = "MoneyDisplay";
            this.cMoney.Text = "Money";
            this.cMoney.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.cMoney.Width = 70;
            //
            // cBank
            //
            this.cBank.AspectName = "BankDisplay";
            this.cBank.Text = "Bank";
            this.cBank.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.cBank.Width = 70;
            //
            // ACStrip
            // 
            this.ACStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyJobIdToolStripMenuItem,
            this.closeRobloxToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.ACStrip.Name = "ACStrip";
            this.ACStrip.Size = new System.Drawing.Size(160, 70);
            //
            // copyJobIdToolStripMenuItem
            //
            this.copyJobIdToolStripMenuItem.Name = "copyJobIdToolStripMenuItem";
            this.copyJobIdToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.copyJobIdToolStripMenuItem.Text = "Copy JobId";
            this.copyJobIdToolStripMenuItem.Click += new System.EventHandler(this.copyJobIdToolStripMenuItem_Click);
            //
            // closeRobloxToolStripMenuItem
            //
            this.closeRobloxToolStripMenuItem.Name = "closeRobloxToolStripMenuItem";
            this.closeRobloxToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.closeRobloxToolStripMenuItem.Text = "Close This Roblox";
            this.closeRobloxToolStripMenuItem.Click += new System.EventHandler(this.closeRobloxToolStripMenuItem_Click);
            //
            // removeToolStripMenuItem
            //
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // SettingsTab
            // 
            this.SettingsTab.Controls.Add(this.SettingsLayoutPanel);
            this.SettingsTab.Location = new System.Drawing.Point(4, 25);
            this.SettingsTab.Name = "SettingsTab";
            this.SettingsTab.Padding = new System.Windows.Forms.Padding(3);
            this.SettingsTab.Size = new System.Drawing.Size(585, 365);
            this.SettingsTab.TabIndex = 3;
            this.SettingsTab.Text = "Settings";
            this.SettingsTab.UseVisualStyleBackColor = true;
            //
            // WebControlTab
            //
            this.WebControlTab.Controls.Add(this.WebControlLayoutPanel);
            this.WebControlTab.Location = new System.Drawing.Point(4, 25);
            this.WebControlTab.Name = "WebControlTab";
            this.WebControlTab.Padding = new System.Windows.Forms.Padding(3);
            this.WebControlTab.Size = new System.Drawing.Size(585, 365);
            this.WebControlTab.TabIndex = 4;
            this.WebControlTab.Text = "Web Control";
            this.WebControlTab.UseVisualStyleBackColor = true;
            //
            // WebControlLayoutPanel
            //
            this.WebControlLayoutPanel.Controls.Add(this.WebControlLabel);
            this.WebControlLayoutPanel.Controls.Add(this.WebControlTokenBox);
            this.WebControlLayoutPanel.Controls.Add(this.CopyWebControlUrlButton);
            this.WebControlLayoutPanel.Controls.Add(this.RegenerateTokenButton);
            this.WebControlLayoutPanel.Controls.Add(this.PublicAccessCB);
            this.WebControlLayoutPanel.Controls.Add(this.NgrokAuthTokenLabel);
            this.WebControlLayoutPanel.Controls.Add(this.NgrokAuthTokenBox);
            this.WebControlLayoutPanel.Controls.Add(this.PublicAccessStatusLabel);
            this.WebControlLayoutPanel.Controls.Add(this.RelayLabel);
            this.WebControlLayoutPanel.Controls.Add(this.RelayEnabledCB);
            this.WebControlLayoutPanel.Controls.Add(this.RelayUrlLabel);
            this.WebControlLayoutPanel.Controls.Add(this.RelayUrlBox);
            this.WebControlLayoutPanel.Controls.Add(this.RelayTokenLabel);
            this.WebControlLayoutPanel.Controls.Add(this.RelayTokenBox);
            this.WebControlLayoutPanel.Controls.Add(this.RelayMachineLabel);
            this.WebControlLayoutPanel.Controls.Add(this.RelayMachineBox);
            this.WebControlLayoutPanel.Controls.Add(this.RelayStatusLabel);
            this.WebControlLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WebControlLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.WebControlLayoutPanel.Name = "WebControlLayoutPanel";
            this.WebControlLayoutPanel.Padding = new System.Windows.Forms.Padding(12);
            this.WebControlLayoutPanel.Size = new System.Drawing.Size(579, 359);
            this.WebControlLayoutPanel.TabIndex = 0;
            //
            // WebControlLabel
            //
            this.WebControlLabel.AutoSize = true;
            this.WebControlLayoutPanel.SetFlowBreak(this.WebControlLabel, true);
            this.WebControlLabel.Location = new System.Drawing.Point(15, 15);
            this.WebControlLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.WebControlLabel.Name = "WebControlLabel";
            this.WebControlLabel.Size = new System.Drawing.Size(120, 13);
            this.WebControlLabel.TabIndex = 0;
            this.WebControlLabel.Text = "Web Control Token";
            //
            // WebControlTokenBox
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.WebControlTokenBox, true);
            this.WebControlTokenBox.Location = new System.Drawing.Point(15, 31);
            this.WebControlTokenBox.Name = "WebControlTokenBox";
            this.WebControlTokenBox.ReadOnly = true;
            this.WebControlTokenBox.Size = new System.Drawing.Size(400, 20);
            this.WebControlTokenBox.TabIndex = 1;
            this.Helper.SetToolTip(this.WebControlTokenBox, "Anyone with this token can view and teleport your connected\r\naccounts from the" +
        " /control web page. Treat it like a password.");
            //
            // CopyWebControlUrlButton
            //
            this.CopyWebControlUrlButton.Location = new System.Drawing.Point(15, 54);
            this.CopyWebControlUrlButton.Name = "CopyWebControlUrlButton";
            this.CopyWebControlUrlButton.Size = new System.Drawing.Size(160, 23);
            this.CopyWebControlUrlButton.TabIndex = 2;
            this.CopyWebControlUrlButton.Text = "Copy Web Control URL";
            this.CopyWebControlUrlButton.UseVisualStyleBackColor = true;
            this.CopyWebControlUrlButton.Click += new System.EventHandler(this.CopyWebControlUrlButton_Click);
            //
            // RegenerateTokenButton
            //
            this.RegenerateTokenButton.Location = new System.Drawing.Point(181, 54);
            this.RegenerateTokenButton.Name = "RegenerateTokenButton";
            this.RegenerateTokenButton.Size = new System.Drawing.Size(120, 23);
            this.RegenerateTokenButton.TabIndex = 3;
            this.RegenerateTokenButton.Text = "Regenerate Token";
            this.RegenerateTokenButton.UseVisualStyleBackColor = true;
            this.RegenerateTokenButton.Click += new System.EventHandler(this.RegenerateTokenButton_Click);
            //
            // PublicAccessCB
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.PublicAccessCB, true);
            this.PublicAccessCB.AutoSize = true;
            this.PublicAccessCB.Location = new System.Drawing.Point(15, 83);
            this.PublicAccessCB.Margin = new System.Windows.Forms.Padding(3, 16, 3, 0);
            this.PublicAccessCB.Name = "PublicAccessCB";
            this.PublicAccessCB.Size = new System.Drawing.Size(190, 17);
            this.PublicAccessCB.TabIndex = 4;
            this.PublicAccessCB.Text = "Enable Public Access (ngrok)";
            this.PublicAccessCB.UseVisualStyleBackColor = true;
            this.Helper.SetToolTip(this.PublicAccessCB, "Makes the Web Control link work from anywhere on the internet,\r\nnot just this network, by tunneling it through ngrok.");
            this.PublicAccessCB.CheckedChanged += new System.EventHandler(this.PublicAccessCB_CheckedChanged);
            //
            // NgrokAuthTokenLabel
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.NgrokAuthTokenLabel, true);
            this.NgrokAuthTokenLabel.AutoSize = true;
            this.NgrokAuthTokenLabel.Location = new System.Drawing.Point(15, 103);
            this.NgrokAuthTokenLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.NgrokAuthTokenLabel.Name = "NgrokAuthTokenLabel";
            this.NgrokAuthTokenLabel.Size = new System.Drawing.Size(160, 13);
            this.NgrokAuthTokenLabel.TabIndex = 5;
            this.NgrokAuthTokenLabel.Text = "ngrok Auth Token";
            //
            // NgrokAuthTokenBox
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.NgrokAuthTokenBox, true);
            this.NgrokAuthTokenBox.Location = new System.Drawing.Point(15, 119);
            this.NgrokAuthTokenBox.Name = "NgrokAuthTokenBox";
            this.NgrokAuthTokenBox.Size = new System.Drawing.Size(400, 20);
            this.NgrokAuthTokenBox.TabIndex = 6;
            this.NgrokAuthTokenBox.UseSystemPasswordChar = true;
            this.Helper.SetToolTip(this.NgrokAuthTokenBox, "Get a free token at https://dashboard.ngrok.com/get-started/your-authtoken");
            this.NgrokAuthTokenBox.TextChanged += new System.EventHandler(this.NgrokAuthTokenBox_TextChanged);
            //
            // PublicAccessStatusLabel
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.PublicAccessStatusLabel, true);
            this.PublicAccessStatusLabel.AutoSize = true;
            this.PublicAccessStatusLabel.Location = new System.Drawing.Point(15, 142);
            this.PublicAccessStatusLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.PublicAccessStatusLabel.Name = "PublicAccessStatusLabel";
            this.PublicAccessStatusLabel.Size = new System.Drawing.Size(0, 13);
            this.PublicAccessStatusLabel.TabIndex = 7;
            //
            // RelayLabel
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.RelayLabel, true);
            this.RelayLabel.AutoSize = true;
            this.RelayLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.RelayLabel.Location = new System.Drawing.Point(15, 160);
            this.RelayLabel.Margin = new System.Windows.Forms.Padding(3, 16, 3, 0);
            this.RelayLabel.Name = "RelayLabel";
            this.RelayLabel.Size = new System.Drawing.Size(220, 15);
            this.RelayLabel.TabIndex = 8;
            this.RelayLabel.Text = "Show multiple machines on one link";
            //
            // RelayEnabledCB
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.RelayEnabledCB, true);
            this.RelayEnabledCB.AutoSize = true;
            this.RelayEnabledCB.Location = new System.Drawing.Point(15, 181);
            this.RelayEnabledCB.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.RelayEnabledCB.Name = "RelayEnabledCB";
            this.RelayEnabledCB.Size = new System.Drawing.Size(220, 17);
            this.RelayEnabledCB.TabIndex = 9;
            this.RelayEnabledCB.Text = "Report this machine to a relay";
            this.RelayEnabledCB.UseVisualStyleBackColor = true;
            this.Helper.SetToolTip(this.RelayEnabledCB, "Connects this machine to a relay server (which can be this\r\nsame machine, or another one) so its accounts show up on the\r\nsame /control link as other connected machines.");
            this.RelayEnabledCB.CheckedChanged += new System.EventHandler(this.RelayEnabledCB_CheckedChanged);
            //
            // RelayUrlLabel
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.RelayUrlLabel, true);
            this.RelayUrlLabel.AutoSize = true;
            this.RelayUrlLabel.Location = new System.Drawing.Point(15, 201);
            this.RelayUrlLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.RelayUrlLabel.Name = "RelayUrlLabel";
            this.RelayUrlLabel.Size = new System.Drawing.Size(100, 13);
            this.RelayUrlLabel.TabIndex = 10;
            this.RelayUrlLabel.Text = "Relay URL";
            //
            // RelayUrlBox
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.RelayUrlBox, true);
            this.RelayUrlBox.Location = new System.Drawing.Point(15, 217);
            this.RelayUrlBox.Name = "RelayUrlBox";
            this.RelayUrlBox.Size = new System.Drawing.Size(400, 20);
            this.RelayUrlBox.TabIndex = 11;
            this.Helper.SetToolTip(this.RelayUrlBox, "The Web Control URL of the machine acting as the relay, e.g.\r\nhttps://abc123.ngrok-free.app");
            this.RelayUrlBox.TextChanged += new System.EventHandler(this.RelaySettings_TextChanged);
            //
            // RelayTokenLabel
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.RelayTokenLabel, true);
            this.RelayTokenLabel.AutoSize = true;
            this.RelayTokenLabel.Location = new System.Drawing.Point(15, 240);
            this.RelayTokenLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.RelayTokenLabel.Name = "RelayTokenLabel";
            this.RelayTokenLabel.Size = new System.Drawing.Size(100, 13);
            this.RelayTokenLabel.TabIndex = 12;
            this.RelayTokenLabel.Text = "Relay Token";
            //
            // RelayTokenBox
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.RelayTokenBox, true);
            this.RelayTokenBox.Location = new System.Drawing.Point(15, 256);
            this.RelayTokenBox.Name = "RelayTokenBox";
            this.RelayTokenBox.Size = new System.Drawing.Size(400, 20);
            this.RelayTokenBox.TabIndex = 13;
            this.Helper.SetToolTip(this.RelayTokenBox, "The Web Control Token shown on the relay machine's own\r\nWeb Control tab.");
            this.RelayTokenBox.TextChanged += new System.EventHandler(this.RelaySettings_TextChanged);
            //
            // RelayMachineLabel
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.RelayMachineLabel, true);
            this.RelayMachineLabel.AutoSize = true;
            this.RelayMachineLabel.Location = new System.Drawing.Point(15, 279);
            this.RelayMachineLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.RelayMachineLabel.Name = "RelayMachineLabel";
            this.RelayMachineLabel.Size = new System.Drawing.Size(150, 13);
            this.RelayMachineLabel.TabIndex = 14;
            this.RelayMachineLabel.Text = "This Machine's Name";
            //
            // RelayMachineBox
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.RelayMachineBox, true);
            this.RelayMachineBox.Location = new System.Drawing.Point(15, 295);
            this.RelayMachineBox.Name = "RelayMachineBox";
            this.RelayMachineBox.Size = new System.Drawing.Size(200, 20);
            this.RelayMachineBox.TabIndex = 15;
            this.Helper.SetToolTip(this.RelayMachineBox, "Shown as the machine name on the shared /control page -\r\npick something that tells accounts on this machine apart\r\nfrom other connected machines.");
            this.RelayMachineBox.TextChanged += new System.EventHandler(this.RelaySettings_TextChanged);
            //
            // RelayStatusLabel
            //
            this.WebControlLayoutPanel.SetFlowBreak(this.RelayStatusLabel, true);
            this.RelayStatusLabel.AutoSize = true;
            this.RelayStatusLabel.Location = new System.Drawing.Point(15, 318);
            this.RelayStatusLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.RelayStatusLabel.Name = "RelayStatusLabel";
            this.RelayStatusLabel.Size = new System.Drawing.Size(0, 13);
            this.RelayStatusLabel.TabIndex = 16;
            //
            // SettingsLayoutPanel
            // 
            this.SettingsLayoutPanel.Controls.Add(this.StartOnLaunch);
            this.SettingsLayoutPanel.Controls.Add(this.AllowExternalConnectionsCB);
            this.SettingsLayoutPanel.Controls.Add(this.InternetCheckCB);
            this.SettingsLayoutPanel.Controls.Add(this.UsePresenceCB);
            this.SettingsLayoutPanel.Controls.Add(this.RLLabel);
            this.SettingsLayoutPanel.Controls.Add(this.RelaunchDelayNumber);
            this.SettingsLayoutPanel.Controls.Add(this.LDLabel);
            this.SettingsLayoutPanel.Controls.Add(this.LauncherDelayNumber);
            this.SettingsLayoutPanel.Controls.Add(this.PortLabel);
            this.SettingsLayoutPanel.Controls.Add(this.PortNumber);
            this.SettingsLayoutPanel.Controls.Add(this.MinimizeRoblox);
            this.SettingsLayoutPanel.Controls.Add(this.AutoMinimizeCB);
            this.SettingsLayoutPanel.Controls.Add(this.label8);
            this.SettingsLayoutPanel.Controls.Add(this.AutoMinIntervalNum);
            this.SettingsLayoutPanel.Controls.Add(this.CloseRoblox);
            this.SettingsLayoutPanel.Controls.Add(this.tableLayoutPanel1);
            this.SettingsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.SettingsLayoutPanel.Name = "SettingsLayoutPanel";
            this.SettingsLayoutPanel.Padding = new System.Windows.Forms.Padding(12);
            this.SettingsLayoutPanel.Size = new System.Drawing.Size(579, 359);
            this.SettingsLayoutPanel.TabIndex = 2;
            // 
            // StartOnLaunch
            // 
            this.StartOnLaunch.AutoSize = true;
            this.SettingsLayoutPanel.SetFlowBreak(this.StartOnLaunch, true);
            this.StartOnLaunch.Location = new System.Drawing.Point(15, 15);
            this.StartOnLaunch.Name = "StartOnLaunch";
            this.StartOnLaunch.Size = new System.Drawing.Size(223, 17);
            this.StartOnLaunch.TabIndex = 15;
            this.StartOnLaunch.Text = "Start Nexus on Account Manager Launch";
            this.StartOnLaunch.UseVisualStyleBackColor = true;
            this.StartOnLaunch.CheckedChanged += new System.EventHandler(this.StartOnLaunch_CheckedChanged);
            // 
            // AllowExternalConnectionsCB
            // 
            this.AllowExternalConnectionsCB.AutoSize = true;
            this.SettingsLayoutPanel.SetFlowBreak(this.AllowExternalConnectionsCB, true);
            this.AllowExternalConnectionsCB.Location = new System.Drawing.Point(15, 38);
            this.AllowExternalConnectionsCB.Name = "AllowExternalConnectionsCB";
            this.AllowExternalConnectionsCB.Size = new System.Drawing.Size(154, 17);
            this.AllowExternalConnectionsCB.TabIndex = 7;
            this.AllowExternalConnectionsCB.Text = "Allow External Connections";
            this.AllowExternalConnectionsCB.UseVisualStyleBackColor = true;
            this.AllowExternalConnectionsCB.CheckedChanged += new System.EventHandler(this.AllowExternalConnectionsCB_CheckedChanged);
            // 
            // InternetCheckCB
            // 
            this.InternetCheckCB.AutoSize = true;
            this.SettingsLayoutPanel.SetFlowBreak(this.InternetCheckCB, true);
            this.InternetCheckCB.Location = new System.Drawing.Point(15, 61);
            this.InternetCheckCB.Name = "InternetCheckCB";
            this.InternetCheckCB.Size = new System.Drawing.Size(184, 17);
            this.InternetCheckCB.TabIndex = 24;
            this.InternetCheckCB.Text = "Check for Internet Before Launch";
            this.InternetCheckCB.UseVisualStyleBackColor = true;
            this.InternetCheckCB.CheckedChanged += new System.EventHandler(this.InternetCheckCB_CheckedChanged);
            // 
            // UsePresenceCB
            // 
            this.UsePresenceCB.AutoSize = true;
            this.UsePresenceCB.Cursor = System.Windows.Forms.Cursors.Help;
            this.SettingsLayoutPanel.SetFlowBreak(this.UsePresenceCB, true);
            this.UsePresenceCB.Location = new System.Drawing.Point(15, 84);
            this.UsePresenceCB.Name = "UsePresenceCB";
            this.UsePresenceCB.Size = new System.Drawing.Size(113, 17);
            this.UsePresenceCB.TabIndex = 25;
            this.UsePresenceCB.Text = "Use Presence API";
            this.Helper.SetToolTip(this.UsePresenceCB, "Uses Roblox\'s presence API to check if an\r\naccount is online instead of having Ne" +
        "xus\r\nconnect to the program when re-launching");
            this.UsePresenceCB.UseVisualStyleBackColor = true;
            this.UsePresenceCB.CheckedChanged += new System.EventHandler(this.UsePresenceCB_CheckedChanged);
            // 
            // RLLabel
            // 
            this.RLLabel.AutoSize = true;
            this.RLLabel.Location = new System.Drawing.Point(15, 108);
            this.RLLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
            this.RLLabel.Name = "RLLabel";
            this.RLLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RLLabel.Size = new System.Drawing.Size(171, 13);
            this.RLLabel.TabIndex = 11;
            this.RLLabel.Text = "Relaunch Delay Per Account (sec)";
            // 
            // RelaunchDelayNumber
            // 
            this.SettingsLayoutPanel.SetFlowBreak(this.RelaunchDelayNumber, true);
            this.RelaunchDelayNumber.Location = new System.Drawing.Point(192, 105);
            this.RelaunchDelayNumber.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
            this.RelaunchDelayNumber.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.RelaunchDelayNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.RelaunchDelayNumber.Name = "RelaunchDelayNumber";
            this.RelaunchDelayNumber.Size = new System.Drawing.Size(56, 20);
            this.RelaunchDelayNumber.TabIndex = 10;
            this.RelaunchDelayNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.RelaunchDelayNumber.ValueChanged += new System.EventHandler(this.RelaunchDelayNumber_ValueChanged);
            // 
            // LDLabel
            // 
            this.LDLabel.Location = new System.Drawing.Point(15, 129);
            this.LDLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
            this.LDLabel.Name = "LDLabel";
            this.LDLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.LDLabel.Size = new System.Drawing.Size(171, 13);
            this.LDLabel.TabIndex = 23;
            this.LDLabel.Text = "Launcher Delay (sec)";
            // 
            // LauncherDelayNumber
            // 
            this.SettingsLayoutPanel.SetFlowBreak(this.LauncherDelayNumber, true);
            this.LauncherDelayNumber.Location = new System.Drawing.Point(192, 126);
            this.LauncherDelayNumber.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
            this.LauncherDelayNumber.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.LauncherDelayNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.LauncherDelayNumber.Name = "LauncherDelayNumber";
            this.LauncherDelayNumber.Size = new System.Drawing.Size(56, 20);
            this.LauncherDelayNumber.TabIndex = 22;
            this.LauncherDelayNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.LauncherDelayNumber.ValueChanged += new System.EventHandler(this.LauncherDelayNumber_ValueChanged);
            // 
            // PortLabel
            // 
            this.PortLabel.Location = new System.Drawing.Point(15, 150);
            this.PortLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
            this.PortLabel.Name = "PortLabel";
            this.PortLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.PortLabel.Size = new System.Drawing.Size(171, 13);
            this.PortLabel.TabIndex = 9;
            this.PortLabel.Text = "Port";
            // 
            // PortNumber
            // 
            this.SettingsLayoutPanel.SetFlowBreak(this.PortNumber, true);
            this.PortNumber.Location = new System.Drawing.Point(192, 147);
            this.PortNumber.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
            this.PortNumber.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PortNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.PortNumber.Name = "PortNumber";
            this.PortNumber.Size = new System.Drawing.Size(56, 20);
            this.PortNumber.TabIndex = 8;
            this.PortNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.PortNumber.ValueChanged += new System.EventHandler(this.PortNumber_ValueChanged);
            // 
            // MinimizeRoblox
            // 
            this.MinimizeRoblox.Location = new System.Drawing.Point(15, 170);
            this.MinimizeRoblox.Name = "MinimizeRoblox";
            this.MinimizeRoblox.Size = new System.Drawing.Size(145, 23);
            this.MinimizeRoblox.TabIndex = 12;
            this.MinimizeRoblox.Text = "Minimize Roblox";
            this.MinimizeRoblox.UseVisualStyleBackColor = true;
            this.MinimizeRoblox.Click += new System.EventHandler(this.MinimizeRoblox_Click);
            // 
            // AutoMinimizeCB
            // 
            this.AutoMinimizeCB.Location = new System.Drawing.Point(166, 174);
            this.AutoMinimizeCB.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.AutoMinimizeCB.Name = "AutoMinimizeCB";
            this.AutoMinimizeCB.Size = new System.Drawing.Size(91, 17);
            this.AutoMinimizeCB.TabIndex = 14;
            this.AutoMinimizeCB.Text = "Auto Minimize";
            this.AutoMinimizeCB.UseVisualStyleBackColor = true;
            this.AutoMinimizeCB.CheckedChanged += new System.EventHandler(this.AutoMinimizeCB_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(263, 167);
            this.label8.Name = "label8";
            this.label8.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.label8.Size = new System.Drawing.Size(68, 21);
            this.label8.TabIndex = 19;
            this.label8.Text = "Interval (sec)";
            // 
            // AutoMinIntervalNum
            // 
            this.SettingsLayoutPanel.SetFlowBreak(this.AutoMinIntervalNum, true);
            this.AutoMinIntervalNum.Location = new System.Drawing.Point(337, 170);
            this.AutoMinIntervalNum.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.AutoMinIntervalNum.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.AutoMinIntervalNum.Name = "AutoMinIntervalNum";
            this.AutoMinIntervalNum.Size = new System.Drawing.Size(69, 20);
            this.AutoMinIntervalNum.TabIndex = 20;
            this.AutoMinIntervalNum.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.AutoMinIntervalNum.ValueChanged += new System.EventHandler(this.AutoMinIntervalNum_ValueChanged);
            // 
            // CloseRoblox
            // 
            this.CloseRoblox.Location = new System.Drawing.Point(15, 199);
            this.CloseRoblox.Name = "CloseRoblox";
            this.CloseRoblox.Size = new System.Drawing.Size(145, 23);
            this.CloseRoblox.TabIndex = 13;
            this.CloseRoblox.Text = "Close Roblox";
            this.CloseRoblox.UseVisualStyleBackColor = true;
            this.CloseRoblox.Click += new System.EventHandler(this.CloseRoblox_Click);
            //
            // tableLayoutPanel1
            //
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.AutoCloseType, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.ACLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.AutoCloseIntervalNum, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.MaxInstanceLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.MaxInstancesNum, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.AutoCloseCB, 0, 0);
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(166, 199);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(194, 79);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // AutoCloseType
            // 
            this.AutoCloseType.FormattingEnabled = true;
            this.AutoCloseType.ItemHeight = 13;
            this.AutoCloseType.Items.AddRange(new object[] {
            "Per Instance",
            "Global"});
            this.AutoCloseType.Location = new System.Drawing.Point(100, 3);
            this.AutoCloseType.Name = "AutoCloseType";
            this.AutoCloseType.Size = new System.Drawing.Size(90, 21);
            this.AutoCloseType.TabIndex = 21;
            this.AutoCloseType.Text = "Per Instance";
            this.AutoCloseType.SelectedIndexChanged += new System.EventHandler(this.AutoCloseType_SelectedIndexChanged);
            // 
            // ACLabel
            // 
            this.ACLabel.AutoSize = true;
            this.ACLabel.Location = new System.Drawing.Point(3, 27);
            this.ACLabel.Name = "ACLabel";
            this.ACLabel.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.ACLabel.Size = new System.Drawing.Size(67, 17);
            this.ACLabel.TabIndex = 17;
            this.ACLabel.Text = "Interval (min)";
            // 
            // AutoCloseIntervalNum
            // 
            this.AutoCloseIntervalNum.Location = new System.Drawing.Point(100, 30);
            this.AutoCloseIntervalNum.Maximum = new decimal(new int[] {
            6000,
            0,
            0,
            0});
            this.AutoCloseIntervalNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.AutoCloseIntervalNum.Name = "AutoCloseIntervalNum";
            this.AutoCloseIntervalNum.Size = new System.Drawing.Size(69, 20);
            this.AutoCloseIntervalNum.TabIndex = 18;
            this.AutoCloseIntervalNum.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.AutoCloseIntervalNum.ValueChanged += new System.EventHandler(this.AutoCloseIntervalNum_ValueChanged);
            // 
            // MaxInstanceLabel
            // 
            this.MaxInstanceLabel.AutoSize = true;
            this.MaxInstanceLabel.Location = new System.Drawing.Point(3, 53);
            this.MaxInstanceLabel.Name = "MaxInstanceLabel";
            this.MaxInstanceLabel.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.MaxInstanceLabel.Size = new System.Drawing.Size(76, 17);
            this.MaxInstanceLabel.TabIndex = 27;
            this.MaxInstanceLabel.Text = "Max Instances";
            this.Helper.SetToolTip(this.MaxInstanceLabel, "Will close every single Roblox process if there\r\nare over a specified amount of i" +
        "nstances open");
            // 
            // MaxInstancesNum
            // 
            this.MaxInstancesNum.Location = new System.Drawing.Point(100, 56);
            this.MaxInstancesNum.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.MaxInstancesNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MaxInstancesNum.Name = "MaxInstancesNum";
            this.MaxInstancesNum.Size = new System.Drawing.Size(69, 20);
            this.MaxInstancesNum.TabIndex = 28;
            this.MaxInstancesNum.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.MaxInstancesNum.ValueChanged += new System.EventHandler(this.MaxInstancesNum_ValueChanged);
            // 
            // AutoCloseCB
            // 
            this.AutoCloseCB.Location = new System.Drawing.Point(3, 7);
            this.AutoCloseCB.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.AutoCloseCB.Name = "AutoCloseCB";
            this.AutoCloseCB.Size = new System.Drawing.Size(91, 17);
            this.AutoCloseCB.TabIndex = 16;
            this.AutoCloseCB.Text = "Auto Close";
            this.AutoCloseCB.UseVisualStyleBackColor = true;
            this.AutoCloseCB.CheckedChanged += new System.EventHandler(this.AutoCloseCB_CheckedChanged);
            // 
            // HelpPage
            // 
            this.HelpPage.Controls.Add(this.label7);
            this.HelpPage.Controls.Add(this.NexusDocsButton);
            this.HelpPage.Controls.Add(this.label6);
            this.HelpPage.Controls.Add(this.NexusDL);
            this.HelpPage.Controls.Add(this.label5);
            this.HelpPage.Controls.Add(this.label4);
            this.HelpPage.Location = new System.Drawing.Point(4, 25);
            this.HelpPage.Name = "HelpPage";
            this.HelpPage.Size = new System.Drawing.Size(585, 365);
            this.HelpPage.TabIndex = 2;
            this.HelpPage.Text = "Help";
            this.HelpPage.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 79);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(346, 39);
            this.label7.TabIndex = 5;
            this.label7.Text = resources.GetString("label7.Text");
            // 
            // NexusDocsButton
            // 
            this.NexusDocsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NexusDocsButton.Location = new System.Drawing.Point(92, 337);
            this.NexusDocsButton.Name = "NexusDocsButton";
            this.NexusDocsButton.Size = new System.Drawing.Size(94, 23);
            this.NexusDocsButton.TabIndex = 4;
            this.NexusDocsButton.Text = "Documentation";
            this.NexusDocsButton.UseVisualStyleBackColor = true;
            this.NexusDocsButton.Click += new System.EventHandler(this.NexusDocsButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 133);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(301, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Found a bug? Make sure to report it in the discord or on github";
            // 
            // NexusDL
            // 
            this.NexusDL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NexusDL.Location = new System.Drawing.Point(11, 337);
            this.NexusDL.Name = "NexusDL";
            this.NexusDL.Size = new System.Drawing.Size(75, 23);
            this.NexusDL.TabIndex = 2;
            this.NexusDL.Text = "Nexus.lua";
            this.NexusDL.UseVisualStyleBackColor = true;
            this.NexusDL.Click += new System.EventHandler(this.NexusDL_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(417, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "To connect your accounts, make sure to download Nexus.lua into your autoexec fold" +
    "er";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(327, 26);
            this.label4.TabIndex = 0;
            this.label4.Text = "To add an account into Account Control, simply select the accounts\r\nfrom the main" +
    " form and drag them down into the Control Panel";
            // 
            // AutoRelaunchTimer
            // 
            this.AutoRelaunchTimer.Enabled = true;
            this.AutoRelaunchTimer.Interval = 9000;
            this.AutoRelaunchTimer.Tick += new System.EventHandler(this.AutoRelaunchTimer_Tick);
            // 
            // MinimzeTimer
            // 
            this.MinimzeTimer.Interval = 5000;
            this.MinimzeTimer.Tick += new System.EventHandler(this.MinimzeTimer_Tick);
            // 
            // CloseTimer
            // 
            this.CloseTimer.Interval = 1200000;
            this.CloseTimer.Tick += new System.EventHandler(this.CloseTimer_Tick);
            // 
            // AccountControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(593, 394);
            // WinForms lays out Dock=Fill/Top siblings by processing the LAST-added control
            // first, giving it first claim on space - verified empirically with an isolated
            // repro, since the intuitive "first added, first docked" reading is backwards and
            // silently produces overlapping controls (this cost real debugging time to catch,
            // since nothing throws - controls just overlap with no error). To get the visual
            // stack TopStrip (top) -> TabButtonsPanel -> HeaderPanel (fills the rest), they
            // must be added in the OPPOSITE order: Fill control first, then Top controls
            // bottom-most-visually first, topmost-visually last.
            this.Controls.Add(this.HeaderPanel);
            this.Controls.Add(this.TabButtonsPanel);
            this.Controls.Add(this.TopStrip);
            this.MinimumSize = new System.Drawing.Size(475, 200);
            this.Name = "AccountControl";
            this.ShowIcon = false;
            this.Text = "Account Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AccountControl_FormClosing);
            this.Load += new System.EventHandler(this.AccountControl_Load);
            this.ControlsPanel.ResumeLayout(false);
            this.ControlsPanel.PerformLayout();
            this.TopStrip.ResumeLayout(false);
            this.TopStrip.PerformLayout();
            this.TabButtonsPanel.ResumeLayout(false);
            this.ACTabs.ResumeLayout(false);
            this.ControlPage.ResumeLayout(false);
            this.CPanel.ResumeLayout(false);
            this.CPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AccountsView)).EndInit();
            this.ACStrip.ResumeLayout(false);
            this.SettingsTab.ResumeLayout(false);
            this.SettingsLayoutPanel.ResumeLayout(false);
            this.SettingsLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RelaunchDelayNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LauncherDelayNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AutoMinIntervalNum)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AutoCloseIntervalNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxInstancesNum)).EndInit();
            this.WebControlTab.ResumeLayout(false);
            this.WebControlLayoutPanel.ResumeLayout(false);
            this.WebControlLayoutPanel.PerformLayout();
            this.HelpPage.ResumeLayout(false);
            this.HelpPage.PerformLayout();
            this.HeaderPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel ControlsPanel;
        private System.Windows.Forms.TabPage ControlPage;
        private System.Windows.Forms.Label StatusSummaryLabel;
        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Panel TopStrip;
        public BrightIdeasSoftware.ObjectListView AccountsView;
        private BrightIdeasSoftware.OLVColumn cStatus;
        private BrightIdeasSoftware.OLVColumn cUsername;
        private BrightIdeasSoftware.OLVColumn cJobId;
        private BrightIdeasSoftware.OLVColumn cPlaceId;
        private BrightIdeasSoftware.OLVColumn cPlaceName;
        private BrightIdeasSoftware.OLVColumn cPlayers;
        private BrightIdeasSoftware.OLVColumn cAlive;
        private BrightIdeasSoftware.OLVColumn cMoney;
        private BrightIdeasSoftware.OLVColumn cBank;
        private BrightIdeasSoftware.MultiImageRenderer StatusRenderer;
        private BrightIdeasSoftware.OLVColumn cCheckBoxes;
        private System.Windows.Forms.CheckBox AutoRejoinCheckbox;
        private System.Windows.Forms.Label AutoRejoinJobIdLabel;
        private RBX_Alt_Manager.Classes.BorderedTextBox AutoRejoinJobIdTextBox;
        private System.Windows.Forms.CheckBox AutoAdoptRejoinJobIdCB;
        private System.Windows.Forms.Panel CPanel;
        private System.Windows.Forms.Timer AutoRelaunchTimer;
        private System.Windows.Forms.TabPage HelpPage;
        private System.Windows.Forms.Button NexusDL;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button NexusDocsButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ContextMenuStrip ACStrip;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeRobloxToolStripMenuItem;
        private System.Windows.Forms.TabPage SettingsTab;
        private System.Windows.Forms.FlowLayoutPanel SettingsLayoutPanel;
        private System.Windows.Forms.Label RLLabel;
        private System.Windows.Forms.NumericUpDown RelaunchDelayNumber;
        private System.Windows.Forms.Label PortLabel;
        private System.Windows.Forms.NumericUpDown PortNumber;
        private System.Windows.Forms.CheckBox AllowExternalConnectionsCB;
        private System.Windows.Forms.ToolStripMenuItem copyJobIdToolStripMenuItem;
        private System.Windows.Forms.Button MinimizeRoblox;
        private System.Windows.Forms.Button CloseRoblox;
        private System.Windows.Forms.TabPage WebControlTab;
        private System.Windows.Forms.FlowLayoutPanel WebControlLayoutPanel;
        private System.Windows.Forms.Label WebControlLabel;
        private System.Windows.Forms.TextBox WebControlTokenBox;
        private System.Windows.Forms.Button CopyWebControlUrlButton;
        private System.Windows.Forms.Button RegenerateTokenButton;
        private System.Windows.Forms.CheckBox PublicAccessCB;
        private System.Windows.Forms.Label NgrokAuthTokenLabel;
        private System.Windows.Forms.TextBox NgrokAuthTokenBox;
        private System.Windows.Forms.Label PublicAccessStatusLabel;
        private System.Windows.Forms.Label RelayLabel;
        private System.Windows.Forms.CheckBox RelayEnabledCB;
        private System.Windows.Forms.Label RelayUrlLabel;
        private System.Windows.Forms.TextBox RelayUrlBox;
        private System.Windows.Forms.Label RelayTokenLabel;
        private System.Windows.Forms.TextBox RelayTokenBox;
        private System.Windows.Forms.Label RelayMachineLabel;
        private System.Windows.Forms.TextBox RelayMachineBox;
        private System.Windows.Forms.Label RelayStatusLabel;
        private System.Windows.Forms.CheckBox AutoMinimizeCB;
        private System.Windows.Forms.Timer MinimzeTimer;
        private System.Windows.Forms.CheckBox StartOnLaunch;
        private NBTabControl ACTabs;
        private System.Windows.Forms.Panel HeaderPanel;
        private System.Windows.Forms.FlowLayoutPanel TabButtonsPanel;
        private System.Windows.Forms.Button ControlPageButton;
        private System.Windows.Forms.Button SettingsTabButton;
        private System.Windows.Forms.Button WebControlTabButton;
        private System.Windows.Forms.Button HelpPageButton;
        private CheckBox AutoCloseCB;
        private Label ACLabel;
        private Label label8;
        private NumericUpDown AutoMinIntervalNum;
        private NumericUpDown AutoCloseIntervalNum;
        private ComboBox AutoCloseType;
        private Timer CloseTimer;
        private Label LDLabel;
        private NumericUpDown LauncherDelayNumber;
        private CheckBox InternetCheckCB;
        private CheckBox UsePresenceCB;
        private ToolTip Helper;
        private Label MaxInstanceLabel;
        private NumericUpDown MaxInstancesNum;
        private TableLayoutPanel tableLayoutPanel1;
    }
}