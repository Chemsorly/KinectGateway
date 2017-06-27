using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
﻿using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Security.Principal; 

namespace Post_KNV_Gateway
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServerGateway _serverGateway;
        SignalRHandler _signalrHandler;
        const string configPath = "config.xml";

        /// <summary>
        /// constructor. subscribes to log events and creates gateway + signalr instances
        /// </summary>
        public MainWindow()
        {
            //load data
            InitializeComponent();
            LoadData();

            //startup point
            if (!IsRunAsAdministrator())
            {
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                // The following properties run the new process as administrator
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";

                // Start the new process
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    // The user did not allow the application to run as administrator
                    MessageBox.Show("Sorry, this application must be run as Administrator.");
                }

                // Shut down the current process
                Application.Current.Shutdown();
                return;
            }

            if (IsRunAsAdministrator()) writeConsole("[Application] Elevated to admin rights.");

            Post_knv_Server.Log.LogManager.OnLogMessageEvent += LogManager_OnLogMessageEvent;
            Post_knv_Server.Log.LogManager.OnLogMessageDebugEvent += LogManager_OnLogMessageEvent;
            Post_KNV_Client.Log.LogManager.OnLogMessageEvent += LogManager_OnLogMessageEvent;
            Post_KNV_Client.Log.LogManager.OnLogMessageDebugEvent += LogManager_OnLogMessageEvent;
            
            //init server gateway
            _serverGateway = new ServerGateway();
            _serverGateway.OnMessageRecievedEvent += LogManager_OnLogMessageEvent;
            _serverGateway.Initialize(this._TextboxServerAddress.Text, int.Parse(this._TextboxListeningPort.Text));

            //CLIENT GATEWAY NOT USED

            //init signalr server for app message broadcasting
            this._signalrHandler = new SignalRHandler();
            this._signalrHandler.OnLogMessageRecieved += LogManager_OnLogMessageEvent;
            this._signalrHandler.Initialize(@"http://+:9000");
        }

        /// <summary>
        /// checks if the application is run as admin
        /// </summary>
        /// <returns>true if run as admin</returns>
        private bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }


        /// <summary>
        /// send a log message to the console
        /// </summary>
        /// <param name="message">the message</param>
        void LogManager_OnLogMessageEvent(string message)
        {
            this.writeConsole(message);
        }

        /// <summary>
        /// writes in the console
        /// </summary>
        /// <param name="message">the output message</param>
        void writeConsole(String message)
        {
            this.Dispatcher.Invoke(() =>
            {
                _Console.Text = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + " \n" + _Console.Text;
            });
        }

        /// <summary>
        /// sets the new data in the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ButtonSetAddress_Click(object sender, RoutedEventArgs e)
        {
            writeConsole("Address changed to " + this._TextboxServerAddress.Text);
            SaveData();
            _serverGateway.Initialize(this._TextboxServerAddress.Text, int.Parse(this._TextboxListeningPort.Text));            
        }

        /// <summary>
        /// saves the config data
        /// </summary>
        private void SaveData()
        {
            try
            {
                using(StreamWriter stream = new StreamWriter(configPath))
                {
                    //create config data object
                    ConfigData data = new ConfigData();
                    data.targetAddress = this._TextboxServerAddress.Text;
                    data.listeningPort = int.Parse(_TextboxListeningPort.Text);

                    //serialize and save
                    XmlSerializer serializer = new XmlSerializer(typeof(ConfigData));
                    serializer.Serialize(stream, data);

                    stream.Close();
                    writeConsole("Config saved to file.");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// loads the config data
        /// </summary>
        private void LoadData()
        {
            try
            {
                using(StreamReader stream = new StreamReader(configPath))
                {
                    //read object
                    XmlSerializer deserializer = new XmlSerializer(typeof(ConfigData));
                    ConfigData data = (ConfigData)deserializer.Deserialize(stream);

                    //write object data
                    this._TextboxServerAddress.Text = data.targetAddress;
                    this._TextboxListeningPort.Text = data.listeningPort.ToString();

                    stream.Close();
                    writeConsole("Config loaded from file.");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// holding class for config object
        /// </summary>
        public class ConfigData
        {
            /// <summary>
            /// target address of the server; format: host:port
            /// </summary>
            public String targetAddress { get; set; }

            /// <summary>
            /// port the listener is listening on
            /// </summary>
            public int listeningPort { get; set; }
        }

        /// <summary>
        /// gets called when the window is closing
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveData();
        }
    }
}
