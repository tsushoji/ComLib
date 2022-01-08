using ComLibDemo.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using static ComTCP.TCPClient;
using static ComTCP.TCPComBase;

namespace ComLibDemo.ViewModels
{
    public class ClientTestWindowViewModel : BindableBase
    {
        public ObservableCollection<OutputTextModel> OutputMsgList { get; set; } = new ObservableCollection<OutputTextModel>();

        private bool SurevConnectedDone { get; set; }
        public string InputSendIPTextBoxText { get; set; } = string.Empty;

        private bool _isEnabledInputSendIPTextBoxText = true;
        public bool IsEnabledInputSendIPTextBoxText
        {
            get
            {
                return _isEnabledInputSendIPTextBoxText;
            }

            set
            {
                SetProperty(ref _isEnabledInputSendIPTextBoxText, value);

                if (value)
                {
                    IsReadOnlyInputSendIPTextBoxText = false;
                }
            }
        }

        private bool _isReadOnlyInputSendIPTextBoxText = false;
        public bool IsReadOnlyInputSendIPTextBoxText
        {
            get
            {
                return _isReadOnlyInputSendIPTextBoxText;
            }

            set
            {
                SetProperty(ref _isReadOnlyInputSendIPTextBoxText, value);

                if (value)
                {
                    IsEnabledInputSendIPTextBoxText = false;
                }
            }
        }

        public string InputSendPortTextBoxText { get; set; } = string.Empty;

        private bool _isEnabledInputSendPortTextBoxText = true;
        public bool IsEnabledInputSendPortTextBoxText
        {
            get
            {
                return _isEnabledInputSendPortTextBoxText;
            }

            set
            {
                SetProperty(ref _isEnabledInputSendPortTextBoxText, value);

                if (value)
                {
                    IsReadOnlyInputSendPortTextBoxText = false;
                }
            }
        }

        private bool _isReadOnlyInputSendPortTextBoxText = false;
        public bool IsReadOnlyInputSendPortTextBoxText
        {
            get
            {
                return _isReadOnlyInputSendPortTextBoxText;
            }

            set
            {
                SetProperty(ref _isReadOnlyInputSendPortTextBoxText, value);

                if (value)
                {
                    IsEnabledInputSendPortTextBoxText = false;
                }
            }
        }

        public string InputConnectTimeoutTextBoxText { get; set; } = string.Empty;

        private bool _isEnabledInputConnectTimeoutTextBoxText = true;
        public bool IsEnabledInputConnectTimeoutTextBoxText
        {
            get
            {
                return _isEnabledInputConnectTimeoutTextBoxText;
            }

            set
            {
                SetProperty(ref _isEnabledInputConnectTimeoutTextBoxText, value);

                if (value)
                {
                    IsReadOnlyInputConnectTimeoutTextBoxText = false;
                }
            }
        }

        private bool _isReadOnlyInputConnectTimeoutTextBoxText = false;
        public bool IsReadOnlyInputConnectTimeoutTextBoxText
        {
            get
            {
                return _isReadOnlyInputConnectTimeoutTextBoxText;
            }

            set
            {
                SetProperty(ref _isReadOnlyInputConnectTimeoutTextBoxText, value);

                if (value)
                {
                    IsEnabledInputConnectTimeoutTextBoxText = false;
                }
            }
        }

        public string InputReceiveTimeoutTextBoxText { get; set; } = string.Empty;

        private bool _isEnabledInputReceiveTimeoutTextBoxText = true;
        public bool IsEnabledInputReceiveTimeoutTextBoxText
        {
            get
            {
                return _isEnabledInputReceiveTimeoutTextBoxText;
            }

            set
            {
                SetProperty(ref _isEnabledInputReceiveTimeoutTextBoxText, value);

                if (value)
                {
                    IsReadOnlyInputReceiveTimeoutTextBoxText = false;
                }
            }
        }

        private bool _isReadOnlyInputReceiveTimeoutTextBoxText = false;
        public bool IsReadOnlyInputReceiveTimeoutTextBoxText
        {
            get
            {
                return _isReadOnlyInputReceiveTimeoutTextBoxText;
            }

            set
            {
                SetProperty(ref _isReadOnlyInputReceiveTimeoutTextBoxText, value);

                if (value)
                {
                    IsEnabledInputReceiveTimeoutTextBoxText = false;
                }
            }
        }

        public string InputReTryNumTextBoxText { get; set; } = string.Empty;

        private bool _isEnabledInputReTryNumTextBoxText = true;
        public bool IsEnabledInputReTryNumTextBoxText
        {
            get
            {
                return _isEnabledInputReTryNumTextBoxText;
            }

            set
            {
                SetProperty(ref _isEnabledInputReTryNumTextBoxText, value);

                if (value)
                {
                    IsReadOnlyInputReTryNumTextBoxText = false;
                }
            }
        }

