using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using Helper;
using KursAM2.Managers;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Base;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.Projects;
using KursAM2.ViewModel.Management.Projects;
using KursDomain;
using KursDomain.Documents.Base;
using KursDomain.Documents.CommonReferences;
using KursDomain.Event;
using KursDomain.IDocuments.Finance;
using KursDomain.Menu;
using KursDomain.Repository;
using KursDomain.Repository.DocHistoryRepository;
using KursRepositories.Repositories.Projects;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ColumnFilterMode = DevExpress.Xpf.Grid.ColumnFilterMode;

namespace KursAM2.ViewModel.Finance.Invoices
{
    [POCOViewModel]
    public sealed class SearchInvoiceProviderViewModel : RSWindowSearchViewModelBase
    {
        //private InvoicesManager invoiceManager = new InvoicesManager();

        public readonly GenericKursDBRepository<SD_26> GenericProviderRepository;
        private readonly ISubscriber mySubscriber;
        private readonly ConnectionMultiplexer redis;

        public override bool IsDocNewCopyAllow => CurrentDocument is not null;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument is not null;
        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        private readonly IProjectRepository myRepository = new ProjectRepository(GlobalOptions.GetEntities());


        // ReSharper disable once NotAccessedField.Local
        public IInvoiceProviderRepository InvoiceProviderRepository;

        private IInvoiceProvider myCurrentDocument;

        public UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        public SearchInvoiceProviderViewModel(Window form) : base(form)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            GenericProviderRepository = new GenericKursDBRepository<SD_26>(UnitOfWork);
            InvoiceProviderRepository = new InvoiceProviderRepository(UnitOfWork);

