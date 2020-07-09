using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using DevExpress.Mvvm.DataAnnotations;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.View.Finance.Invoices;

namespace KursAM2.ViewModel.Finance.Invoices
{
    [POCOViewModel]
    public class SearchInvoiceProviderViewModel : RSWindowSearchViewModelBase
    {
        private InvoiceProvider myCurrentDocument;
        private DateTime myDateEnd;
        private DateTime myDateStart;
        private bool myIsEnabled;

        public SearchInvoiceProviderViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            Documents = new ObservableCollection<InvoiceProvider>();
            SelectedDocs = new ObservableCollection<InvoiceProvider>();
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
        }
        public SearchInvoiceProviderViewModel(Window form) : base(form)
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            Documents = new ObservableCollection<InvoiceProvider>();
            SelectedDocs = new ObservableCollection<InvoiceProvider>();
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
        }

        public DateTime DateEnd
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                RaisePropertyChanged();
                if (myDateStart < myDateEnd) return;
                myDateStart = myDateEnd;
                RaisePropertyChanged(nameof(DateStart));
            }
        }

        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                RaisePropertyChanged();
                if (myDateStart <= myDateEnd) return;
                myDateEnd = myDateStart;
                RaisePropertyChanged(nameof(DateEnd));
            }
        }

        public ObservableCollection<InvoiceProvider> Documents { set; get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObservableCollection<InvoiceProvider> SelectedDocs { set; get; }

        public InvoiceProvider CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument != null && myCurrentDocument.Equals(value)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => myIsEnabled;
            set
            {
                if (myIsEnabled == value) return;
                myIsEnabled = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public override void RefreshData(object data)
        {
            base.RefreshData(null);
            IsPrintAllow = false;
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

            Documents.Clear();
            if (IsEnabled)
                foreach (var d in InvoicesManager.GetInvoicesProvider(DateStart, DateEnd, false))
                    Documents.Add(d);
            else
                foreach (var d in InvoicesManager.GetInvoicesProvider(false, false))
                    Documents.Add(d);
            RaisePropertyChanged(nameof(Documents));
        }

        public override void DocumentOpen(object obj)
        {
            DocumentsOpenManager.Open(
                DocumentType.InvoiceProvider, CurrentDocument.DOC_CODE);
        }

        public override void DocNewEmpty(object form)
        {
            var view = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel
            {
                Form = view,
                Document = InvoicesManager.NewProvider()
            };
            view.Show();
            view.DataContext = ctx;
        }

        public override void DocNewCopy(object form)
        {
            if (CurrentDocument == null) return;
            var document = InvoicesManager.GetInvoiceProvider(CurrentDocument.DocCode);
            if (document == null) return;
            var frm = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel
            {
                Form = frm, Document = InvoicesManager.NewProviderCopy(document.DocCode)
            };
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (CurrentDocument == null) return;
            var document = InvoicesManager.GetInvoiceProvider(CurrentDocument.DocCode);
            if (document == null) return;
            var frm = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel
            {
                Form = frm, Document = InvoicesManager.NewProviderRequisite(document.DocCode)
            };
            frm.Show();
            frm.DataContext = ctx;
        }
    }
}