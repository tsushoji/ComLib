using Prism.Commands;
using Prism.Mvvm;

namespace ComLibDemo.ViewModels
{
    public class ClientTestWindowViewModel : BindableBase
    {
        public string OutputTextBoxText { get; set; } = string.Empty;

        public string InputSendIPTextBoxText { get; set; } = string.Empty;

        public string InputSendPortTextBoxText { get; set; } = string.Empty;

        public string InputSendStringTextBoxText { get; set; } = string.Empty;

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

        private bool _isEnabledClientSendDataButton = true;
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
            SetEvent();
        }

        private void SetEvent()
        {
            ClientConnectClicked = new DelegateCommand(OnConnect);
            ClientDisconnectClicked = new DelegateCommand(OnDisconnect);
            ClientSendDataClicked = new DelegateCommand(OnSendData);
        }

        private void OnConnect()
        {

        }

        private void OnDisconnect()
        {

        }

        private void OnSendData()
        {

        }
    }
}
