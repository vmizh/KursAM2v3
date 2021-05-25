using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using DevExpress.Data.ODataLinq.Helpers;
using KursAM2.View;
using KursAM2.ViewModel.StartLogin;

namespace KursAM2.ViewModel
{
    public class MainWindowViewModel : RSWindowViewModelBase
    {
        private LastDocumentViewModel myCurrentcLastDocument;

        public MainWindowViewModel()
        {

        }

        public ObservableCollection<LastDocumentViewModel> LastDocuments { set; get; }
            = new ObservableCollection<LastDocumentViewModel>();

        public LastDocumentViewModel CurrentcLastDocument
        {
            get => myCurrentcLastDocument;
            set
            {
                if (myCurrentcLastDocument == value) return;
                myCurrentcLastDocument = value;
                RaisePropertyChanged();
            }
        }

        public ICommand OpenLastDocumentDialogCommand
        {
            get { return new Command(OpenLastDocumentDialog, _ => true); }
        }

        private void OpenLastDocumentDialog(object obj)
        {
            var dlg = new LastDocumentView();
            var ctx = new LastDocumentWindowViewModel
            {
                Form = dlg
            };
            dlg.DataContext = ctx;
            dlg.ShowDialog();

        }

        public ICommand ExpandDocumentListCommand
        {
            get { return new Command(ExpandDocumentList, _ => true); }
        }

        private void ExpandDocumentList(object obj)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                LastDocuments.Clear();
                foreach (var h in ctx.LastDocument.Where(_ => _.UserId == GlobalOptions.UserInfo.KursId
                                                              && _.DbId == GlobalOptions.DataBaseId)
                    .OrderByDescending(_ => _.LastOpen))
                {
                    LastDocuments.Add(new LastDocumentViewModel(h));
                }
            }
        }
    }
}