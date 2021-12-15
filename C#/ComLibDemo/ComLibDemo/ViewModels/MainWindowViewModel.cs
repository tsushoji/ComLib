using ComLibDemo.Views;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows;

namespace ComLibDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public DelegateCommand ClientTestButtonClicked { get; private set; }

        public DelegateCommand ServerTestButtonClicked { get; private set; }

        public MainWindowViewModel()
        {
            SetEvent();
        }

        private void SetEvent()
        {
            ClientTestButtonClicked = new DelegateCommand(OnShowClientTestView);
            ServerTestButtonClicked = new DelegateCommand(OnShowServerTestView);
        }

        private void OnShowClientTestView()
        {
            var clientTestView = new ClientTestWindow();
            clientTestView.Owner = Application.Current.MainWindow as MainWindow;
            clientTestView.ShowDialog();
        }

        private void OnShowServerTestView()
        {
            var serverTestView = new ServerTestWindow();
            serverTestView.Owner = Application.Current.MainWindow as MainWindow;
            serverTestView.ShowDialog();
        }
    }
}
