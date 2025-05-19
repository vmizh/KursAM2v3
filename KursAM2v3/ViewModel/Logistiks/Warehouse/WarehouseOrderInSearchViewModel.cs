using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Base;
using KursAM2.View.Logistiks.Warehouse;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.Repository.DocHistoryRepository;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class WarehouseOrderInSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly ConnectionMultiplexer redis;
        private readonly ISubscriber mySubscriber;

        private readonly WarehouseManager orderManager;
        private WarehouseOrderIn myCurrentDocument;

        public WarehouseOrderInSearchViewModel()
        {
            try
            {
                redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                mySubscriber = redis.GetSubscriber();
                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe(new RedisChannel(RedisMessageChannels.WarehouseOrderIn, RedisChannel.PatternMode.Auto),
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

            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            orderManager =
                new WarehouseManager(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderInSearchViewModel"));
            StartDate = new DateTime(DateTime.Today.AddMonths(-1).Year, DateTime.Today.AddMonths(-1).Month, 1);
            EndDate = DateTime.Today;
        }

        public ObservableCollection<WarehouseOrderIn> Documents { set; get; } =
            new ObservableCollection<WarehouseOrderIn>();

        public WarehouseOrderIn CurrentDocument
        {
            set
            {
                if (Equals(myCurrentDocument?.Id, value?.Id)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
            get => myCurrentDocument;
        }

        public override void AddSearchList(object obj)
        {
            var ctxq = new WarehouseOrderInSearchViewModel();
            var form = new StandartSearchView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctxq
            };
            ctxq.Form = form;
            form.Show();

        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;
        public override bool IsDocNewCopyAllow => false;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument != null;

        public override string WindowName => "Поиск приходных складских ордеров";
        public override string LayoutName => "WarehouseOrderInSearchViewModel";

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

            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());

            using (var ctx = GlobalOptions.GetEntities())
            {
                //var dc = new List<decimal>(new[] { msg.DocCode.Value });
                var d = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == msg.DocCode.Value); /*приходный складской ордер*/
                if (d is null) return;
                var doc = new WarehouseOrderIn(d) { State = RowStatus.NotEdited };
                doc.WarehouseSenderType = doc.KontragentSender != null
                    ? WarehouseSenderType.Kontragent
                    : WarehouseSenderType.Store;

                var last = lastDocumentRopository.GetLastChanges(new[] { msg.DocCode.Value });
                if (last.Count > 0)
                {
                    doc.LastChanger = last.First().Value.Item1;
                    doc.LastChangerDate = last.First().Value.Item2;
                }
                else
                {
                    doc.LastChanger = doc.CREATOR;
                    doc.LastChangerDate = doc.Date;
                }

                var old = Documents.FirstOrDefault(_ => _.DocCode == msg.DocCode);
                if (old != null)
                    switch (msg.OperationType)
                    {
                        case RedisMessageDocumentOperationTypeEnum.Update:
                        {
                            var idx = Documents.IndexOf(old);
                            Documents[idx] = doc;
                            break;
                        }
                        case RedisMessageDocumentOperationTypeEnum.Delete:
                            Documents.Remove(old);
                            break;
                    }
            }
        }

        #region Commands

        public ICommand DoubleClickCommand
        {
            get { return new Command(DoubleClick, _ => true); }
        }

        private void DoubleClick(object obj)
        {
            DocumentOpen(null);
        }

        public override void Search(object obj)
        {
            GetSearchDocument(obj);
        }

        private void Delete(object obj)
        {
        }

        public void GetSearchDocument(object obj)
        {
            try
            {
                Documents.Clear();
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
            var lastDocumentRopository = new DocHistoryRepository(GlobalOptions.GetEntities());
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            try
            {
                var rows = new List<WarehouseOrderIn>();
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var d = ctx.SD_24.Where(_ =>
                            _.DD_DATE >= StartDate && _.DD_DATE <= EndDate &&
                            _.DD_TYPE_DC == 2010000001).OrderByDescending(_ => _.DD_DATE)
                        .ToList(); /*приходный складской ордер*/
                    foreach (var item in d)
                        rows.Add(new WarehouseOrderIn(item) { State = RowStatus.NotEdited });
                    Documents.Clear();
                    if (rows.Count > 0)
                    {
                        var lasts = lastDocumentRopository.GetLastChanges(rows.Select(_ => _.DocCode).Distinct());
                        foreach (var r in rows)
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
                    foreach (var item in rows)
                    {
                        item.WarehouseSenderType = item.KontragentSender != null
                            ? WarehouseSenderType.Kontragent
                            : WarehouseSenderType.Store;
                       
                            
                        
                        Documents.Add(item);
                    }

                    //Documents = new ObservableCollection<WarehouseOrderIn>(rows);
                    RaisePropertyChanged(nameof(Documents));
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }

            RaisePropertyChanged(nameof(Documents));
            SearchText = "";
            //GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
        }

        protected override void DocumentOpen(object form)
        {
            if (CurrentDocument == null) return;

            var ctx = new OrderInWindowViewModel(
                new StandartErrorManager(GlobalOptions.GetEntities(), "WarehouseOrderIn", true)
                , CurrentDocument.DocCode);
            var frm = new OrderInView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new OrderInView { Owner = Application.Current.MainWindow };
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                "WarehouseOrderIn", true))
            {
                Form = frm
            };
            ctx.Document.myState = RowStatus.NewRow;
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderInView { Owner = Application.Current.MainWindow };
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                { Form = frm };
            ctx.Document = orderManager.NewOrderInCopy(CurrentDocument);
            ctx.Document.myState = RowStatus.NewRow;
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderInView { Owner = Application.Current.MainWindow };
            var dbContext = GlobalOptions.GetEntities();
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(dbContext,
                "WarehouseOrderIn", true), 0)
            {
                Form = frm,
                Document = orderManager.NewOrderInRecuisite(CurrentDocument)
            };

            ctx.Document.myState = RowStatus.NewRow;
            ctx.Document.WarehouseSenderType = ctx.Document.KontragentSender != null
                ? WarehouseSenderType.Kontragent
                : WarehouseSenderType.Store;
            frm.DataContext = ctx;
            frm.Show();
        }

        #endregion
    }
}
