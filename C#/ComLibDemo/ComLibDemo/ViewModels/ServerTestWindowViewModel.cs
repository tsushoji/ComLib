using Prism.Commands;
using Prism.Mvvm;

namespace ComLibDemo.ViewModels
{
    public class ServerTestWindowViewModel : BindableBase
    {
        public string OutputTextBoxText { get; set; } = string.Empty;

        public string InputListenPortTextBoxText { get; set; } = string.Empty;

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
            }
        }

        private bool _isEnabledServerEndServiceButton = true;
        public bool IsEnabledServerEndServiceButton
        {
            get
            {
                return _isEnabledServerEndServiceButton;
            }

            set
            {
                SetProperty(ref _isEnabledServerEndServiceButton, value);
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
        }

        private void OnStartService()
        {

        }

        private void OnEndService()
        {

        }
    }
}
