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

        public string InputConnectTimeoutTextBoxText { get; set; } = string.Empty;

        public string InputReceiveTimeoutTextBoxText { get; set; } = string.Empty;

        public string InputReTryNumTextBoxText { get; set; } = string.Empty;

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

                if (value)
                {
                    IsEnabledClientDisconnectButton = false;
                    IsEnabledClientSendDataButton = false;
                }
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

                if (value)
                {
                    IsEnabledClientConnectButton = false;
                }
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
            bool isValidate = true;

            string IP = InputSendIPTextBoxText;
            if (!Regex.IsMatch(IP, @"[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}"))
            {
                OutputMsgList.Add(new OutputTextModel(">>IPアドレスの形式で入力してください。"));

                isValidate = false;
            }

            int port;
            if (!ParseHelper.TryParsePositeviNumStr(InputSendPortTextBoxText, out port))
            {
                OutputMsgList.Add(new OutputTextModel(">>正の数でポートを設定してください。"));

                isValidate = false;
            }

            int connectTimeout;
            if (!ParseHelper.TryParsePositeviNumStr(InputConnectTimeoutTextBoxText, out connectTimeout))
            {
                OutputMsgList.Add(new OutputTextModel(">>正の数で接続タイムアウトを設定してください。"));

                isValidate = false;
            }

            int receiveTimeout;
            if (!ParseHelper.TryParsePositeviNumStr(InputReceiveTimeoutTextBoxText, out receiveTimeout))
            {
                OutputMsgList.Add(new OutputTextModel(">>正の数で受信タイムアウトを設定してください。"));

                isValidate = false;
            }

            int reTryNum;
            if (!ParseHelper.TryParsePositeviNumStr(InputReTryNumTextBoxText, out reTryNum))
            {
                OutputMsgList.Add(new OutputTextModel(">>正の数でリトライ回数を設定してください。"));

                isValidate = false;
            }

            if (!isValidate)
            {
                return;
            }

            ConnectEventInfo.AddEventHandler(Client, ConnectEventHandler);

            if (Client.Connect(IP, port, connectTimeout, receiveTimeout, reTryNum))
            {
                OutputMsgList.Add(new OutputTextModel(">>接続成功"));

                IsEnabledClientDisconnectButton = true;
                IsEnabledClientSendDataButton = true;
            }
            else
            {
                OutputMsgList.Add(new OutputTextModel(">>接続失敗"));
            }

            ConnectEventInfo.RemoveEventHandler(Client, ConnectEventHandler);
        }

        private void OnConnected(object sender, EventArgs e, EndPoint connectedEndPoint)
        {
            OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnConnected end"));
            OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnConnected end"));
        }

        private void OnDisconnect()
        {
            Client.DisConnect();

            OutputMsgList.Add(new OutputTextModel(">>切断"));

            IsEnabledClientConnectButton = true;
        }

        private void OnSendData()
        {
            ReceiveDataEventInfo.AddEventHandler(Client, ReceiveEventHandler);

            string sendText = InputSendStringTextBoxText;
            byte[] sendData = System.Text.Encoding.UTF8.GetBytes(sendText);
            if (Client.Send(sendData))
            {
                OutputMsgList.Add(new OutputTextModel(">>送信成功"));
            }
            else
            {
                OutputMsgList.Add(new OutputTextModel(">>送信失敗"));
            }

            ReceiveDataEventInfo.RemoveEventHandler(Client, ReceiveEventHandler);
        }

        private void OnReceiveData(object sender, byte[] receivedData)
        {
            OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnReceiveData start"));
            OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnReceiveData end"));
        }
    }
}
