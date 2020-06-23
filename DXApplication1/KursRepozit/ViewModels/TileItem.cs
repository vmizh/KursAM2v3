using DevExpress.Mvvm;
using KursRepozit.Auxiliary;

namespace KursRepozit.ViewModels
{
    public class TileItemViewModel : ViewModelBase
    {
        public string Text
        {
            get { return GetProperty(() => Text); }
            set { SetProperty(() => Text, value); }
        }
        public MainMenuEnum Id
        {
            get { return GetProperty(() => Id); }
            set { SetProperty(() => Id, value); }
        }
    }
}