            try
            {
                redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                mySubscriber = redis.GetSubscriber();

                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe(
                        new RedisChannel(RedisMessageChannels.InvoiceProvider, RedisChannel.PatternMode.Auto),
                        (_, message) =>
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateList(message));
                        });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.CashOut, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => UpdatePayments(message));
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.Bank, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => UpdatePayments(message));
                    });

                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.MutualAccounting, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => UpdateMutualPayments(message));
                    });
            }
            catch
            {
                Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
            }

            Form = form;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            Documents = new ObservableCollection<IInvoiceProvider>();
            SelectedDocs = new ObservableCollection<IInvoiceProvider>();
            EndDate = DateTime.Today;
            StartDate = EndDate.AddDays(-30);
            MainWindowViewModel.EventAggregator.GetEvent<AFterSaveInvoiceProvideEvent>()
                .Subscribe(OnAfterSaveInvoiceExecute);
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

        private void OnAfterSaveInvoiceExecute(AFterSaveInvoiceProvideEventArgs args)
        {
            if (Documents.FirstOrDefault(_ => _.DocCode == args.DocCode) is InvoiceProviderBase inv)
            {
                inv.Summa = args.Invoice.Summa;
                inv.CO = args.Invoice.CO;
                inv.DocDate = args.Invoice.DocDate;
                inv.FormRaschet = args.Invoice.FormRaschet;
                inv.IsAccepted = args.Invoice.IsAccepted;
                inv.IsNDSInPrice = args.Invoice.IsNDSInPrice;
                inv.KontrReceiver = args.Invoice.KontrReceiver;
                inv.Note = args.Invoice.Note;
                inv.PayCondition = args.Invoice.PayCondition;
                inv.PaySumma = args.Invoice.PaySumma;
                inv.SF_POSTAV_NUM = args.Invoice.SF_POSTAV_NUM;
            }
        }

        public override void RefreshData(object data)
        {
            var frm = Form as StandartSearchView;
            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());
            InvoiceProviderRepository =
                new InvoiceProviderRepository(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));
            Documents.Clear();
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            Task.Run(() =>
            {
                frm?.Dispatcher.Invoke(() =>
                {
                    if (frm.DataContext is SearchInvoiceProviderViewModel dtx) dtx.IsCanRefresh = false;
                    frm.loadingIndicator.Visibility = Visibility.Visible;
                });
                var result = InvoiceProviderRepository.GetAllByDates(StartDate, EndDate);
                if (result.Count > 0)
                {
                    var lasts = lastDocumentRopository.GetLastChanges(result.Select(_ => _.DocCode).Distinct());
                    foreach (var r in result)
                        if (lasts.ContainsKey(r.DocCode))
                        {
                            var last = lasts[r.DocCode];
                            r.LastChanger = last.Item1;
                            r.LastChangerDate = last.Item2;
                        }
                        else
                        {
                            r.LastChanger = r.CREATOR;
                            r.LastChangerDate = r.DocDate;
                        }
                }

                var prjs = myRepository.GetInvoicesLinkWithProjects(DocumentType.InvoiceProvider, false);
                foreach (var p in prjs)
                {
                    var item = result.FirstOrDefault(_ => _.DocCode == p.Key);
                    if (item == null) continue;
                    item.IsLinkProject = true;
                    item.LinkPrjectNames = p.Value;
                }

                frm?.Dispatcher.Invoke(() =>
                {
                    frm.loadingIndicator.Visibility = Visibility.Hidden;
                    foreach (var d in result) Documents.Add(d);
                    if (frm.DataContext is SearchInvoiceProviderViewModel dtx) dtx.IsCanRefresh = true;
                });
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            });
            frm?.gridDocuments.RefreshData();
        }

        public override void AddSearchList(object obj)
        {
            var form = new StandartSearchView { Owner = Application.Current.MainWindow };
            var dtx = new SearchInvoiceProviderViewModel(form);
            form.DataContext = dtx;
            form.Show();
        }

        protected override void DocumentOpen(object obj)
        {
            if (CurrentDocument == null) return;
            DocumentsOpenManager.Open(
                DocumentType.InvoiceProvider, CurrentDocument.DocCode);
        }

        private void UpdateList(RedisValue message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var msg = JsonConvert.DeserializeObject<RedisMessage>(message);
            if (msg == null || msg.DocCode == null) return;
            if (msg.DbId != GlobalOptions.DataBaseId) return;
            if (msg.OperationType == RedisMessageDocumentOperationTypeEnum.Open
                || (msg.DocDate ?? DateTime.Today) < StartDate || (msg.DocDate ?? DateTime.Today) > EndDate) return;
            if (msg.OperationType == RedisMessageDocumentOperationTypeEnum.Delete)
            {
                var del = Documents.FirstOrDefault(_ => _.DocCode == msg.DocCode);
                if (del != null) Documents.Remove(del);
                return;
            }

            if (msg.OperationType != RedisMessageDocumentOperationTypeEnum.Create
                && Documents.All(_ => _.DocCode != msg.DocCode)) return;

            InvoiceProviderRepository =
                new InvoiceProviderRepository(
                    new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString)));

            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());

            var dc = new List<decimal>(new[] { msg.DocCode.Value });
            var data = InvoiceProviderRepository.GetByDocCodes(dc);
            var last = lastDocumentRopository.GetLastChanges(dc);
            if (data.Count <= 0) return;
            if (last.Count > 0)
            {
                data.First().LastChanger = last.First().Value.Item1;
                data.First().LastChangerDate = last.First().Value.Item2;
            }
            else
            {
                data.First().LastChanger = data.First().CREATOR;
                data.First().LastChangerDate = data.First().DocDate;
            }

            var old = Documents.FirstOrDefault(_ => _.DocCode == msg.DocCode);
            if (old != null)
                switch (msg.OperationType)
                {
                    case RedisMessageDocumentOperationTypeEnum.Update:
                    {
                        var idx = Documents.IndexOf(old);
                        Documents[idx] = data.First();
                        break;
                    }
                    case RedisMessageDocumentOperationTypeEnum.Delete:
                        Documents.Remove(old);
                        break;
                }
            foreach (var doc in Documents)
            {
                doc.IsLinkProject = false;
                doc.LinkPrjectNames = null;
            }
            var prjs = myRepository.GetInvoicesLinkWithProjects(DocumentType.InvoiceProvider, false);
            foreach (var p in prjs)
            {
                var item = Documents.FirstOrDefault(_ => _.DocCode == p.Key);
                if (item == null) continue;
                item.IsLinkProject = true;
                item.LinkPrjectNames = p.Value;
            }
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
            var ctx = new ProviderWindowViewModel();
            ctx.Document = ctx.InvoiceProviderRepository.GetFullCopy(CurrentDocument.DocCode);
            ctx.UnitOfWork.Context.SD_26.Add(ctx.Document.Entity);
            foreach (var ent in  ctx.UnitOfWork.Context.ChangeTracker.Entries())
            {
                if (ent.Entity is SD_26 or TD_26) continue;
                ent.State = EntityState.Unchanged;
            }
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
            if (CurrentDocument == null) return;
            var ctx = new ProviderWindowViewModel();
            ctx.Document = ctx.InvoiceProviderRepository.GetRequisiteCopy(CurrentDocument.DocCode);
            ctx.UnitOfWork.Context.SD_26.Add(ctx.Document.Entity);
            foreach (var ent in  ctx.UnitOfWork.Context.ChangeTracker.Entries())
            {
                if (ent.Entity is SD_26 or TD_26) continue;
                ent.State = EntityState.Unchanged;
            }
            var frm = new InvoiceProviderView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void UpdateVisualObjects()
        {
            base.UpdateVisualObjects();
            var sMenuName = "Связать с проектами";
            if (Form is StandartSearchView frm)
            {
                var menu = frm.gridDocumentsTableView.ContextMenu;
                if (menu == null) return;
                var menus = new List<object>();
                foreach (var item in menu.Items)
                {
                    if (item is not MenuItem mItem) continue;
                    if ((string)mItem.Header == sMenuName)
                        menus.Add(item);
                }

                foreach (var m in menus)
                {
                    menu.Items.Remove(m);
                }

                menu.Items.Insert(3, new MenuItem()
                {
                    Header = "Связать с проектами",
                    Command = LinkProjectsCommand,
                    Icon = new PackIcon { Kind = PackIconKind.ExternalLink }
                });

            }
        }

        public ICommand LinkProjectsCommand
        {
            get { return new Command(LinkProjects, _ => CurrentDocument is not null); }
        }

        private void LinkProjects(object obj)
        {
            var dlg = new SelectProjectDialogView();
            var ctx = new ProjectSelectDialogWindowViewModel(DocumentType.InvoiceProvider, CurrentDocument.DocCode,
                $"№{CurrentDocument.SF_IN_NUM}/{CurrentDocument.SF_POSTAV_NUM} от {CurrentDocument.DocDate.ToShortDateString()} " +
                $"{CurrentDocument.Kontragent}.",true)
            {
                Form = dlg
            };
            dlg.DataContext = ctx;
            dlg.ShowDialog();
            if (!ctx.DialogResult) return;
            InvoiceProviderRepository.UpdateProjectsInfo(CurrentDocument.DocCode,
                ctx.Projects.Where(_ => _.IsSelected).Select(_ => _.Id),
                $"Счет-фактура поставщика №{CurrentDocument.SF_IN_NUM}/{CurrentDocument.SF_POSTAV_NUM} от {CurrentDocument.DocDate.ToShortDateString()}" +
                $" {CurrentDocument.Kontragent} {CurrentDocument.Summa:n2} {CurrentDocument.Currency}");
        }

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
            if (Form is StandartSearchView frm)
            {
                foreach (var col in frm.gridDocuments.Columns)
                {
                    if (col.FieldType != typeof(DateTime) &&
                        col.FieldType != typeof(DateTime?)) continue;
                    col.SortMode = ColumnSortMode.Value;
                    col.ColumnFilterMode = ColumnFilterMode.Value;
                    col.SortMode = ColumnSortMode.Value;
                }

                frm.gridDocumentsTableView.ShowTotalSummary = true;
                frm.gridDocumentsTableView.FormatConditions.Clear();
                var notShippedFormatCondition = new FormatCondition
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

                var shippedFormatCondition = new FormatCondition
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

        public override void OnWindowClosing(object obj)
        {
            mySubscriber?.UnsubscribeAll();
            base.OnWindowClosing(obj);
        }

        private void UpdatePayments(RedisValue message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var msg = JsonConvert.DeserializeObject<RedisMessage>(message);
            if (msg == null || msg.DocCode == null) return;
            if (msg.DbId != GlobalOptions.DataBaseId) return;
            if (!msg.ExternalValues.ContainsKey("InvoiceDC")) return;
            decimal dc;
            try
            {
                dc = Convert.ToDecimal(msg.ExternalValues["InvoiceDC"]);
            }
            catch
            {
                return;
            }

            var old = Documents.FirstOrDefault(_ => _.DocCode == dc);
            if (old == null) return;
            InvoiceProviderRepository =
                new InvoiceProviderRepository(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));
            var doc = InvoiceProviderRepository.GetByDocCode(dc);
            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());

            var last = lastDocumentRopository.GetLastChanges(new List<decimal>(new[] { dc }));
            if (last.Count > 0)
            {
                doc.LastChanger = last.First().Value.Item1;
                doc.LastChangerDate = last.First().Value.Item2;
            }
            else
            {
                doc.LastChanger = doc.CREATOR;
                doc.LastChangerDate = doc.DocDate;
            }

            var idx = Documents.IndexOf(old);
            Documents[idx] = doc;
        }

        private void UpdateMutualPayments(RedisValue message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var msg = JsonConvert.DeserializeObject<RedisMessage>(message);
            if (msg == null || msg.DocCode == null) return;
            if (msg.DbId != GlobalOptions.DataBaseId) return;
            if (!msg.ExternalValues.ContainsKey("ClientInvoiceDCList")) return;
            var arr = msg.ExternalValues["ClientInvoiceDCList"] as JArray;
            if (arr is null) return;
            var listDC = arr.Select(Convert.ToDecimal).ToList();
            foreach (var dc in listDC)
            {
                var old = Documents.FirstOrDefault(_ => _.DocCode == dc);

                InvoiceProviderRepository =
                    new InvoiceProviderRepository(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));
                var doc = InvoiceProviderRepository.GetByDocCode(dc);
                var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());
                int idx;
                if (old == null)
                {
                    Documents.Add(doc);
                    idx = Documents.IndexOf(doc);
                }
                else
                {
                    idx = Documents.IndexOf(old);
                }

                var last = lastDocumentRopository.GetLastChanges(new List<decimal>(new[] { dc }));
                if (last.Count > 0)
                {
                    doc.LastChanger = last.First().Value.Item1;
                    doc.LastChangerDate = last.First().Value.Item2;
                }
                else
                {
                    doc.LastChanger = doc.CREATOR;
                    doc.LastChangerDate = doc.DocDate;
                }

                Documents[idx] = doc;
            }
        }
    }
}
