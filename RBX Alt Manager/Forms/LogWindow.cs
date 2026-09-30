using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RBX_Alt_Manager.Forms
{
    public class LogWindow : Form
    {
        private RichTextBox logTextBox;
        private static LogWindow instance;

        public LogWindow()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Auto Re-join Logs";
            this.Width = 800;
            this.Height = 600;
            this.BackColor = Color.FromArgb(30, 41, 59);
            this.ForeColor = Color.FromArgb(248, 250, 252);

            logTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                WordWrap = true
            };

            this.Controls.Add(logTextBox);
        }

        private const int MaxLines = 1000;

        public void AppendLog(string message)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action<string>(AppendLog), message);
                return;
            }

            logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");

            // Trim old lines so the buffer doesn't grow unbounded over long-running sessions
            // with many accounts (unbounded RichTextBox text was a source of slowdown/hangs).
            if (logTextBox.Lines.Length > MaxLines)
            {
                var trimmed = logTextBox.Lines.Skip(logTextBox.Lines.Length - MaxLines).ToArray();
                logTextBox.Lines = trimmed;
            }

            logTextBox.SelectionStart = logTextBox.Text.Length;
            logTextBox.ScrollToCaret();
        }

        public static LogWindow GetInstance()
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new LogWindow();
            }
            return instance;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Don't dispose, just hide
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnFormClosing(e);
        }
    }
}
