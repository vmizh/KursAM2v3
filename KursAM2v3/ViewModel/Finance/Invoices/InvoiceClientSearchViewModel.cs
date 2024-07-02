using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Invoices;
using KursDomain.IDocuments.Finance;
using KursDomain.Menu;
using KursDomain.Repository;
using KursDomain.Repository.DocHistoryRepository;
using Newtonsoft.Json;
using StackExchange.Redis;
using ColumnFilterMode = DevExpress.Xpf.Grid.ColumnFilterMode;

namespace KursAM2.ViewModel.Finance.Invoices
{
    public sealed class InvoiceClientSearchViewModel : RSWindowSearchViewModelBase
    {
        public readonly GenericKursDBRepository<SD_84> GenericProviderRepository;
        private readonly IDatabase myRedis = RedisStore.RedisCache;
        private readonly ISubscriber mySubscriber;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        public IInvoiceClientRepository InvoiceClientRepository;

        private IInvoiceClient myCurrentDocument;


        public InvoiceClientSearchViewModel()
        {
            if (myRedis != null)
            {
                mySubscriber = myRedis.Multiplexer.GetSubscriber();
                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe("ClientInvoice",
                        (_, message) =>
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateList(message));
                        });
            }

            WindowName = "Счета фактуры для клиентов";
            Documents = new ObservableCollection<IInvoiceClient>();
        }

        public InvoiceClientSearchViewModel(Window form) : base(form)
        {
            if (myRedis != null)
            {
                mySubscriber = myRedis.Multiplexer.GetSubscriber();
                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe("ClientInvoice",
                        (_, message) =>
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateList(message));
                        });
            }

            GenericProviderRepository = new GenericKursDBRepository<SD_84>(UnitOfWork);
            InvoiceClientRepository = new InvoiceClientRepository(UnitOfWork);
            WindowName = "Счета фактуры для клиентов";
            Documents = new ObservableCollection<IInvoiceClient>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
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

        //public override void Print(object form)
        //{
        //    var rep = new ExportView();
        //    rep.Show();
        //}

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
            if (mySubscriber != null && mySubscriber.IsConnected())
                mySubscriber.UnsubscribeAll(CommandFlags.FireAndForget);

            base.OnWindowClosing(obj);
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

            var dc = new List<decimal>(new[] { msg.DocCode.Value });
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
            {
                switch (msg.OperationType)
                {
                    case RedisMessageDocumentOperationTypeEnum.Update:
                    {
                        var idx = Documents.IndexOf(old);
                        Documents[idx] = old;
                        break;
                    }
                    case RedisMessageDocumentOperationTypeEnum.Delete:
                        Documents.Remove(old);
                        break;
                }
            }
            Documents.Add(data.First());
        }

        #region Commands

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
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var query = ctx
                        .SD_84
                        .Include(_ => _.SD_43)
                        .Include(_ => _.SD_431)
                        .Include(_ => _.SD_432)
                        .Include(_ => _.SD_301)
                        .Include(_ => _.TD_84)
                        .Include("TD_84.SD_83")
                        .Include("TD_84.TD_24")
                        .Where(_ => _.SF_DATE >= StartDate && _.SF_DATE <= EndDate);
                    foreach (var item in query.ToList())
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
                        Documents.Add(d);
                });
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            });
        }

        public override void DocumentOpen(object form)
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
