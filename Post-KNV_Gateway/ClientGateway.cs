using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Post_knv_Server.Webservice;
using Post_KNV_Client.WebService;

namespace Post_KNV_Gateway
{
    /// <summary>
    /// gateway logic to move external server requests to internal clients. CLIENT GATEWAY NOT USED / FULLY IMPLEMENTED!
    /// </summary>
    class ClientGateway
    {
        public delegate void OnMessageRecievedHandler(String message);
        public event OnMessageRecievedHandler OnMessageRecievedEvent;

        //outgoing messages
        ServerSender _webserviceSender;

        //incoming messages
        ClientListener _webserviceListener;

        /// <summary>
        /// constructor
        /// </summary>
        ClientGateway()
        {
            throw new NotImplementedException();

            //init listener
            _webserviceListener = new ClientListener();
            _webserviceListener.initialize(8999);
            ClientDefinition.ErrorMessageEvent += ClientDefinition_ErrorMessageEvent;
            ClientDefinition.OnConfigRequestEvent += ClientDefinition_OnConfigRequestEvent;
            ClientDefinition.OnPingEvent += ClientDefinition_OnPingEvent;
            ClientDefinition.OnScanRequestEvent += ClientDefinition_OnScanRequestEvent;
            ClientDefinition.OnShutdownRequestEvent += ClientDefinition_OnShutdownRequestEvent;

            //init sender
            _webserviceSender = new ServerSender();   
        }

        void ClientDefinition_OnShutdownRequestEvent()
        {
            throw new NotImplementedException();
        }

        void ClientDefinition_OnScanRequestEvent(Post_KNV_MessageClasses.ClientConfigObject configObject)
        {
            throw new NotImplementedException();

            _webserviceSender.sendScanRequest(configObject);
            OnMessageRecievedEvent("[ClientGateway] Scan request forwarded to " + configObject.ownIP);
        }

        void ClientDefinition_OnPingEvent()
        {
            throw new NotImplementedException();
        }

        void ClientDefinition_OnConfigRequestEvent(Post_KNV_MessageClasses.ClientConfigObject pConfig)
        {
            throw new NotImplementedException();

            _webserviceSender.sendConfig(pConfig);
            OnMessageRecievedEvent("[ClientGateway] Config request forwarded to " + pConfig.ownIP);
        }

        void ClientDefinition_ErrorMessageEvent(Exception e)
        {
            throw new NotImplementedException();
        }




    }
}
