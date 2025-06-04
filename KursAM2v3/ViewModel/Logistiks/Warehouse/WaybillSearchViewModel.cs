using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using KursAM2.Managers;
using KursAM2.Repositories;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Base;
using KursAM2.View.Logistiks.Warehouse;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.Repository;
using KursDomain.Repository.DocHistoryRepository;
using Newtonsoft.Json;
using Reports.Base;
using StackExchange.Redis;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class WaybillSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly WarehouseManager DocManager = new WarehouseManager(new StandartErrorManager(
            GlobalOptions.GetEntities(),
            "WaybillViewModel"));

        public readonly GenericKursDBRepository<SD_24> GenericProviderRepository;

        private readonly ConnectionMultiplexer redis;
        private readonly ISubscriber mySubscriber;


        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private WayBillShort myCurrentDocument;

        // ReSharper disable once NotAccessedField.Local
        public ISD_24Repository SD_24Repository;

        public WaybillSearchViewModel()
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);

            try
            {
                redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                mySubscriber = redis.GetSubscriber();
                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe(
                        new RedisChannel(RedisMessageChannels.WayBill, RedisChannel.PatternMode.Auto),
                        (_, message) =>
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateList(message));
                        });
            }
            catch
            {
                Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
            }

            Documents = new ObservableCollection<WayBillShort>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            if (prn != null)
            {
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Товарная накладная",
                    Command = PrintSFCommand
                });
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportSFCommand
                });
            }

            //StartDate = DateTime.Today.AddDays(-30);
            //начальная дата поиска - 1-е число предыдущего месяца или 1-го января
            StartDate = new DateTime(DateTime.Today.Month == 1 ? DateTime.Today.Year - 1 : DateTime.Today.Year,
                DateTime.Today.Month == 1 ? 12 : DateTime.Today.Month - 1, 1);
            EndDate = DateTime.Today;
        }

        public WayBillShort CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument?.DocCode == value?.DocCode) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        //public override bool IsDocNewCopyAllow => CurrentDocument != null;
        public override bool IsDocNewCopyAllow => false;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument != null;
        public override bool IsPrintAllow => CurrentDocument != null;

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<WayBillShort> Documents { set; get; } = new ObservableCollection<WayBillShort>();

        public override string WindowName => "Поиск расходных накладных для клиентов";
        public override string LayoutName => "WaybillSearchViewModel";

        public Command ExportSFCommand
        {
            get { return new Command(ExportSF, _ => IsDocumentOpenAllow); }
        }

        public Command PrintSFCommand
        {
            get { return new Command(PrintSF, _ => IsDocumentOpenAllow); }
        }

        /*
        public WaybillSearchViewModel(Window form) : base(form)
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            if (prn == null) return;
            prn.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Товарная накладная",
                Command = PrintSFCommand
            });
            prn.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Экспорт",
                Command = ExportSFCommand
            });
            StartDate = DateTime.Today.AddDays(-30);
            EndDate = DateTime.Today;
        }
        */

        public override void AddSearchList(object obj)
        {
            var form = new StandartSearchView
            {
                Owner = Application.Current.MainWindow
            };
            var ctxNaklad = new WaybillSearchViewModel
            {
                Form = form
            };
            form.DataContext = ctxNaklad;
            form.Show();
        }

        public override void Print(object form)
        {
            var rep = new ExportView();
            rep.Show();
        }

        private void ExportSF(object obj)
        {
            var ctx = new WaybillWindowViewModel2(CurrentDocument.DocCode);
            ctx.ExportWayBill(null);
        }

        private void PrintSF(object obj)
        {
            var ctx = new WaybillWindowViewModel2(CurrentDocument.DocCode);
            ctx.PrintWaybill(null);
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

            SD_24Repository = new SD_24Repository(UnitOfWork);

            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());

            var list_dc = new List<decimal>(new[] { msg.DocCode.Value });
            var d = SD_24Repository.GetByDocCodes(list_dc);
            var data = d.Select(item => new WayBillShort(item)).ToList();
            var last = lastDocumentRopository.GetLastChanges(list_dc);
            if (data.Count <= 0) return;
            if (last.Count > 0)
            {
                data.First().LastChanger = last.First().Value.Item1;
                data.First().LastChangerDate = last.First().Value.Item2;
            }
            else
            {
                data.First().LastChanger = data.First().CREATOR;
                data.First().LastChangerDate = data.First().Date;
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
        }

        #region Commands

        public override void RefreshData(object data)
        {
            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());
            var frm = Form as StandartSearchView;
            Documents.Clear();
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            Task.Run(() =>
            {
                frm?.Dispatcher.Invoke(() => { frm.loadingIndicator.Visibility = Visibility.Visible; });
                var result = SD_24Repository.GetWayBillAllByDates(StartDate, EndDate)
                    .Select(d => new WayBillShort(d)
                    {
                        DocCode = d.DOC_CODE,
                        InvoiceClient = d.SD_84 != null
                            ? $"С/ф №{d.SD_84.SF_IN_NUM}/{d.SD_84.SF_OUT_NUM} от {d.SD_84.SF_DATE.ToShortDateString()}"
                            : null,
                        State = RowStatus.NotEdited
                    })
                    .ToList();
                if (result.Any())
                {
                    frm?.Dispatcher.Invoke(() =>
                    {
                        if (result.Count <= 0) return;
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
                                    r.LastChangerDate = r.Date;
                                }
                        }

                        foreach (var d in result) Documents.Add(d);
                        frm.loadingIndicator.Visibility = Visibility.Hidden;
                        RaisePropertyChanged(nameof(Documents));
                    });
                }
                else
                {
                    frm?.Dispatcher.Invoke(() => { frm.loadingIndicator.Visibility = Visibility.Hidden; });
                }

                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            });
        }

        protected override void DocumentOpen(object form)
        {
            DocumentsOpenManager.Open(DocumentType.Waybill, CurrentDocument.DocCode);
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new WayBillView2 { Owner = Application.Current.MainWindow };
            var ctx = new WaybillWindowViewModel2(null) { Form = frm };
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new WayBillView2 { Owner = Application.Current.MainWindow };
            var ctx = new WaybillWindowViewModel2(null)
            {
                Form = frm, Document = DocManager.NewWaybillCopy(CurrentDocument.DocCode)
            };

            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new WayBillView2 { Owner = Application.Current.MainWindow };
            var ctx = new WaybillWindowViewModel2(null)
                { Form = frm };
            ctx.Document = DocManager.NewWaybillRecuisite(CurrentDocument.DocCode);
            frm.DataContext = ctx;
            ctx.DocCurrencyVisible = Visibility.Visible;

            //ctx.Document.RaisePropertyChanged("DocCurrency");

            frm.Show();
        }

        #endregion
    }
}
