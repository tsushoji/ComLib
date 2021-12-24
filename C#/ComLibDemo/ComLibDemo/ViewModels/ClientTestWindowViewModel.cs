using ComLibDemo.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using static ComTCP.TCPClient;

namespace ComLibDemo.ViewModels
{
    public class ClientTestWindowViewModel : BindableBase
    {
        public ObservableCollection<OutputTextModel> OutputMsgList { get; set; } = new ObservableCollection<OutputTextModel>();

        public string InputSendIPTextBoxText { get; set; } = string.Empty;

        public string InputSendPortTextBoxText { get; set; } = string.Empty;

        public string InputSendStringTextBoxText { get; set; } = string.Empty;

        private Type TCPClient { get; set; }

        private dynamic Client { get; set; }

        private EventInfo ReceiveDataEventInfo { get; set; }

        private Delegate ReceiveEventHandler { get; set; }

        private EventInfo ConnectEventInfo { get; set; }

        private Delegate ConnectEventHandler { get; set; }

        private bool _isEnabledClientConnectButton = false;
        public bool IsEnabledClientConnectButton
        {
            get
            {
                return _isEnabledClientConnectButton;
            }

            set
            {
                SetProperty(ref _isEnabledClientConnectButton, value);
                SetProperty(ref _isEnabledClientDisconnectButton, !value);
                SetProperty(ref _isEnabledClientSendDataButton, !value);
            }
        }

        private bool _isEnabledClientDisconnectButton = false;
        public bool IsEnabledClientDisconnectButton
        {
            get
            {
                return _isEnabledClientDisconnectButton;
            }

            set
            {
                SetProperty(ref _isEnabledClientDisconnectButton, value);
                SetProperty(ref _isEnabledClientConnectButton, !value);
                SetProperty(ref _isEnabledClientSendDataButton, !value);
            }
        }

        private bool _isEnabledClientSendDataButton = false;
        public bool IsEnabledClientSendDataButton
        {
            get
            {
                return _isEnabledClientSendDataButton;
            }

            set
            {
                SetProperty(ref _isEnabledClientSendDataButton, value);
            }
        }

        public DelegateCommand ClientConnectClicked { get; private set; }

        public DelegateCommand ClientDisconnectClicked { get; private set; }

        public DelegateCommand ClientSendDataClicked { get; private set; }

        public ClientTestWindowViewModel()
        {
            if (ImportDll())
            {
                ReceiveDataEventInfo = TCPClient.GetEvent("OnClientReceiveData");
                ConnectEventInfo = TCPClient.GetEvent("OnClientConnected");
                SetEvent();

                SetProperty(ref _isEnabledClientConnectButton, true);
            }
        }

        private void SetEvent()
        {
            ClientConnectClicked = new DelegateCommand(OnConnect);
            ClientDisconnectClicked = new DelegateCommand(OnDisconnect);
            ClientSendDataClicked = new DelegateCommand(OnSendData);

            ReceiveEventHandler = new ReceiveEventHandler(OnReceiveData);
            ConnectEventHandler = new ConnectEventHandler(OnConnected);
        }

        private bool ImportDll()
        {
            var asm = Assembly.LoadFrom("ComLib.dll");
            var module = asm.GetModule("ComLib.dll");
            TCPClient = module.GetType("ComTCP.TCPClient");

            if (TCPClient != null)
            {
                Client = Activator.CreateInstance(TCPClient);
                return true;
            }

            OutputMsgList.Add(new OutputTextModel(">>Dllが存在しません。"));

            return false;
        }

        private void OnConnect()
        {
            string IP = InputSendIPTextBoxText;
            if (!Regex.IsMatch(IP, @"[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}"))
            {
                OutputMsgList.Add(new OutputTextModel(">>IPアドレスの形式で入力してください。"));

                return;
            }

            int port;
            if (!int.TryParse(InputSendPortTextBoxText, out port))
            {
                OutputMsgList.Add(new OutputTextModel(">>数値でポートを設定してください。"));

                return;
            }

            if (port < 0)
            {
                OutputMsgList.Add(new OutputTextModel(">>正の数でポートを設定してください。"));

                return;
            }

            ConnectEventInfo.AddEventHandler(Client, ConnectEventHandler);

            Client.Connect(IP, port, 1000, 1000, 1);

            ConnectEventInfo.RemoveEventHandler(Client, ConnectEventHandler);

            IsEnabledClientDisconnectButton = true;
        }

        private void OnConnected(object sender, EventArgs e, EndPoint connectedEndPoint)
        {
            OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnConnected end"));
            OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnConnected end"));
        }

        private void OnDisconnect()
        {
            Client.DisConnect();

            IsEnabledClientConnectButton = true;
        }

        private void OnSendData()
        {
            ReceiveDataEventInfo.AddEventHandler(Client, ReceiveEventHandler);

            string sendText = InputSendStringTextBoxText;
            byte[] sendData = System.Text.Encoding.UTF8.GetBytes(sendText);
            Client.Send(sendData);

            ReceiveDataEventInfo.RemoveEventHandler(Client, ReceiveEventHandler);
        }

        private void OnReceiveData(object sender, byte[] receivedData)
        {
            OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnReceiveData start"));
            OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnReceiveData end"));
        }
    }
}
