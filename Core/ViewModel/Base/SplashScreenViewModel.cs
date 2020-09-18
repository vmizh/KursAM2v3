using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.ViewModel.Base
{
    public class SplashScreenViewModel : ViewModelBase
    {
        public string Caption
        {
            get { return GetProperty(() => Caption); }
            set { SetProperty(() => Caption, value); }
        }

        public bool IsShown
        {
            get { return GetProperty(() => IsShown); }
            set { SetProperty(() => IsShown, value); }
        }

        [Command]
        public void ChangeText()
        {
            Caption = Caption;
        }
    }
}