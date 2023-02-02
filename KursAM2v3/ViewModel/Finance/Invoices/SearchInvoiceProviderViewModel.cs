using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Core.ViewModel.Base;
using Data;
using DevExpress.Data.Helpers;
using DevExpress.Mvvm.DataAnnotations;
using KursAM2.Managers;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Base;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.Logistiks;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.IDocuments.Finance;
using KursDomain.Menu;
using KursDomain.Repository;

namespace KursAM2.ViewModel.Finance.Invoices
{
    [POCOViewModel]
    public sealed class SearchInvoiceProviderViewModel : RSWindowSearchViewModelBase
    {
        //private InvoicesManager invoiceManager = new InvoicesManager();

        public readonly GenericKursDBRepository<SD_26> GenericProviderRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IInvoiceProviderRepository InvoiceProviderRepository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private IInvoiceProvider myCurrentDocument;
        

        public SearchInvoiceProviderViewModel()
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_26>(UnitOfWork);
            InvoiceProviderRepository = new InvoiceProviderRepository(UnitOfWork);

            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            Documents = new ObservableCollection<IInvoiceProvider>();
            SelectedDocs = new ObservableCollection<IInvoiceProvider>();
            EndDate = DateTime.Today;
            StartDate = EndDate.AddDays(-30);
        }

        public SearchInvoiceProviderViewModel(Window form) : base(form)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            GenericProviderRepository = new GenericKursDBRepository<SD_26>(UnitOfWork);
            InvoiceProviderRepository = new InvoiceProviderRepository(UnitOfWork);
            Form = form;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            Documents = new ObservableCollection<IInvoiceProvider>();
            SelectedDocs = new ObservableCollection<IInvoiceProvider>();
            EndDate = DateTime.Today;
            StartDate = EndDate.AddDays(-30);
            //LoadLayout();
        }

        public override string WindowName => "Поиск счетов-фактур поставщиков";

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<IInvoiceProvider> Documents { set; get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObservableCollection<IInvoiceProvider> SelectedDocs { set; get; }

        public override string LayoutName => "SearchInvoiceProviderViewModel";

        public IInvoiceProvider CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                if (myCurrentDocument != null)
                {
                    IsDocumentOpenAllow = true;
                    IsDocNewCopyAllow = true;
                    IsDocNewCopyRequisiteAllow = true;
                    IsPrintAllow = true;
                }

                RaisePropertyChanged();
            }
        }

        private Task Load()
        {
            var res = Task.Factory.StartNew(() =>
                new List<IInvoiceProvider>(InvoiceProviderRepository.GetAllByDates(StartDate, EndDate)));
            Documents.Clear();
            foreach (var d in res.Result) Documents.Add(d);
            RaisePropertyChanged(nameof(Documents));
            DispatcherService.BeginInvoke(SplashScreenService.HideSplashScreen);
            return res;
        }

        public override void RefreshData(object data)
        {
            List<IInvoiceProvider> dbdata = new List<IInvoiceProvider>();
            var frm = Form as StandartSearchView;
            Documents.Clear();
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            Task.Run(() =>
            {
                frm?.Dispatcher.Invoke(() => { frm.loadingIndicator.Visibility = Visibility.Visible; });
                var result = InvoiceProviderRepository.GetAllByDates(StartDate, EndDate);
                frm?.Dispatcher.Invoke(() =>
                {
                    frm.loadingIndicator.Visibility = Visibility.Hidden;
                    foreach (var d in result)
                        Documents.Add(d);
                });
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            });
        }

        public override void DocumentOpen(object obj)
        {
            if (CurrentDocument == null) return;
            DocumentsOpenManager.Open(
                DocumentType.InvoiceProvider, CurrentDocument.DocCode);
        }

        public override void DocNewEmpty(object form)
        {
            var view = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel(null)
            {
                Form = view
            };
            view.DataContext = ctx;
            view.Show();
        }

        public override void DocNewCopy(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new ProviderWindowViewModel(CurrentDocument.DocCode);
            ctx.SetAsNewCopy(true);
            var frm = new InvoiceProviderView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new ProviderWindowViewModel(CurrentDocument.DocCode);
            ctx.SetAsNewCopy(false);
            var frm = new InvoiceProviderView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }
    }
}
