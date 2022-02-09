using System.Windows.Controls;
using Core.ViewModel.Base;
using DevExpress.Mvvm;

namespace KursAM2.View.DialogUserControl.ViewModel
{
    public class NameNoteViewModel : RSViewModelBase
    {
        public MessageResult DialogResult = MessageResult.No;
        private string myHeaderName;
        private string myHeaderNote;

        public UserControl CustomDataUserControl { set; get; } = new NameNoteUC();

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