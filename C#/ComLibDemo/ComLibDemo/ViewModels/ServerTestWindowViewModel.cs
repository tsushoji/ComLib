using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Net;
using System.Reflection;
using System.Text;
using static ComTCP.TCPServer;

namespace ComLibDemo.ViewModels
{
    public class ServerTestWindowViewModel : BindableBase
    {
        public string OutputTextBoxText
        {
            get
            {
                if (_outputText.Length < 1)
                {
                    return string.Empty;
                }
                return _outputText.ToString();
            }

            set
            {
                throw new Exception();
            }
        }

        private StringBuilder _outputText = new StringBuilder();

        public string InputListenPortTextBoxText { get; set; } = string.Empty;

        private Type TCPServer { get; set; }

        private dynamic ServerService { get; set; }

        private EventInfo ReceiveDataEventInfo { get; set; }

        private Delegate ReceiveEventHandler { get; set; }

        private EventInfo DisconnectedEventInfo { get; set; }

        private Delegate DisconnectedEventHandler { get; set; }

        private EventInfo ConnectedEventInfo { get; set; }

        private Delegate ConnectedEventHandler { get; set; }

        private EventInfo SendEventInfo { get; set; }

        private Delegate SendEventHandler { get; set; }

        private bool _isEnabledServerStartServiceButton = true;
        public bool IsEnabledServerStartServiceButton
        {
            get
            {
                return _isEnabledServerStartServiceButton;
            }

            set
            {
                SetProperty(ref _isEnabledServerStartServiceButton, value);
                SetProperty(ref _isEnabledServerEndServiceButton, !value);
            }
        }

        private bool _isEnabledServerEndServiceButton = false;
        public bool IsEnabledServerEndServiceButton
        {
            get
            {
                return _isEnabledServerEndServiceButton;
            }

            set
            {
                SetProperty(ref _isEnabledServerEndServiceButton, value);
                SetProperty(ref _isEnabledServerStartServiceButton, !value);
            }
        }

        public DelegateCommand ServerStartServiceClicked { get; private set; }

        public DelegateCommand ServerEndServiceClicked { get; private set; }

        public ServerTestWindowViewModel()
        {
            SetEvent();
        }

        private void SetEvent()
        {
            ServerStartServiceClicked = new DelegateCommand(OnStartService);
            ServerEndServiceClicked = new DelegateCommand(OnEndService);

            ReceiveEventHandler = new ReceiveEventHandler(OnReceiveData);
            DisconnectedEventHandler = new DisconnectedEventHandler(OnDisconnected);
            ConnectedEventHandler = new ConnectedEventHandler(OnConnected);
            SendEventHandler = new SendEventHandler(OnSend);
        }

        private bool ImportDll(int port)
        {
            var asm = Assembly.LoadFrom("ComLib.dll");
            var module = asm.GetModule("ComLib.dll");
            TCPServer = module.GetType("ComLib.TCPServer");

            if (TCPServer != null)
            {
                ServerService = Activator.CreateInstance(TCPServer, port);
                return true;
            }

            return false;
        }

        private void OnStartService()
        {
            int port;
            if (!int.TryParse(InputListenPortTextBoxText, out port))
            {
                return;
            }

            if (port < 0)
            {
                return;
            }

            if (!ImportDll(port))
            {
                return;
            }

            ReceiveDataEventInfo = TCPServer.GetEvent("OnServerReceiveData");
            ReceiveDataEventInfo.AddEventHandler(ServerService, ReceiveEventHandler);

            DisconnectedEventInfo = TCPServer.GetEvent("OnServerDisconnected");
            DisconnectedEventInfo.AddEventHandler(ServerService, DisconnectedEventHandler);

            ConnectedEventInfo = TCPServer.GetEvent("OnServerConnected");
            ConnectedEventInfo.AddEventHandler(ServerService, ConnectedEventHandler);

            SendEventInfo = TCPServer.GetEvent("OnServerSend");
            SendEventInfo.AddEventHandler(ServerService, SendEventHandler);

            ServerService.StartService();

            IsEnabledServerEndServiceButton = true;
        }

        private void OnReceiveData(object sender, byte[] receivedData, ref byte[] sendData)
        {
            _outputText.AppendLine("TCPServer:OnReceiveData start");
            _outputText.AppendLine("TCPServer:OnReceiveData end");
        }

        private void OnDisconnected(object sender, EventArgs e, Exception ex)
        {
            _outputText.AppendLine("TCPServer:OnDisconnected start");
            _outputText.AppendLine("TCPServer:OnDisconnected end");
        }

        private void OnConnected(EventArgs e, EndPoint connectedEndPoint)
        {
            _outputText.AppendLine("TCPServer:OnConnected start");
            _outputText.AppendLine("TCPServer:OnConnected end");
        }

        private void OnSend(int byteSize, EndPoint sendEndPoint)
        {
            _outputText.AppendLine("TCPServer:OnSend start");
            _outputText.AppendLine("TCPServer:OnSend end");
        }

        private void OnEndService()
        {
            ServerService.EndService();

            ReceiveDataEventInfo.RemoveEventHandler(ServerService, ReceiveEventHandler);
            ReceiveDataEventInfo = null;

            DisconnectedEventInfo.RemoveEventHandler(ServerService, DisconnectedEventHandler);
            DisconnectedEventInfo = null;

            ConnectedEventInfo.RemoveEventHandler(ServerService, ConnectedEventHandler);
            ConnectedEventInfo = null;

            SendEventInfo.RemoveEventHandler(ServerService, SendEventHandler);
            SendEventInfo = null;

            IsEnabledServerStartServiceButton = true;
        }
    }
}
