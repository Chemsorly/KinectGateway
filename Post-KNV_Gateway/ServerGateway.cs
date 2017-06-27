using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerListener = Post_knv_Server.Webservice.ServerListener;
using ServerDefinition = Post_knv_Server.Webservice.ServerDefinition;
using Post_KNV_Client.WebService;

namespace Post_KNV_Gateway
{
    /// <summary>
    /// gateway logic to move external client requests to internal server
    /// </summary>
    class ServerGateway
    {
        /// <summary>
        /// target server address to send the messages to
        /// </summary>
        public string serverAddress;

        /// <summary>
        /// event handler to manage log messages
        /// </summary>
        /// <param name="message">the message</param>
        public delegate void OnMessageRecievedHandler(String message);
        public event OnMessageRecievedHandler OnMessageRecievedEvent;

        /// <summary>
        /// the client sender to mirror incoming client messages
        /// </summary>
        ClientSender _webserviceSender;

        /// <summary>
        /// the server listener to mirror incoming client messages
        /// </summary>
        ServerListener _webserviceListener = null;

        /// <summary>
        /// constructor
        /// </summary>
        public ServerGateway()
        {
            ServerDefinition.OnConfigRequestEvent += ServerDefinition_OnConfigRequestEvent;
            ServerDefinition.OnHelloRequestEvent += ServerDefinition_OnHelloRequestEvent;
            ServerDefinition.OnKinectDataPackageEvent += ServerDefinition_OnKinectDataPackageEvent;
            ServerDefinition.OnKinectStatusPackageEvent += ServerDefinition_OnKinectStatusPackageEvent;
            ServerDefinition.OnAppScanrequestEvent += ServerDefinition_OnAppScanrequestEvent;
        }

        /// <summary>
        /// initializes the server gateway with target address and listening port
        /// </summary>
        /// <param name="pServerAddress">target address</param>
        /// <param name="pListeningPort">listening port</param>
        public void Initialize(String pServerAddress, int pListeningPort)
        {
            this.serverAddress = pServerAddress;

            //init listener
            if (_webserviceListener != null)
                _webserviceListener.Close();

            _webserviceListener = new ServerListener(pListeningPort);

            //init sender
            if (_webserviceSender != null)
            {
                try
                {
                    _webserviceSender.OnHelloSuccessfullEvent -= _webserviceSender_OnLogMessage;
                    _webserviceSender.OnConfigSuccessfullEvent -= _webserviceSender_OnLogMessage;
                    _webserviceSender.OnErrorEvent -= _webserviceSender_OnError;
                }catch(Exception) {}
            }

            _webserviceSender = new ClientSender();
            _webserviceSender.OnHelloSuccessfullEvent += _webserviceSender_OnLogMessage;
            _webserviceSender.OnConfigSuccessfullEvent += _webserviceSender_OnLogMessage;
            _webserviceSender.OnErrorEvent += _webserviceSender_OnError;
        }

        void _webserviceSender_OnError(Exception pEx)
        { OnMessageRecievedEvent(pEx.Message); }

        void _webserviceSender_OnLogMessage(string response)
        { OnMessageRecievedEvent(response); }


        /// <summary>
        /// gets fired when a status package arrives. forwards it to the server
        /// </summary>
        /// <param name="EventArgs">the status package</param>
        void ServerDefinition_OnKinectStatusPackageEvent(Post_KNV_MessageClasses.KinectStatusPackage EventArgs)
        {
            _webserviceSender.startStatusRequest(serverAddress, EventArgs);
            OnMessageRecievedEvent("[ServerGateway] Kinect Status package forwarded to " + serverAddress);
        }

        /// <summary>
        /// gets fired when a data package arrives. forwards it to the server
        /// </summary>
        /// <param name="EventArgs">the data package</param>
        void ServerDefinition_OnKinectDataPackageEvent(Post_KNV_MessageClasses.KinectDataPackage EventArgs)
        {
            _webserviceSender.startDataRequest(serverAddress, EventArgs);
            OnMessageRecievedEvent("[ServerGateway] Kinect Data package forwarded to " + serverAddress);
        }

        /// <summary>
        /// gets fired when a hello request arrives. forwards it to the server
        /// </summary>
        /// <param name="EventArgs">the hello request object</param>
        void ServerDefinition_OnHelloRequestEvent(Post_KNV_MessageClasses.HelloRequestObject EventArgs)
        {
            _webserviceSender.startHelloRequest(serverAddress, EventArgs);
            OnMessageRecievedEvent("[ServerGateway] Hello Request forwarded to " + serverAddress);
        }

        /// <summary>
        /// gets fired when a config request arrives. forwards it to the server
        /// </summary>
        /// <param name="EventArgs">the config object</param>
        void ServerDefinition_OnConfigRequestEvent(Post_KNV_MessageClasses.ClientConfigObject EventArgs)
        {
            _webserviceSender.startConfigRequest(serverAddress, EventArgs);
            OnMessageRecievedEvent("[ServerGateway] Config forwarded to " + serverAddress);
        }

        /// <summary>
        /// gets fired when a apprequest occurs, forwards the request and returns the answer
        /// </summary>
        /// <param name="EventArgs">the app request object</param>
        /// <returns>the answer back to requesting client</returns>
        String ServerDefinition_OnAppScanrequestEvent(Post_KNV_MessageClasses.AppRequestObject EventArgs)
        {
            String s = _webserviceSender.startAppRequest(serverAddress, EventArgs);
            OnMessageRecievedEvent("[ServerGateway] AppRequest forwarded to " + serverAddress);
            return s;
        }

    }
}
