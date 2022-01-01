using ComLibDemo.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static ComTCP.TCPComBase;
using static ComTCP.TCPServer;

namespace ComLibDemo.ViewModels
{
    public class ServerTestWindowViewModel : BindableBase
    {
        public ObservableCollection<OutputTextModel> OutputMsgList { get; set; } = new ObservableCollection<OutputTextModel>();

        public string InputListenPortTextBoxText { get; set; } = string.Empty;

        private bool _isEnabledInputListenPortTextBoxText = true;
        public bool IsEnabledInputListenPortTextBoxText
        {
            get
            {
                return _isEnabledInputListenPortTextBoxText;
            }

            set
            {
                SetProperty(ref _isEnabledInputListenPortTextBoxText, value);

                if (value)
                {
                    IsReadOnlyInputListenPortTextBoxText = false;
                }
            }
        }

        private bool _isReadOnlyInputListenPortTextBoxText = false;
        public bool IsReadOnlyInputListenPortTextBoxText
        {
            get
            {
                return _isReadOnlyInputListenPortTextBoxText;
            }

            set
            {
                SetProperty(ref _isReadOnlyInputListenPortTextBoxText, value);

                if (value)
                {
                    IsEnabledInputListenPortTextBoxText = false;
                }
            }
        }

        public string InputListenBackLogTextBoxText { get; set; } = string.Empty;

        private bool _isEnabledInputListenBackLogTextBoxText = true;
        public bool IsEnabledInputListenBackLogTextBoxText
        {
            get
            {
                return _isEnabledInputListenBackLogTextBoxText;
            }

            set
            {
                SetProperty(ref _isEnabledInputListenBackLogTextBoxText, value);

                if (value)
                {
                    IsReadOnlyInputListenBackLogTextBoxText = false;
                }
            }
        }

        private bool _isReadOnlyInputListenBackLogTextBoxText = false;
        public bool IsReadOnlyInputListenBackLogTextBoxText
        {
            get
            {
                return _isReadOnlyInputListenBackLogTextBoxText;
            }

            set
            {
                SetProperty(ref _isReadOnlyInputListenBackLogTextBoxText, value);

                if (value)
                {
                    IsEnabledInputListenBackLogTextBoxText = false;
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

        private Type TCPServer { get; set; }

        private dynamic ServerService { get; set; }

        private Task SurvStartUpServiceTask { get; set; }

        private bool IsSurvStartUpServiceRunning { get; set; } = false;

        private EventInfo ReceivedDataEventInfo { get; set; }

        private Delegate ReceivedDataEventHandler { get; set; }

        private EventInfo DisconnectedEventInfo { get; set; }

        private Delegate DisconnectedEventHandler { get; set; }

        private EventInfo ConnectedEventInfo { get; set; }

        private Delegate ConnectedEventHandler { get; set; }

        private EventInfo SendDataEventInfo { get; set; }

        private Delegate SendDataEventHandler { get; set; }

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

            ReceivedDataEventHandler = new ServerReceivedDataEventHandler(OnReceivedData);
            DisconnectedEventHandler = new DisconnectedEventHandler(OnDisconnected);
            ConnectedEventHandler = new ConnectedEventHandler(OnConnected);
            SendDataEventHandler = new SendDataEventHandler(OnSendData);
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
            bool isValidate = true;

            int port;
            if (!ParseHelper.TryParsePositeviNumStr(InputListenPortTextBoxText, out port))
            {
                OutputMsgList.Add(new OutputTextModel(">>正の数でポートを設定してください。"));

                isValidate = false;
            }

            int listenBackLog;
            if (!ParseHelper.TryParsePositeviNumStr(InputListenBackLogTextBoxText, out listenBackLog))
            {
                OutputMsgList.Add(new OutputTextModel(">>正の数でリッスンバックログ最大長を設定してください。"));

                isValidate = false;
            }

            int receiveTimeout;
            if (!ParseHelper.TryParsePositeviNumStr(InputReceiveTimeoutTextBoxText, out receiveTimeout))
            {
                OutputMsgList.Add(new OutputTextModel(">>正の数で受信タイムアウトを設定してください。"));

                isValidate = false;
            }

            if (!isValidate)
            {
                return;
            }

            if (!ImportDll(port))
            {
                return;
            }

            ReceivedDataEventInfo = TCPServer.GetEvent("OnServerReceivedData");
            ReceivedDataEventInfo.AddEventHandler(ServerService, ReceivedDataEventHandler);

            DisconnectedEventInfo = TCPServer.GetEvent("OnServerDisconnected");
            DisconnectedEventInfo.AddEventHandler(ServerService, DisconnectedEventHandler);

            ConnectedEventInfo = TCPServer.GetEvent("OnServerConnected");
            ConnectedEventInfo.AddEventHandler(ServerService, ConnectedEventHandler);

            SendDataEventInfo = TCPServer.GetEvent("OnServerSendData");
            SendDataEventInfo.AddEventHandler(ServerService, SendDataEventHandler);

            ServerService.StartService(listenBackLog, receiveTimeout);

            OutputMsgList.Add(new OutputTextModel(">>サーバー処理開始"));

            IsReadOnlyInputListenPortTextBoxText = true;
            IsReadOnlyInputListenBackLogTextBoxText = true;
            IsReadOnlyInputReceiveTimeoutTextBoxText = true;

            IsEnabledServerEndServiceButton = true;

            IsSurvStartUpServiceRunning = true;
            SurvStartUpServiceTask = Task.Factory.StartNew(() =>
            {
                SurvConnected();
            });
        }

        private void SurvConnected()
        {
            while (IsSurvStartUpServiceRunning)
            {
                if (!ServerService.IsServiceRunning())
                {
                    ReceivedDataEventInfo.RemoveEventHandler(ServerService, ReceivedDataEventHandler);
                    ReceivedDataEventInfo = null;

                    DisconnectedEventInfo.RemoveEventHandler(ServerService, DisconnectedEventHandler);
                    DisconnectedEventInfo = null;

                    ConnectedEventInfo.RemoveEventHandler(ServerService, ConnectedEventHandler);
                    ConnectedEventInfo = null;

                    SendDataEventInfo.RemoveEventHandler(ServerService, SendDataEventHandler);
                    SendDataEventInfo = null;

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        OutputMsgList.Add(new OutputTextModel(">>サーバー処理終了"));
                    }));

                    IsEnabledInputListenPortTextBoxText = true;
                    IsEnabledInputListenBackLogTextBoxText = true;
                    IsEnabledInputReceiveTimeoutTextBoxText = true;

                    IsEnabledServerStartServiceButton = true;

                    IsSurvStartUpServiceRunning = false;
                }
            }
        }

