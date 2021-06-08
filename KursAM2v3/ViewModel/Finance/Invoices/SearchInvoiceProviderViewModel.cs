using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using KursAM2.Managers;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Finance.Invoices;

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

        private InvoiceProvider myCurrentDocument;
        private DateTime myDateEnd;
        private DateTime myDateStart;

        public SearchInvoiceProviderViewModel()
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_26>(UnitOfWork);
            InvoiceProviderRepository = new InvoiceProviderRepository(UnitOfWork);

            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            Documents = new ObservableCollection<InvoiceProvider>();
            SelectedDocs = new ObservableCollection<InvoiceProvider>();
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
        }

        public SearchInvoiceProviderViewModel(Window form) : base(form)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            GenericProviderRepository = new GenericKursDBRepository<SD_26>(UnitOfWork);
            InvoiceProviderRepository = new InvoiceProviderRepository(UnitOfWork);
            Form = form;
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

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<InvoiceProvider> Documents { set; get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObservableCollection<InvoiceProvider> SelectedDocs { set; get; }

        public override string LayoutName => "SearchInvoiceProviderViewModel";

        public InvoiceProvider CurrentDocument
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
                new List<InvoiceProvider>(InvoiceProviderRepository.GetAllByDates(DateStart, DateEnd)));
            Documents.Clear();
            foreach (var d in res.Result) Documents.Add(d);
            RaisePropertyChanged(nameof(Documents));
            DispatcherService.BeginInvoke(SplashScreenService.HideSplashScreen);
            return res;
        }

        public override async void RefreshData(object data)
        {
            SplashScreenService.ShowSplashScreen();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

            base.RefreshData(null);
            await Load();
        }

        public override void DocumentOpen(object obj)
        {
            if (CurrentDocument == null) return;
            DocumentsOpenManager.Open(
                DocumentType.InvoiceProvider, CurrentDocument.DOC_CODE);
        }

        public override void DocNewEmpty(object form)
        {
            var view = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel(null)
            {
                Form = view
            };
            view.Show();
            view.DataContext = ctx;
        }

        public override void DocNewCopy(object form)
        {
            if (CurrentDocument == null) return;
            var frm = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel {Form = frm};
            ctx.Document = new InvoiceProvider(ctx.GenericProviderRepository.GetById(CurrentDocument.DocCode),
                ctx.UnitOfWork);
            ctx.GenericProviderRepository.DetachObjects();
            var id = Guid.NewGuid();
            ctx.Document.Id = id;
            ctx.Document.DocCode = -1;
            ctx.Document.SF_POSTAV_NUM = null;
            ctx.Document.SF_POSTAV_DATE = DateTime.Today;
            ctx.Document.SF_REGISTR_DATE = DateTime.Today;
            ctx.Document.CREATOR = GlobalOptions.UserInfo.Name;
            ctx.Document.myState = RowStatus.NewRow;
            ctx.Document.PaymentDocs.Clear();
            ctx.Document.Facts.Clear();
            ctx.Document.IsAccepted = false;
            var code = 1;
            foreach (var row in ctx.Document.Rows)
            {
                row.DocCode = -1;
                row.Id = Guid.NewGuid();
                row.DocId = id;
                row.Code = code;
                row.myState = RowStatus.NewRow;
                code++;
            }

            ctx.Document.DeletedRows.Clear();
            ctx.Document.PaymentDocs.Clear();
            ctx.Document.Facts.Clear();
            ctx.UnitOfWork.Context.SD_26.Add(ctx.Document.Entity);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (CurrentDocument == null) return;
            var frm = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel {Form = frm};
            ctx.Document = new InvoiceProvider(ctx.GenericProviderRepository.GetById(CurrentDocument.DocCode),
                ctx.UnitOfWork);
            ctx.GenericProviderRepository.DetachObjects();
            ctx.Document.DocCode = -1;
            ctx.Document.Entity.SF_CRS_SUMMA = 0;
            ctx.Document.Id = Guid.NewGuid();
            ctx.Document.SF_POSTAV_NUM = null;
            ctx.Document.SF_POSTAV_DATE = DateTime.Today;
            ctx.Document.SF_REGISTR_DATE = DateTime.Today;
            ctx.Document.CREATOR = GlobalOptions.UserInfo.Name;
            ctx.Document.myState = RowStatus.NewRow;
            ctx.Document.PaymentDocs.Clear();
            ctx.Document.Facts.Clear();
            ctx.Document.IsAccepted = false;
            ctx.Document.DeletedRows.Clear();
            ctx.Document.PaymentDocs.Clear();
            ctx.Document.Facts.Clear();
            ctx.Document.Rows.Clear();
            ctx.UnitOfWork.Context.SD_26.Add(ctx.Document.Entity);
            frm.Show();
            frm.DataContext = ctx;
        }
    }
}