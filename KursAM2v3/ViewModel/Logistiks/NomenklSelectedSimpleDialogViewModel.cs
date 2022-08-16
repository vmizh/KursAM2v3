using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using KursAM2.Managers.Nomenkl;
using KursAM2.View.Logistiks.UC;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklSelectedSimpleDialogViewModel : RSWindowViewModelBase
    {
        private Nomenkl myCurrentNomenkl;
        private SelectNomenklSimpleUC myDataUserControl;

        public NomenklSelectedSimpleDialogViewModel()
        {
            myDataUserControl = new SelectNomenklSimpleUC();
        }

        public ObservableCollection<Nomenkl> NomenklCollection { set; get; } = new ObservableCollection<Nomenkl>();

        public SelectNomenklSimpleUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public override string SearchText
        {
            get => mySearchText;
            set
            {
                if (mySearchText == value) return;
                mySearchText = value;
                RaisePropertyChanged();
            }
        }

        public Nomenkl CurrentNomenkl
        {
            get => myCurrentNomenkl;
            set
            {
                if (myCurrentNomenkl != null && myCurrentNomenkl.Equals(value)) return;
                myCurrentNomenkl = value;
                RaisePropertyChanged();
            }
        }

        #region Command

        public ICommand SearchExecuteCommand
        {
            get { return new Command(SearchExecute, _ => !string.IsNullOrWhiteSpace(SearchText)); }
        }

        public void SearchExecute(object obj)
        {
            NomenklCollection.Clear();
            foreach (var n in NomenklManager.GetNomenklsSearch(SearchText))
                NomenklCollection.Add(n);
            RaisePropertyChanged(nameof(NomenklCollection));
        }

        #endregion
    }
}
