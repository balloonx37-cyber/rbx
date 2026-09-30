using RBX_Alt_Manager.Forms;
using System;
using System.Linq;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RBX_Alt_Manager.Nexus
{
    public class WebsocketServer : WebSocketBehavior
    {
        private static int _num = 0;

        private string _name;
        private string _prefix;

        public WebsocketServer() : this("anon#")
        {
        }

        public WebsocketServer(string prefix) => _prefix = prefix;

        private string getName() => Context.QueryString["name"] ?? (_prefix + getNum());
        private int getNum() => Interlocked.Increment(ref _num);

        protected override void OnOpen()
        {
            if (string.IsNullOrEmpty(Context.QueryString["name"]) || string.IsNullOrEmpty(Context.QueryString["id"])) { Context.WebSocket.Close(); return; }

            string jobID = string.IsNullOrEmpty(Context.QueryString["jobId"]) ? "UNKNOWN" : Context.QueryString["jobId"];

            long.TryParse(Context.QueryString["id"], out long UserId);
            long.TryParse(Context.QueryString["placeId"], out long PlaceId);

            _name = getName();

            string username = Context.QueryString["name"];

            // No account with this username may be tracked yet - auto-add it instead of
            // rejecting the connection, so simply running Nexus.lua in an already-open Roblox
            // window is enough to show up, without having to add the account manually first.
            // This placeholder Account has no login cookie (Nexus.lua only ever sends the
            // username/userid, never credentials), so login-based actions like Launch/Join
            // Server won't work on it from the main account list - but everything that only
            // needs the live WebSocket connection (Auto Re-join, Nexus commands, presence)
            // works fine since those never re-authenticate, they just talk to the already
            // running game client.
            // GetOrAddAccount does the find-or-create atomically under a lock, since OnOpen
            // can run concurrently for the same username (e.g. Roblox reconnecting) and a
            // separate check-then-add here could add two ControlledAccounts for one account.
            (ControlledAccount Account, bool WasCreated) = AccountControl.Instance.GetOrAddAccount(username, UserId);

            if (WasCreated)
                Program.Logger.Info($"Auto-added {username} from a new Nexus connection");

            Account.Connect(Context);
            Account.InGameJobId = jobID;
            if (PlaceId > 0) Account.UpdatePlaceId(PlaceId);
            Utilities.InvokeIfRequired(AccountControl.Instance, () => AccountControl.Instance.AccountsView.RefreshObject(Account));
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (AccountControl.Instance.ContextList.TryGetValue(Context, out ControlledAccount account))
                account.HandleMessage(e.Data);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (AccountControl.Instance.ContextList.TryGetValue(Context, out ControlledAccount Account))
                Account.Disconnect();
        }

        protected override void OnError(ErrorEventArgs e) => Program.Logger.Error($"WebsocketServer Error {_name}: {e.Message} {e.Exception}");
    }
}