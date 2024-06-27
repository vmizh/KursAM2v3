using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers;
using KursAM2.ReportManagers;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Base;
using KursAM2.View.Logistiks.Warehouse;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.Event;
using KursDomain.ICommon;
using KursDomain.IDocuments;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.Repository;
using KursDomain.Repository.SD24Repository;
using KursDomain.Services;
using KursDomain.Wrapper.Nomenkl.WarehouseOut;
using Newtonsoft.Json;
using Prism.Events;
using StackExchange.Redis;
using Application = System.Windows.Application;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class WarehouseOrderOutSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());
        private readonly WarehouseManager orderManager;
        private WarehouseOrderOut myCurrentDocument;

        private readonly IDatabase myRedis = RedisStore.RedisCache;
        private readonly ISubscriber mySubscriber;

        public WarehouseOrderOutSearchViewModel()
        {
            if (myRedis != null)
            {
                mySubscriber = myRedis.Multiplexer.GetSubscriber();
                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe("ClientInvoice",
                        (channel, message) =>
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateList(message));
                        });
            }
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            orderManager =
                new WarehouseManager(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderSearchViewModel"));
            StartDate = new DateTime(DateTime.Today.Year, 1, 1);
            EndDate = DateTime.Today;
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            prn?.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Ордер",
                Command = PrintOrderCommand
            });
            MainWindowViewModel.EventAggregator.GetEvent<AFterDeleteBaseWrapperEvent<WarehouseOutWrapper>>()
                .Subscribe(deleteDocument);
            MainWindowViewModel.EventAggregator.GetEvent<AfterAddNewBaseWrapperEvent<WarehouseOutWrapper>>()
                .Subscribe(onAddNewDocument);
            MainWindowViewModel.EventAggregator.GetEvent<AFterSaveBaseWrapperEvent<WarehouseOutWrapper>>()
                .Subscribe(onUpdateDocument);
        }

        public ICommand PrintOrderCommand
        {
            get { return new Command(PrintOrder, _ => CurrentDocument != null); }
        }

        public ObservableCollection<WarehouseOrderOut> Documents { set; get; } =
            new ObservableCollection<WarehouseOrderOut>();

        public override string WindowName => "Поиск расходных складских ордеров";
        public override string LayoutName => "WarehouseOrderOutSearchViewModel";

        public WarehouseOrderOut CurrentDocument
        {
            set
            {
                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
            get => myCurrentDocument;
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument != null;
        public override bool IsDocNewCopyAllow => false;

        private void onUpdateDocument(AFterSaveBaseWrapperEventArgs<WarehouseOutWrapper> obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var d = ctx.SD_24.FirstOrDefault(_ =>
                    _.DOC_CODE == obj.DocCode);
                if (d != null && d.DD_DATE >= StartDate && d.DD_DATE <= EndDate)
                {
                    var old = Documents.FirstOrDefault(_ => _.DocCode == obj.DocCode);
                    if (old != null)
                        Documents.Remove(old);
                    Documents.Add(new WarehouseOrderOut(d) { State = RowStatus.NotEdited });
                }
            }
        }

        private void onAddNewDocument(AfterAddNewBaseWrapperEventArgs<WarehouseOutWrapper> obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var d = ctx.SD_24.FirstOrDefault(_ =>
                    _.DOC_CODE == obj.DocCode);
                if (d != null && d.DD_DATE >= StartDate && d.DD_DATE <= EndDate)
                    Documents.Add(new WarehouseOrderOut(d) { State = RowStatus.NotEdited });
            }
        }

        private void deleteDocument(AFterDeleteBaseWrapperEventArgs<WarehouseOutWrapper> obj)
        {
            var delRow = Documents.FirstOrDefault(_ => _.DocCode == obj.DocCode);
            if (delRow != null) Documents.Remove(delRow);
        }

        private void PrintOrder(object obj)
        {
            ReportManager.WarehouseOrderOutReport(CurrentDocument.DocCode);
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

        private void UpdateList(RedisValue message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var msg = JsonConvert.DeserializeObject<RedisMessage>(message);
            if (msg == null || msg.DocCode == null) return;
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

            //InvoiceClientRepository =
            //    new InvoiceClientRepository(
            //        new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString)));

            //var dc = new List<decimal>(new[] { msg.DocCode.Value });
            //var data = InvoiceClientRepository.GetByDocCodes(dc);
            //var last = InvoiceClientRepository.GetLastChanges(dc);
            //if (data.Count <= 0) return;
            //if (last.Count > 0)
            //{
            //    data.First().LastChanger = last.First().Value.Item1;
            //    data.First().LastChangerDate = last.First().Value.Item2;
            //}
            //else
            //{
            //    data.First().LastChanger = data.First().CREATOR;
            //    data.First().LastChangerDate = data.First().DocDate;
            //}

            //var old = Documents.FirstOrDefault(_ => _.DocCode == msg.DocCode);
            //if (old != null) Documents.Remove(old);
            //Documents.Add(data.First());
        }

        public override void RefreshData(object data)
        {
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            Documents.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var d = ctx.SD_24.Where(_ =>
                        _.DD_DATE >= StartDate && _.DD_DATE <= EndDate &&
                        _.DD_TYPE_DC == 2010000003).ToList();
                    foreach (var item in d)
                        Documents.Add(new WarehouseOrderOut(item) { State = RowStatus.NotEdited });


                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }

            RaisePropertyChanged(nameof(Documents));
            SearchText = "";
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
        }

        public override void DocumentOpen(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new OrderOutWindowViewModel2(CurrentDocument.DocCode, GlobalOptions.ReferencesCache,
                null, GlobalOptions.GlobalEventAggregator, new MessageDialogService());
            var frm = new OrderOutView2
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var ctx = new OrderOutWindowViewModel2(null, GlobalOptions.ReferencesCache,
                null, GlobalOptions.GlobalEventAggregator, new MessageDialogService());
            ctx.Document.Model.DD_TYPE_DC = (decimal)MaterialDocumentTypeEnum.WarehouseOut;
            var frm = new OrderOutView2
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object obj)
        {
            var context = GlobalOptions.GetEntities();
            var repo = new SD24Repository(context);
            var d = repo.GetDocument(CurrentDocument.DocCode);
            var doc = new WarehouseOutWrapper(d, GlobalOptions.ReferencesCache, context, new EventAggregator(),
                new MessageDialogService());
            var ctx = new OrderOutWindowViewModel2(null, GlobalOptions.ReferencesCache,
                null, GlobalOptions.GlobalEventAggregator, new MessageDialogService())
            {
                Document =
                {
                    WarehouseIn = doc.WarehouseIn,
                    WarehouseOut = doc.WarehouseOut,
                    StoreKeeper = doc.StoreKeeper,
                    SenderPersonaName = doc.SenderPersonaName
                }
            };

            ctx.Document.Model.DD_TYPE_DC = doc.Model.DD_TYPE_DC;
            var frm = new OrderOutView2
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
                foreach (var col in frm.gridDocuments.Columns)
                    switch (col.FieldName)
                    {
                        case "State":
                            col.Visible = false;
                            break;
                    }
        }

        public override void OnWindowClosing(object obj)
        {
            MainWindowViewModel.EventAggregator.GetEvent<AFterDeleteBaseWrapperEvent<WarehouseOutWrapper>>()
                .Unsubscribe(unsubScribeDelete);
            base.OnWindowClosing(obj);
            MainWindowViewModel.EventAggregator.GetEvent<AFterSaveBaseWrapperEvent<WarehouseOutWrapper>>()
                .Unsubscribe(unsubScribeUpdate);
            base.OnWindowClosing(obj);
            MainWindowViewModel.EventAggregator.GetEvent<AfterAddNewBaseWrapperEvent<WarehouseOutWrapper>>()
                .Unsubscribe(unsubScribeAdd);
            base.OnWindowClosing(obj);
        }

        private void unsubScribeDelete(AFterDeleteBaseWrapperEventArgs<WarehouseOutWrapper> obj)
        {
        }

        private void unsubScribeUpdate(AFterSaveBaseWrapperEventArgs<WarehouseOutWrapper> obj)
        {
        }

        private void unsubScribeAdd(AfterAddNewBaseWrapperEventArgs<WarehouseOutWrapper> obj)
        {
        }

        #endregion
    }
}
