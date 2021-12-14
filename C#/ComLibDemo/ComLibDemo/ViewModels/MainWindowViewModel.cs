using Prism.Mvvm;

namespace ComLibDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "ComLibDemo";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
