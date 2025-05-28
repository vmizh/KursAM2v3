using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using Helper;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Base;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.Projects;
using KursAM2.ViewModel.Finance.Invoices.Base;
using KursAM2.ViewModel.Management.Projects;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Invoices;
using KursDomain.IDocuments.Finance;
using KursDomain.Menu;
using KursDomain.Repository;
using KursDomain.Repository.DocHistoryRepository;
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
    public sealed class InvoiceClientSearchViewModel : RSWindowSearchViewModelBase
    {
        public readonly GenericKursDBRepository<SD_84> GenericProviderRepository;
        private readonly ISubscriber mySubscriber;
        private readonly ConnectionMultiplexer redis;

        public override bool IsDocNewCopyAllow => CurrentDocument is not null;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument is not null;
        public override bool IsDocumentOpenAllow => CurrentDocument != null;
        
        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        public IInvoiceClientRepository InvoiceClientRepository;
        private IInvoiceClient myCurrentDocument;

        public InvoiceClientSearchViewModel(Window form) : base(form)
        {
            try
            {
                redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                mySubscriber = redis.GetSubscriber();
                if (mySubscriber.IsConnected())
                {
                    mySubscriber.Subscribe(
                        new RedisChannel(RedisMessageChannels.InvoiceClient, RedisChannel.PatternMode.Auto),
                        (_, message) =>
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateList(message));
                        });
                    mySubscriber.Subscribe(
                        new RedisChannel(RedisMessageChannels.CashIn, RedisChannel.PatternMode.Auto),
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
            }
            catch
            {
                Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
            }

            GenericProviderRepository = new GenericKursDBRepository<SD_84>(UnitOfWork);
            InvoiceClientRepository = new InvoiceClientRepository(UnitOfWork);
            WindowName = "Счета фактуры для клиентов";
            Documents = new ObservableCollection<IInvoiceClient>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            EndDate = DateTime.Today;
            StartDate = DateHelper.GetFirstDate();
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            if (prn != null)
            {
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Заказ",
                    Command = PrintZakazCommand
                });
                var schet = new MenuButtonInfo
                {
                    Caption = "Счет"
                    //Command = PrintSFSchetCommand
                };
                schet.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Печать",
                    Command = PrintSFSchetCommand
                });
                schet.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportSFCommand
                });
                prn.SubMenu.Add(schet);
                var sf = new MenuButtonInfo
                {
                    Caption = "Счет фактура"
                    //Command = PrintSFCommand
                };
                sf.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Печать",
                    Command = PrintSFCommand
                });
                sf.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportSFCommand
                });
                prn.SubMenu.Add(sf);
            }
        }

        public override string LayoutName => "InvoiceClientSearchViewModel";

        public IInvoiceClient CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
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

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<IInvoiceClient> Documents { set; get; }

        public Command ExportSFCommand
        {
            get { return new Command(ExportSF, _ => IsDocumentOpenAllow); }
        }

        public Command PrintSFCommand
        {
            get { return new Command(PrintSF, _ => IsDocumentOpenAllow); }
        }

        public Command PrintZakazCommand
        {
            get { return new Command(PrintZakaz, _ => true); }
        }

        public override bool IsPrintAllow => CurrentDocument != null;

        public Command PrintSFSchetCommand
        {
            get { return new Command(PrintSChet, _ => true); }
        }

        private void PrintSChet(object obj)
        {
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.PrintSChet(null);
        }

        private void PrintZakaz(object obj)
        {
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.PrintZakaz(null);
        }

        private void ExportSF(object obj)
        {
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.ExportSF(null);
        }

        private void PrintSF(object obj)
        {
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.PrintSF(null);
        }

        public override void OnWindowClosing(object obj)
        {
            mySubscriber?.UnsubscribeAll();
            base.OnWindowClosing(obj);
        }

        public override void AddSearchList(object obj)
        {
            var form = new StandartSearchView { Owner = Application.Current.MainWindow };
            var ctxsf = new InvoiceClientSearchViewModel(form);
            form.DataContext = ctxsf;
            form.Show();
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
                    FieldName = "SummaOtgruz",
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
                    Expression = "[Summa] > [SummaOtgruz] and [SummaOtgruz] != 0m",
                    FieldName = "SummaOtgruz",
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

        public override void UpdateVisualObjects()
        {
            base.UpdateVisualObjects();
            if (Form is StandartSearchView frm)
            {
                var menu = frm.gridDocumentsTableView.ContextMenu;
                if (menu != null)
                {
                    menu.Items.Insert(3, new MenuItem()
                    {
                        Header = "Связать с проектами", 
                        Command = LinkProjectsCommand,
                        Icon = new PackIcon { Kind = PackIconKind.ExternalLink }
                    });
                }
            }
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
                
                InvoiceClientRepository =
                    new InvoiceClientRepository(
                        new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString)));
                var doc = InvoiceClientRepository.GetByDocCode(dc);
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
            InvoiceClientRepository =
                new InvoiceClientRepository(
                    new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString)));
            var doc = InvoiceClientRepository.GetByDocCode(dc);
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

            InvoiceClientRepository =
                new InvoiceClientRepository(
                    new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString)));

            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());

            var dc = new List<decimal>([msg.DocCode.Value]);
            var data = InvoiceClientRepository.GetByDocCodes(dc);
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
            //Documents.Add(data.First());
        }

        #region Commands

        public ICommand LinkProjectsCommand
        {
            get { return new Command(LinkProjects, _ => CurrentDocument is not null); }
        }

        private void LinkProjects(object obj)
        {
            var dlg = new SelectProjectDialogView();
            var ctx = new ProjectSelectDialogWindowViewModel(DocumentType.InvoiceClient, CurrentDocument.DocCode,
                $"№{CurrentDocument.InnerNumber}/{CurrentDocument.OuterNumber} от {CurrentDocument.DocDate.ToShortDateString()} " +
                $"{CurrentDocument.Client}.",false)
            {
                Form = dlg
            };
            dlg.DataContext = ctx;
            dlg.ShowDialog();
            if (!ctx.DialogResult) return;
            //InvoiceClientRepository.UpdateProjectsInfo(CurrentDocument.DocCode,
            //    ctx.Projects.Where(_ => _.IsSelected).Select(_ => _.Id),
            //    $"Валютная конвертация СФ №{CurrentDocument.InnerNumber}/{CurrentDocument.OuterNumber} от {CurrentDocument.DocDate.ToShortDateString()}" +
            //    $" {CurrentDocument.Client} {CurrentDocument.Summa:n2} {CurrentDocument.Currency}");

        }

        public override void Search(object obj)
        {
            GetSearchDocument(obj);
        }

        private void Delete(object obj)
        {
            if (CurrentDocument != null)
                InvoicesManager.DeleteProvider(CurrentDocument.DocCode);
            RefreshData(null);
        }

        public void GetSearchDocument(object obj)
        {
            try
            {
                Documents.Clear();
                foreach (var item in InvoiceClientRepository.GetSearch(StartDate, EndDate))
                {
                    var newItem = new InvoiceClientViewModel(item);
                    string d;
                    d = newItem.Diler != null ? newItem.Diler.Name : "";
                    if (newItem.InnerNumber.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                        newItem.OuterNumber.ToUpper().Contains(SearchText.ToUpper()) ||
                        newItem.SF_CLIENT_NAME.ToUpper().Contains(SearchText.ToUpper()) ||
                        newItem.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                        newItem.SF_DILER_SUMMA.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                        newItem.CO.Name.ToUpper().Contains(SearchText.ToUpper()) ||
                        // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                        newItem.Summa.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                        d.ToUpper().Contains(SearchText.ToUpper()))
                        Documents.Add(newItem);
                }

            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public ICommand DeleteCommand
        {
            get { return new Command(Delete, _ => true); }
        }

        public override void SearchClear(object obj)
        {
            base.SearchClear(obj);
            RefreshData(null);
        }

        public override void RefreshData(object data)
        {
            InvoiceClientRepository =
                new InvoiceClientRepository(
                    new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString)));
            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());
            var frm = Form as StandartSearchView;
            Documents.Clear();
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            Task.Run(() =>
            {
                frm?.Dispatcher.Invoke(() => { frm.loadingIndicator.Visibility = Visibility.Visible; });
                var result = InvoiceClientRepository.GetAllByDates(StartDate, EndDate);
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

                frm?.Dispatcher.Invoke(() =>
                {
                    frm.loadingIndicator.Visibility = Visibility.Hidden;
                    foreach (var d in result)
                    {
                        ((InvoiceClientBase)d).RaisePropertyAllChanged();
                        Documents.Add(d);
                    }
                });
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            });
            RaisePropertyChanged(nameof(Documents));
        }

        protected override void DocumentOpen(object form)
        {
            if (CurrentDocument == null) return;
            DocumentsOpenManager.Open(DocumentType.InvoiceClient, CurrentDocument.DocCode);
        }

        public override void DocNewEmpty(object form)
        {
            var ctx = new ClientWindowViewModel(null);
            var frm = new InvoiceClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.SetAsNewCopy(true);
            var frm = new InvoiceClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.SetAsNewCopy(false);
            var frm = new InvoiceClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        #endregion

        //protected override void UnboundColumnData(object obj)
        //{
        //    if (obj is not UnboundColumnRowArgs args) return;
        //    if (!args.IsGetData) return; 
        //    var item = (IInvoiceClient)args.Item;
        //    switch (args.FieldName)
        //    {
        //        case "ExtColumnDecimal": 
        //            args.Value = item. item;
        //            break;
        //    }
        //}
    }
}