        private bool _isReadOnlyInputReTryNumTextBoxText = false;
        public bool IsReadOnlyInputReTryNumTextBoxText
        {
            get
            {
                return _isReadOnlyInputReTryNumTextBoxText;
            }

            set
            {
                SetProperty(ref _isReadOnlyInputReTryNumTextBoxText, value);

                if (value)
                {
                    IsEnabledInputReTryNumTextBoxText = false;
                }
            }
        }

        public string InputSendStringTextBoxText { get; set; } = string.Empty;

        private bool _isEnabledInputSendStringTextBoxText = false;
        public bool IsEnabledInputSendStringTextBoxText
        {
            get
            {
                return _isEnabledInputSendStringTextBoxText;
            }

            set
            {
                SetProperty(ref _isEnabledInputSendStringTextBoxText, value);

                if (value)
                {
                    IsReadOnlyInputSendStringTextBoxText = false;
                }
            }
        }

        private bool _isReadOnlyInputSendStringTextBoxText = true;
        public bool IsReadOnlyInputSendStringTextBoxText
        {
            get
            {
                return _isReadOnlyInputSendStringTextBoxText;
            }

            set
            {
                SetProperty(ref _isReadOnlyInputSendStringTextBoxText, value);

                if (value)
                {
                    IsEnabledInputSendStringTextBoxText = false;
                }
            }
        }

        private Type TCPClient { get; set; }

        private dynamic Client { get; set; }

        private Task SurvConnectedTask { get; set; }

        private bool IsSurvConnectedRunning { get; set; }

        private EventInfo ReceivedDataEventInfo { get; set; }

        private Delegate ReceivedDataEventHandler { get; set; }

        private EventInfo DisconnectedEventInfo { get; set; }

        private Delegate DisconnectedEventHandler { get; set; }

        private EventInfo ConnectedEventInfo { get; set; }

        private Delegate ConnectedEventHandler { get; set; }

        private EventInfo SendDataEventInfo { get; set; }

        private Delegate SendDataEventHandler { get; set; }

        private bool _isEnabledClientConnectButton = true;
        public bool IsEnabledClientConnectButton
        {
            get
            {
                return _isEnabledClientConnectButton;
            }

            set
            {
                SetProperty(ref _isEnabledClientConnectButton, value);
            }
        }

        private bool _isEnabledClientDisconnectButton = true;
        public bool IsEnabledClientDisconnectButton
        {
            get
            {
                return _isEnabledClientDisconnectButton;
            }

            set
            {
                SetProperty(ref _isEnabledClientDisconnectButton, value);
            }
        }

        private bool _isEnabledClientSendButton = true;
        public bool IsEnabledClientSendButton
        {
            get
            {
                return _isEnabledClientSendButton;
            }

            set
            {
                SetProperty(ref _isEnabledClientSendButton, value);
            }
        }

        public DelegateCommand Unloaded { get; private set; }

        public DelegateCommand ClientConnectClicked { get; private set; }

        public DelegateCommand ClientDisconnectClicked { get; private set; }

        public DelegateCommand ClientSendClicked { get; private set; }

        public ClientTestWindowViewModel()
        {
            if (ImportDll())
            {
                SetEvent();
            }
            else
            {
                IsEnabledClientConnectButton = false;
                IsEnabledClientDisconnectButton = false;
                IsEnabledClientSendButton = false;
            }
        }

        private void SetEvent()
        {
            Unloaded = new DelegateCommand(OnUnload);

            ClientConnectClicked = new DelegateCommand(OnConnect);
            ClientDisconnectClicked = new DelegateCommand(OnDisconnect);
            ClientSendClicked = new DelegateCommand(OnSend);

            ReceivedDataEventHandler = new ClientReceivedDataEventHandler(OnReceivedData);
            DisconnectedEventHandler = new DisconnectedEventHandler(OnDisconnected);
            ConnectedEventHandler = new ConnectedEventHandler(OnConnected);
            SendDataEventHandler = new SendDataEventHandler(OnSendData);

            ReceivedDataEventInfo = TCPClient.GetEvent("OnClientReceivedData");
            DisconnectedEventInfo = TCPClient.GetEvent("OnClientDisconnected");
            ConnectedEventInfo = TCPClient.GetEvent("OnClientConnected");
            SendDataEventInfo = TCPClient.GetEvent("OnClientSendData");

            ReceivedDataEventInfo.AddEventHandler(Client, ReceivedDataEventHandler);
            DisconnectedEventInfo.AddEventHandler(Client, DisconnectedEventHandler);
            ConnectedEventInfo.AddEventHandler(Client, ConnectedEventHandler);
            SendDataEventInfo.AddEventHandler(Client, SendDataEventHandler);
        }

