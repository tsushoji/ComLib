using ComLibDemo.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using static ComTCP.TCPClient;

namespace ComLibDemo.ViewModels
{
    public class ClientTestWindowViewModel : BindableBase
    {
        public ObservableCollection<OutputTextModel> OutputMsgList { get; set; } = new ObservableCollection<OutputTextModel>();

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

        private bool IsSurvConnectedRunning { get; set; } = false;

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

            ReceiveDataEventInfo.AddEventHandler(Client, ReceiveEventHandler);

            if (Client.Connect(IP, port, connectTimeout, receiveTimeout, reTryNum))
            {
                OutputMsgList.Add(new OutputTextModel(">>接続成功"));

                IsReadOnlyInputSendIPTextBoxText = true;
                IsReadOnlyInputSendPortTextBoxText = true;
                IsReadOnlyInputConnectTimeoutTextBoxText = true;
                IsReadOnlyInputReceiveTimeoutTextBoxText = true;
                IsReadOnlyInputReTryNumTextBoxText = true;

                IsEnabledInputSendStringTextBoxText = true;

                IsEnabledClientDisconnectButton = true;
                IsEnabledClientSendDataButton = true;

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

            ConnectEventInfo.RemoveEventHandler(Client, ConnectEventHandler);
        }

        private void OnConnected(object sender, EventArgs e, IPEndPoint connectedEndPoint)
        {
            var addres = connectedEndPoint.Address.ToString();
            var port = connectedEndPoint.Port.ToString();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnConnected start"));
                OutputMsgList.Add(new OutputTextModel($">>アドレス:{addres}"));
                OutputMsgList.Add(new OutputTextModel($">>ポート:{port}"));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnConnected end"));
            }));
        }

        private void SurvConnected()
        {
            while (IsSurvConnectedRunning)
            {
                if (!Client.GetConnectedStatus())
                {
                    ReceiveDataEventInfo.RemoveEventHandler(Client, ReceiveEventHandler);

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        OutputMsgList.Add(new OutputTextModel(">>切断"));

                        IsEnabledInputSendIPTextBoxText = true;
                        IsEnabledInputSendPortTextBoxText = true;
                        IsEnabledInputConnectTimeoutTextBoxText = true;
                        IsEnabledInputReceiveTimeoutTextBoxText = true;
                        IsEnabledInputReTryNumTextBoxText = true;

                        IsReadOnlyInputSendStringTextBoxText = true;

                        IsEnabledClientConnectButton = true;
                    }));

                    IsSurvConnectedRunning = false;
                }
            }
        }

        private void OnDisconnect()
        {
            IsSurvConnectedRunning = false;
            SurvConnectedTask.Wait();

            Client.DisConnect();

            ReceiveDataEventInfo.RemoveEventHandler(Client, ReceiveEventHandler);

            IsEnabledInputSendIPTextBoxText = true;
            IsEnabledInputSendPortTextBoxText = true;
            IsEnabledInputConnectTimeoutTextBoxText = true;
            IsEnabledInputReceiveTimeoutTextBoxText = true;
            IsEnabledInputReTryNumTextBoxText = true;

            IsReadOnlyInputSendStringTextBoxText = true;

            IsEnabledClientConnectButton = true;
        }

        private void OnSendData()
        {
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
        }

        private void OnReceiveData(object sender, byte[] receivedData)
        {
            var data = System.Text.Encoding.UTF8.GetString(receivedData);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnReceiveData start"));
                OutputMsgList.Add(new OutputTextModel(">>受信データ"));
                OutputMsgList.Add(new OutputTextModel($">>{data}"));
                OutputMsgList.Add(new OutputTextModel(">>TCPClient:OnReceiveData end"));
            }));
        }
    }
}