        private void OnReceivedData(object sender, ServerReceivedEventArgs e, ref byte[] sendData, ref bool isSendAll)
        {
            var receivedIP = e.IP;
            var receivedPort = e.Port;
            var receivedStr = Encoding.UTF8.GetString(e.ReceivedData);

            SetSendData(receivedStr, ref sendData, ref isSendAll);

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnReceivedData start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:IP " + receivedIP));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:ポート " + receivedPort));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:送信バイト数 " + receivedStr));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnReceivedData end"));
            }));
        }

        private void SetSendData(string str, ref byte[] sendData, ref bool isSendAll)
        {
            switch (str)
            {
                case "test1":
                    sendData = Encoding.UTF8.GetBytes(str);
                    isSendAll = false;
                    break;

                case "test2":
                    sendData = Encoding.UTF8.GetBytes(str);
                    isSendAll = true;
                    break;

                default:
                    break;
            }
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            var disconnectedIP = e.IP;
            var disconnectedPort = e.Port;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnDisconnected start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:IP " + disconnectedIP));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:ポート " + disconnectedPort));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnDisconnected end"));
            }));
        }

        private void OnConnected(object sender, ConnectedEventArgs e)
        {
            var connectedIP = e.IP;
            var connectedPort = e.Port;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnConnected start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:IP " + connectedIP));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:ポート " + connectedPort));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnConnected end"));
            }));
        }

        private void OnSendData(object sender, SendEventArgs e)
        {
            var sendIP = e.IP;
            var sendPort = e.Port;
            var sendByteNum = e.SendByteNum;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnSendData start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:IP " + sendIP));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:ポート " + sendPort));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:送信バイト数 " + sendByteNum));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnSendData end"));
            }));
        }

        private void OnEndService()
        {
            ServerService.EndService();
        }
    }
}