        private void OnUnload()
        {
            IsSurvConnectedRunning = false;
            SurvConnectedTask?.Wait();
            SurvConnectedTask?.Dispose();
            SurvConnectedTask = null;
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
            OutputMsgList.Add(new OutputTextModel(">>接続開始"));

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

            int connectTimeout = 0;
            if (!string.IsNullOrEmpty(InputConnectTimeoutTextBoxText) && !int.TryParse(InputConnectTimeoutTextBoxText, out connectTimeout))
            {
                OutputMsgList.Add(new OutputTextModel(">>整数で接続タイムアウトを設定してください。"));

                isValidate = false;
            }

            int receiveTimeout = 0;
            if (!string.IsNullOrEmpty(InputReceiveTimeoutTextBoxText) && !int.TryParse(InputReceiveTimeoutTextBoxText, out receiveTimeout))
            {
                OutputMsgList.Add(new OutputTextModel(">>整数で受信タイムアウトを設定してください。"));

                isValidate = false;
            }

            int reTryNum = 0;
            if (!string.IsNullOrEmpty(InputReTryNumTextBoxText) && !int.TryParse(InputReTryNumTextBoxText, out reTryNum))
            {
                OutputMsgList.Add(new OutputTextModel(">>整数でリトライ回数を設定してください。"));

                isValidate = false;
            }

            if (!isValidate)
            {
                return;
            }

            if (Client.ConnectToServerAsync(IP, port, connectTimeout, receiveTimeout, reTryNum))
            {
                OutputMsgList.Add(new OutputTextModel(">>接続成功"));

                IsReadOnlyInputSendIPTextBoxText = true;
                IsReadOnlyInputSendPortTextBoxText = true;
                IsReadOnlyInputConnectTimeoutTextBoxText = true;
                IsReadOnlyInputReceiveTimeoutTextBoxText = true;
                IsReadOnlyInputReTryNumTextBoxText = true;

                IsEnabledInputSendStringTextBoxText = true;

                IsSurvConnectedRunning = true;
                SurvConnectedTask = Task.Factory.StartNew(() =>
                {
                    SurvConnected();
                });
            }
            else
            {
                OutputMsgList.Add(new OutputTextModel(">>接続失敗"));
            }

            OutputMsgList.Add(new OutputTextModel(">>接続終了"));
        }

        private void SurvConnected()
        {
            while (IsSurvConnectedRunning)
            {
                if (!Client.IsClientConnected())
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        OutputMsgList.Add(new OutputTextModel(">>監視 切断確認"));
                    }));

                    IsEnabledInputSendIPTextBoxText = true;
                    IsEnabledInputSendPortTextBoxText = true;
                    IsEnabledInputConnectTimeoutTextBoxText = true;
                    IsEnabledInputReceiveTimeoutTextBoxText = true;
                    IsEnabledInputReTryNumTextBoxText = true;

                    IsReadOnlyInputSendStringTextBoxText = true;

                    break;
                }

                Task.Delay(5000);
            }
        }

        private void OnDisconnect()
        {
            IsSurvConnectedRunning = false;
            SurvConnectedTask?.Wait();

            OutputMsgList.Add(new OutputTextModel(">>切断開始"));

            Client.DisConnectServer();

            OutputMsgList.Add(new OutputTextModel(">>切断成功"));

            IsEnabledInputSendIPTextBoxText = true;
            IsEnabledInputSendPortTextBoxText = true;
            IsEnabledInputConnectTimeoutTextBoxText = true;
            IsEnabledInputReceiveTimeoutTextBoxText = true;
            IsEnabledInputReTryNumTextBoxText = true;

            IsReadOnlyInputSendStringTextBoxText = true;

            OutputMsgList.Add(new OutputTextModel(">>切断終了"));
        }

        private void OnSend()
        {
            OutputMsgList.Add(new OutputTextModel(">>送信開始"));

            string sendText = InputSendStringTextBoxText;
            byte[] sendData = System.Text.Encoding.UTF8.GetBytes(sendText);
            if (Client.SendToServer(sendData))
            {
                OutputMsgList.Add(new OutputTextModel(">>送信成功"));
            }
            else
            {
                OutputMsgList.Add(new OutputTextModel(">>送信失敗"));
            }

            OutputMsgList.Add(new OutputTextModel(">>送信終了"));
        }

        private void OnReceivedData(object sender, ReceivedEventArgs e)
        {
            var receivedIP = e.IP;
            var receivedPort = e.Port;
            var receivedStr = Encoding.UTF8.GetString(e.ReceivedData);

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnReceivedData start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:IP " + receivedIP));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:ポート " + receivedPort));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:送信バイト数 " + receivedStr));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnReceivedData end"));
            }));
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            var disconnectedIP = e.IP;
            var disconnectedPort = e.Port;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnDisconnected start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:IP " + disconnectedIP));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:ポート " + disconnectedPort));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnDisconnected end"));
            }));
        }

        private void OnConnected(object sender, ConnectedEventArgs e)
        {
            var connectedIP = e.IP;
            var connectedPort = e.Port;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnConnected start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:IP " + connectedIP));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:ポート " + connectedPort));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnConnected end"));
            }));
        }

        private void OnSendData(object sender, SendEventArgs e)
        {
            var sendIP = e.IP;
            var sendPort = e.Port;
            var sendByteNum = e.SendByteNum;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnSendData start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:IP " + sendIP));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:ポート " + sendPort));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:送信バイト数 " + sendByteNum));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnSendData end"));
            }));
        }
    }
}
