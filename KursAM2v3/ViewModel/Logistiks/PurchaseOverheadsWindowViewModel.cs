using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using KursAM2.Managers.Invoices;
using KursDomain.Documents.Invoices;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Logistiks
{
    public class PurchaseOverheadsWindowViewModel : RSWindowViewModelBase
    {
        //private readonly InvoicesManager myInvoiceManager = new InvoiceSupplierManager();
        private InvoiceProvider myCurrentInvoice;
        private DateTime myDateEnd;
        private DateTime myDateStart;

        public PurchaseOverheadsWindowViewModel()
        {
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
        }

        public ObservableCollection<InvoiceProvider> Invoices { set; get; } =
            new ObservableCollection<InvoiceProvider>();

        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceProvider CurrentInvoice
        {
            get => myCurrentInvoice;
            set
            {
                if (Equals(myCurrentInvoice,value)) return;
                myCurrentInvoice = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DateEnd
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
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

        private void ShowError(object obj)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        #region Commands

        public ICommand AddNakladSimpleCommand
        {
            get { return new Command(ShowError, _ => true); }
        }

        public ICommand AddNakladExternalKontragentCommand
        {
            get { return new Command(ShowError, _ => true); }
        }

        public ICommand AddNakladAccountCommand
        {
            get { return new Command(ShowError, _ => true); }
        }

        public ICommand DeleteNakladCommand
        {
            get { return new Command(ShowError, _ => true); }
        }

        public override void RefreshData(object obj)
        {
            var doclist = string.IsNullOrWhiteSpace(SearchText)
                ? InvoicesManager.GetInvoicesProvider(DateStart, DateEnd, false)
                : InvoicesManager.GetInvoicesProvider(DateStart, DateEnd, false, SearchText);
            Invoices.Clear();
            foreach (var d in doclist)
                Invoices.Add(d);
        }

        #endregion
    }
}
