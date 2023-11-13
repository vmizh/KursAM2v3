using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Managers;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Base;
using KursAM2.View.Finance.Invoices;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.IDocuments.Finance;
using KursDomain.Menu;
using KursDomain.Repository;
using ConditionRule = DevExpress.Xpf.Core.ConditionalFormatting.ConditionRule;

namespace KursAM2.ViewModel.Finance.Invoices
{
    [POCOViewModel]
    public sealed class SearchInvoiceProviderViewModel : RSWindowSearchViewModelBase
    {
        //private InvoicesManager invoiceManager = new InvoicesManager();

        public readonly GenericKursDBRepository<SD_26> GenericProviderRepository;

        // ReSharper disable once NotAccessedField.Local
        public IInvoiceProviderRepository InvoiceProviderRepository;

        private IInvoiceProvider myCurrentDocument;

        public UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));


        public SearchInvoiceProviderViewModel()
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_26>(UnitOfWork);
            InvoiceProviderRepository = new InvoiceProviderRepository(UnitOfWork);

            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            Documents = new ObservableCollection<IInvoiceProvider>();
            SelectedDocs = new ObservableCollection<IInvoiceProvider>();
            EndDate = DateTime.Today;
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

        public override void RefreshData(object data)
        {
            var frm = Form as StandartSearchView;
            InvoiceProviderRepository =
                new InvoiceProviderRepository(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));
            Documents.Clear();
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            Task.Run(() =>
            {
                frm?.Dispatcher.Invoke(() => { frm.loadingIndicator.Visibility = Visibility.Visible; });
                var result = InvoiceProviderRepository.GetAllByDates(StartDate, EndDate);
                frm?.Dispatcher.Invoke(() =>
                {
                    frm.loadingIndicator.Visibility = Visibility.Hidden;
                    foreach (var d in result) Documents.Add(d);
                });
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            });
            frm?.gridDocuments.RefreshData();
        }

        public override void DocumentOpen(object obj)
        {
            if (CurrentDocument == null) return;
            DocumentsOpenManager.Open(
                DocumentType.InvoiceProvider, CurrentDocument.DocCode);
        }

        public override void DocNewEmpty(object form)
        {
            var view = new InvoiceProviderView { Owner = Application.Current.MainWindow };
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

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
            if (Form is StandartSearchView frm)
            {
                frm.gridDocumentsTableView.ShowTotalSummary = true;
                frm.gridDocumentsTableView.FormatConditions.Clear();
                var notShippedFormatCondition = new FormatCondition()
                {
                    //Expression = "[SummaFact] < [Summa]",
                    FieldName = "SummaFact",
                    ApplyToRow = true,
                    Format = new Format
                    {
                        Foreground = Brushes.Red
                    },
                    ValueRule = ConditionRule.Equal,
                    Value1 = 0m
                };
                
                var shippedFormatCondition = new FormatCondition()
                {
                    Expression = "[Summa] > [SummaFact]",
                    FieldName = "SummaFact",
                    ApplyToRow = true,
                    Format = new Format
                    {
                        Foreground = Brushes.Blue
                    }
                };
                frm.gridDocumentsTableView.FormatConditions.Add(shippedFormatCondition);
                frm.gridDocumentsTableView.FormatConditions.Add(notShippedFormatCondition);
            }
            StartDate = DateHelper.GetFirstDate();
        }
    }
}
