using System.Windows.Controls;
using Core.ViewModel.Base;
using DevExpress.Mvvm;
using KursAM2.ViewModel.Finance;

namespace KursAM2.View.DialogUserControl.ViewModel
{
    public class NameNoteViewModel : RSViewModelBase
    {
        private string myHeaderName;
        private string myHeaderNote;

        public new MessageResult DialogResult = MessageResult.No;

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