using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using Post_KNV_MessageClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(Post_KNV_Gateway.SignalRHandler.Startup))]
namespace Post_KNV_Gateway
{
    /// <summary>
    /// the signalr handler class for the gateway
    /// </summary>
    public class SignalRHandler
    {
        /// <summary>
        /// the signalr object
        /// </summary>
        IDisposable SignalR { get; set; }
        string ServerURI;

        /// <summary>
        /// loggable messages
        /// </summary>
        /// <param name="message"></param>
        public delegate void OnLogMessage(String message);
        public event OnLogMessage OnLogMessageRecieved;

        /// <summary>
        /// initializes the signalr server
        /// </summary>
        /// <param name="pServerURI">the signalr server adress to listen on (aka localhost)</param>
        public void Initialize(String pServerURI)
        {
            this.ServerURI = pServerURI;
            StartServer();
        }

        /// <summary>
        /// starts the server
        /// </summary>
        private void StartServer()
        {
            try
            {
                SignalR = WebApp.Start(ServerURI);
            }
            catch (Exception ex)
            {
                OnLogMessageRecieved("ERROR: " + ex.Message);
                return;
            }

            //try to unregister event
            try { ChatHub.OnMessageEvent -= ChatHub_OnMessageEvent; }
            catch (Exception) { }
            ChatHub.OnMessageEvent += ChatHub_OnMessageEvent;

            OnLogMessageRecieved("[SignalR] Server successfully started!");
        }

        /// <summary>
        /// relay chathub messages to log
        /// </summary>
        /// <param name="message">log message</param>
        void ChatHub_OnMessageEvent(string message) { OnLogMessageRecieved(message); }

        /// <summary> 
        /// Used by OWIN's startup process.  
        /// </summary>         
        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseCors(CorsOptions.AllowAll);
                app.MapSignalR();
            }
        }

        /// <summary> 
        /// Echoes messages sent using the Send message by calling the 
        /// addMessage method on the client. Also reports to the console 
        /// when clients connect and disconnect. 
        /// </summary> 
        public class ChatHub : Hub
        {
            #region Data Members

            //list of connected users
            static List<UserDetail> ConnectedUsers = new List<UserDetail>();

            //list of previous scan results
            static List<ScanResultPackage> CurrentScanresults = new List<ScanResultPackage>();

            public static event OnLogMessage OnMessageEvent;

            #endregion

            #region Methods

            /// <summary>
            /// connects a user to the server and relays information back to him
            /// </summary>
            /// <param name="userName">the own user name</param>
            public void Connect(string userName)
            {
                var id = Context.ConnectionId;
                if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
                {
                    ConnectedUsers.Add(new UserDetail { ConnectionId = id, UserName = userName });

                    // send to caller
                    Clients.Caller.onConnected(id, userName);
                    Clients.Caller.onConnectedApp(id, userName, CurrentScanresults);

                    // send to all except caller client
                    Clients.AllExcept(id).onNewUserConnected(id, userName);
                    OnMessageEvent("[SignalR] " + userName + " has connected.");
                }
            }

            /// <summary>
            /// broadcasts a status message to all clients
            /// </summary>
            /// <param name="pName">name of broadcaster</param>
            /// <param name="pStatus">status message</param>
            public void SendStatusMessage(String pName, String pStatus)
            {
                Clients.All.statusRecieved(pName, pStatus);
            }

            /// <summary>
            /// sends a scan result to all connected clients
            /// </summary>
            /// <param name="userName">the name of the broadcaster</param>
            /// <param name="pResult">the scan results</param>
            public void SendScanResult(string userName, ScanResultPackage pResult)
            {
                //broadcast message
                CurrentScanresults.Add(pResult);
                Clients.All.scanresultRecieved(userName, pResult);
                OnMessageEvent("[SignalR] " + userName + " broadcasted a result.");
            }

            /// <summary>
            /// clears all scan results
            /// </summary>
            /// <param name="userName">requesting user</param>
            public void ClearScanResults(string userName)
            {
                CurrentScanresults.Clear();
                Clients.All.scanresultCleared(userName);
                OnMessageEvent("[SignalR] " + userName + " cleared all scanresults.");
            }

            /// <summary>
            /// gets called after a user disconnected
            /// </summary>
            /// <param name="pStopCalled"></param>
            /// <returns></returns>
            public override System.Threading.Tasks.Task OnDisconnected(bool pStopCalled)
            {
                var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                if (item != null)
                {
                    ConnectedUsers.Remove(item);

                    var id = Context.ConnectionId;
                    Clients.All.onUserDisconnected(id, item.UserName);
                    OnMessageEvent("[SignalR] " + item.UserName + " disconnected.");
                }                
                return base.OnDisconnected(pStopCalled);
            }

            #endregion
        }

        /// <summary>
        /// user detail for the userlist
        /// </summary>
        public class UserDetail
        {
            public string ConnectionId { get; set; }
            public string UserName { get; set; }
        }
    }
}
