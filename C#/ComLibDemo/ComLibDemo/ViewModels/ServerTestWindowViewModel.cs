using ComLibDemo.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using static ComTCP.TCPServer;

namespace ComLibDemo.ViewModels
{
    public class ServerTestWindowViewModel : BindableBase
    {
        public ObservableCollection<OutputTextModel> OutputMsgList { get; set; } = new ObservableCollection<OutputTextModel>();

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

                if (value)
                {
                    IsEnabledServerEndServiceButton = false;
                }
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

                if (value)
                {
                    IsEnabledServerStartServiceButton = false;
                }
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
            TCPServer = module.GetType("ComTCP.TCPServer");

            if (TCPServer != null)
            {
                ServerService = Activator.CreateInstance(TCPServer, port);
                return true;
            }

            OutputMsgList.Add(new OutputTextModel(">>Dllが存在しません。"));

            return false;
        }

        private void OnStartService()
        {
            int port;
            if (!int.TryParse(InputListenPortTextBoxText, out port))
            {
                OutputMsgList.Add(new OutputTextModel(">>数値でポートを設定してください。"));

                return;
            }

            if (port < 0)
            {
                OutputMsgList.Add(new OutputTextModel(">>正の数でポートを設定してください。"));

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

            ServerService.StartService(1000);

            IsEnabledServerEndServiceButton = true;
        }

        private void OnReceiveData(object sender, byte[] receivedData, ref byte[] sendData, ref bool isSendAll)
        {
            OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnReceiveData start"));
            OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnReceiveData end"));
        }

        private void OnDisconnected(object sender, EventArgs e, EndPoint disconnectedEndPoint)
        {
            OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnDisconnected start"));
            OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnDisconnected end"));
        }

        private void OnConnected(object sender, EventArgs e, EndPoint connectedEndPoint)
        {
            OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnConnected end"));
            OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnConnected end"));
        }

        private void OnSend(int byteSize, EndPoint sendEndPoint)
        {
            OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnSend end"));
            OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnSend end"));
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
