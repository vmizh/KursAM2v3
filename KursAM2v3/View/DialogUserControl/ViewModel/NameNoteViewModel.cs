using Core.ViewModel.Base;

namespace KursAM2.View.DialogUserControl.ViewModel
{
    public class NameNoteViewModel : RSViewModelBase
    {
        private string myHeaderName;
        private string myHeaderNote;

        public string HeaderName
        {
            get => myHeaderName;
            set
            {
                if (myHeaderName == value) return;
                myHeaderName = value;
                RaisePropertyChanged();
            }
        }

        public string HeaderNote
        {
            get => myHeaderNote;
            set
            {
                if (myHeaderNote == value) return;
                myHeaderNote = value;
                RaisePropertyChanged();
            }
        }
    }
}