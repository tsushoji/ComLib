using ComLibDemo.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
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

            ReceiveDataEventInfo = TCPServer.GetEvent("OnServerReceiveData");
            ReceiveDataEventInfo.AddEventHandler(ServerService, ReceiveEventHandler);

            DisconnectedEventInfo = TCPServer.GetEvent("OnServerDisconnected");
            DisconnectedEventInfo.AddEventHandler(ServerService, DisconnectedEventHandler);

            ConnectedEventInfo = TCPServer.GetEvent("OnServerConnected");
            ConnectedEventInfo.AddEventHandler(ServerService, ConnectedEventHandler);

            SendEventInfo = TCPServer.GetEvent("OnServerSend");
            SendEventInfo.AddEventHandler(ServerService, SendEventHandler);

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
                    ReceiveDataEventInfo.RemoveEventHandler(ServerService, ReceiveEventHandler);
                    ReceiveDataEventInfo = null;

                    DisconnectedEventInfo.RemoveEventHandler(ServerService, DisconnectedEventHandler);
                    DisconnectedEventInfo = null;

                    ConnectedEventInfo.RemoveEventHandler(ServerService, ConnectedEventHandler);
                    ConnectedEventInfo = null;

                    SendEventInfo.RemoveEventHandler(ServerService, SendEventHandler);
                    SendEventInfo = null;

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        OutputMsgList.Add(new OutputTextModel(">>サーバー処理終了"));

                        IsEnabledInputListenPortTextBoxText = true;
                        IsEnabledInputListenBackLogTextBoxText = true;
                        IsEnabledInputReceiveTimeoutTextBoxText = true;

                        IsEnabledServerStartServiceButton = true;
                    }));

                    IsSurvStartUpServiceRunning = false;
                }
            }
        }

        private void OnReceiveData(object sender, byte[] receivedData, ref byte[] sendData, ref bool isSendAll)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnReceiveData start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnReceiveData end"));
            }));
        }

        private void OnDisconnected(object sender, EventArgs e, EndPoint disconnectedEndPoint)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnDisconnected start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnDisconnected end"));
            }));
        }

        private void OnConnected(object sender, EventArgs e, EndPoint connectedEndPoint)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnConnected start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnConnected end"));
            }));
        }

        private void OnSend(int byteSize, EndPoint sendEndPoint)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnSend start"));
                OutputMsgList.Add(new OutputTextModel(">>TCPServer:OnSend end"));
            }));
        }

        private void OnEndService()
        {
            ServerService.EndService();
        }
    }
}
