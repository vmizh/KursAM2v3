using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.ReportManagers;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Logistiks.Warehouse;
using KursAM2.ViewModel.Logistiks.AktSpisaniya;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Event;
using KursDomain.ICommon;
using KursDomain.IDocuments;
using KursDomain.IReferences;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository.SD24Repository;
using KursDomain.Services;
using KursDomain.Wrapper.Nomenkl.WarehouseOut;
using Newtonsoft.Json;
using Prism.Events;
using StackExchange.Redis;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class OrderOutWindowViewModel2 : RSWindowViewModelBase
    {
        private readonly IDatabase myRedis = RedisStore.RedisCache;
        private readonly ISubscriber mySubscriber;

        #region Constructor

        public OrderOutWindowViewModel2(decimal? docDC, IReferencesCache cache, ALFAMEDIAEntities ctx,
            IEventAggregator eventAggregator, IMessageDialogService messageDialogService)
        {
            if (myRedis != null)
            {
                mySubscriber = myRedis.Multiplexer.GetSubscriber();
                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe(new RedisChannel("WarehouseOut", RedisChannel.PatternMode.Auto),
                        (channel, message) =>
                        {
                            if (KursNotyficationService != null)
                            {
                                Console.WriteLine($"Redis - {message}");
                                Form.Dispatcher.Invoke(() => ShowNotify(message));
                            }
                        });
            }
            myCache = cache;
            myContext = ctx ?? new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString);
            myEeventAggregator = eventAggregator;
            myMessageDialogService = messageDialogService;
            mySD24Repository = new SD24Repository(myContext);
            myNomenklManager = new NomenklManager2(myContext);
            myWrapperEventAggregator = new EventAggregator();
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;

            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);

            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            prn?.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Ордер",
                Command = PrintOrderCommand
            });

            if (docDC != null)
            {
                var model = mySD24Repository.GetDocument(docDC.Value);
                Document = new WarehouseOutWrapper(model, myCache, myContext, myWrapperEventAggregator,
                    myMessageDialogService);
                Document.Initialize();
                UpdateMaxQuantity(Document.DocDate);
                WindowName = Document.ToString();
                Document.State = getState();
                myEeventAggregator.GetEvent<SaveLastDocumentEvent<ILastDocument>>().Publish(
                    new SaveLastDocumentEventArgs<ILastDocument>
                    {
                        info = new LastDocumentInfo
                        {
                            Creator = Document.Creator,
                            Desc = Document.Description,
                            DocDC = Document.DocCode,
                            DocId = Document.Id,
                            DocType = DocumentType.StoreOrderOut,
                            LastChanger = GlobalOptions.UserInfo.NickName
                        }
                    });
            }
            else
            {
                Document = new WarehouseOutWrapper(mySD24Repository.CreateNew(), myCache, myContext,
                    myWrapperEventAggregator, myMessageDialogService);
                Document.State = getState();
                WindowName = "Расходный складской ордер (новый)";
            }
            if (mySubscriber != null && mySubscriber.IsConnected())
            {
                var message = new RedisMessage
                {
                    DocumentType = DocumentType.StoreOrderOut,
                    DocCode = Document.DocCode,
                    DocDate = Document.DocDate,
                    OperationType = RedisMessageDocumentOperationTypeEnum.Open,
                    IsDocument = true,
                    Message = $"Пользователь '{GlobalOptions.UserInfo.Name}' открыл расх. складской ордер {Document.Description}"
                };
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                if (Document.State != RowStatus.NewRow)
                    mySubscriber.Publish(new RedisChannel("WarehouseOut", RedisChannel.PatternMode.Auto), json);
            }

            myWrapperEventAggregator.GetEvent<AfterUpdateBaseWrapperEvent<WarehouseOutWrapper>>()
                .Subscribe(updateDocument);
        }

        #endregion

        #region Methods

        private void ShowNotify(string notify)
        {
            if (string.IsNullOrWhiteSpace(notify)) return;
            var msg = JsonConvert.DeserializeObject<RedisMessage>(notify);
            if (msg == null || msg.UserId == GlobalOptions.UserInfo.KursId) return;
            if (msg.DocCode == Document.DocCode)
            {
                NotifyInfo = msg.Message;
                var notification = KursNotyficationService.CreateCustomNotification(this);
                notification.ShowAsync();
            }
        }

        public string NotifyInfo { get; set; }

        public void UpdateMaxQuantity(DateTime newDate)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var dateOld = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == DocCode)?.DD_DATE ?? Document.DocDate;
                foreach (var r in Document.Rows.Cast<WarehouseOutRowWrapper>())
                {
                    decimal oldQuan = 0;
                    var nq = myNomenklManager.GetNomenklQuantity(Document.WarehouseOut.DocCode, r.Nomenkl.DocCode,
                        newDate, newDate > DateTime.Today ? newDate : DateTime.Today);

                    if (State != RowStatus.NewRow && Document.DocDate >= dateOld)
                    {
                        var d = ctx.TD_24.FirstOrDefault(_ => _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                        oldQuan = d?.DDT_KOL_RASHOD ?? 0;
                    }


                    r.MaxQuantity = nq.Count > 0 ? nq.Min(_ => _.OstatokQuantity) + oldQuan : oldQuan;
                    if (r.QuantityOut > r.MaxQuantity)
                        r.QuantityOut = r.MaxQuantity;
                }
            }
        }

        #endregion

        #region Fields

        private readonly IReferencesCache myCache;
        private ALFAMEDIAEntities myContext;
        private readonly IEventAggregator myEeventAggregator;
        private readonly IEventAggregator myWrapperEventAggregator;
        private readonly IMessageDialogService myMessageDialogService;
        private ISD24Repository mySD24Repository;
        private NomenklManager2 myNomenklManager;
        private readonly WindowManager winManager = new WindowManager();

        private WarehouseOutRowWrapper myCurrentRow;

        private readonly List<WarehouseOutRowWrapper> deletedRows = new List<WarehouseOutRowWrapper>();

        #endregion

        #region Справочники

        public List<Kontragent> Kontragents => GlobalOptions.ReferencesCache.GetKontragentsAll()
            .Cast<Kontragent>().ToList();

        public List<KursDomain.References.Warehouse> StoreDictionary =>
            GlobalOptions.ReferencesCache.GetWarehousesAll().Cast<KursDomain.References.Warehouse>()
                .OrderBy(_ => _.Name).ToList();

        #endregion

        #region Document

        #endregion


        #region Properties

        public override string LayoutName => GetType().Name;

        public WarehouseOutWrapper Document { get; set; }

        public WarehouseOutRowWrapper CurrentRow
        {
            set
            {
                if (Equals(myCurrentRow, value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
                if (Form is OrderOutView2 frm)
                    foreach (var col in frm.gridRows.Columns)
                        if (col.FieldName == nameof(WarehouseOutRowWrapper.QuantityOut))
                            col.ReadOnly = myCurrentRow?.WarehouseOrderIn != null;
            }
            get => myCurrentRow;
        }

        public ObservableCollection<WarehouseOutRowWrapper> SelectedRows { set; get;} =
            new ObservableCollection<WarehouseOutRowWrapper>();

        public List<WarehouseOutRowWrapper> DeletedRows { set; get; } = new List<WarehouseOutRowWrapper>();

        #endregion

        #region Commands

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow && 
                                                 Document.Rows.All( _ => _.WarehouseOrderIn == null);
        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;

        public override bool IsCanSaveData => Document != null && (Document.State != RowStatus.NotEdited
                                                                   || Document.Rows.Any(_ =>
                                                                       _.State != RowStatus.NotEdited)
                                                                   || DeletedRows.Count > 0)
                                                               && Document.State != RowStatus.NotEdited &&
                                                               Document.Rows.All(_ => _.MaxQuantity > 0)
                                                               && Document.WarehouseOut != null &&
                                                               Document.WarehouseIn != null;

        public override bool IsDocNewCopyRequisiteAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsDocNewCopyAllow => false;

        public ICommand PrintOrderCommand
        {
            get { return new Command(PrintOrder, _ => State != RowStatus.NewRow); }
        }

        private void PrintOrder(object obj)
        {
            ReportManager.WarehouseOrderOutReport(Document.DocCode);
        }

        protected override void OnWindowLoaded(object obj)
        {
            //return;
            base.OnWindowLoaded(obj);
            var frm = Form as OrderOutView2;
            if (Document.State != RowStatus.NewRow)
            {
                if (Document.Rows.Any(_ => _.WarehouseOrderIn != null))
                    if (frm != null)
                    {
                        frm.docDateEditor.IsReadOnly = true;
                        frm.docDateEditor.AllowDefaultButton = false;
                        if (frm.comboWarehouseOut != null)
                        {
                            frm.comboWarehouseOut.IsReadOnly = true;
                            frm.comboWarehouseOut.AllowDefaultButton = false;
                        }
                    }

                if (Document.Rows.Count > 0)
                    if (frm != null && frm.comboWarehouseIn != null)
                    {
                        frm.comboWarehouseIn.IsReadOnly = true;
                        frm.comboWarehouseIn.AllowDefaultButton = false;
                    }
            }
            else
            {
                if (frm != null)
                {
                    frm.docDateEditor.IsReadOnly = false;
                    frm.docDateEditor.AllowDefaultButton = true;
                    if (frm.comboWarehouseOut != null)
                    {
                        frm.comboWarehouseOut.IsReadOnly = false;
                        frm.comboWarehouseOut.AllowDefaultButton = true;
                    }

                    if (frm.comboWarehouseIn != null)
                    {
                        frm.comboWarehouseIn.IsReadOnly = false;
                        frm.comboWarehouseIn.AllowDefaultButton = true;
                    }
                }
            }
        }

        #endregion

        #region Commands

        public override void DocNewEmpty(object obj)
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

        public override void DocNewCopyRequisite(object form)
        {
            var ctx = new OrderOutWindowViewModel2(null, GlobalOptions.ReferencesCache,
                null, GlobalOptions.GlobalEventAggregator, new MessageDialogService())
            {
                Document =
                {
                    WarehouseIn = Document.WarehouseIn,
                    WarehouseOut = Document.WarehouseOut,
                    StoreKeeper = Document.StoreKeeper,
                    SenderPersonaName = Document.SenderPersonaName
                }
            };

            ctx.Document.Model.DD_TYPE_DC = Document.Model.DD_TYPE_DC;
            var frm = new OrderOutView2
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void RefreshData(object obj)
        {
            if (IsCanSaveData)
            {
                var service = this.GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                if (service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this) == MessageResult.Yes)
                {
                    SaveData(null);
                    return;
                }
            }


            var frm = Form as OrderOutView2;
            if (frm != null)
                using (var ms = new MemoryStream())
                {
                    frm.gridRows.SaveLayoutToStream(ms);

                    myContext = new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString);
                    mySD24Repository = new SD24Repository(myContext);
                    myNomenklManager = new NomenklManager2(myContext);
                    var model = mySD24Repository.GetDocument(Document.DocCode);
                    Document = new WarehouseOutWrapper(model, myCache, myContext, myWrapperEventAggregator,
                        myMessageDialogService);
                    Document.Initialize();
                    UpdateMaxQuantity(Document.DocDate);
                    WindowName = Document.ToString();
                    deletedRows.Clear();

                    Document.State = getState();
                    RaisePropertyChanged(nameof(Document));

                    ms.Seek(0, SeekOrigin.Begin);
                    frm.gridRows.RestoreLayoutFromStream(ms);
                }
        }

        public ICommand DocumentChangedCommand
        {
            get { return new Command(DocumentChanged, _ => true); }
        }

        private void DocumentChanged(object obj)
        {
            if (obj is CellValueChangedEventArgs args)
                if (args.Column.FieldName == "QuantityOut")
                    if (CurrentRow.QuantityOut > CurrentRow.MaxQuantity)
                    {
                        WindowManager.ShowMessage("Кол-во отгрузки первышает кол-ва на складе",
                            "Ошибка", MessageBoxImage.Stop);
                        CurrentRow.QuantityOut = (decimal)args.OldValue;
                    }
        }

        private RowStatus getState()
        {
            switch (myContext.Entry(Document.Model).State)
            {
                case EntityState.Added:
                    return RowStatus.NewRow;
                case EntityState.Modified:
                    return RowStatus.Edited;
                case EntityState.Unchanged:
                    return deletedRows.Count > 0 ||
                           Document.Rows.Any(r => myContext.Entry(r.Model).State != EntityState.Unchanged)
                        ? RowStatus.Edited
                        : RowStatus.NotEdited;
            }

            return RowStatus.NotDefinition;
        }

        private void updateDocument(AfterUpdateBaseWrapperEventArgs<WarehouseOutWrapper> obj)
        {
            Document.State = getState();
            Document.RaisePropertyChanged("State");
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => Document != null && Document.WarehouseOut != null); }
        }

        private void AddNomenkl(object obj)
        {
            var nomenkls = StandartDialogs.SelectNomenkls(null, true);
            if (nomenkls == null || nomenkls.Count <= 0) return;
            foreach (var n in nomenkls)
                if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode && !n.IsUsluga))
                {
                    var q = myNomenklManager.GetNomenklQuantity(Document.WarehouseOut.DocCode, n.DocCode,
                        Document.DocDate, Document.DocDate);
                    var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                    if (m <= 0)
                    {
                        winManager.ShowWinUIMessageBox($"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                                                       $"{GlobalOptions.ReferencesCache.GetWarehouse(Document.WarehouseOut.DocCode)}" +
                                                       $"кол-во {m}. Операция по номенклатуре не может быть проведена.",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    var code = Document.Rows.Any() ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                    Document.Rows.Add(new WarehouseOutRowWrapper(new TD_24(), myCache, myContext,
                        myWrapperEventAggregator,
                        myMessageDialogService)
                    {
                        DocCode = -1,
                        DocId = Document.Id,
                        Id = Guid.NewGuid(),
                        Code = code,
                        Nomenkl = n,
                        QuantityOut = Math.Min(1, m),
                        Unit = n.Unit as Unit,
                        Currency = n.Currency as Currency,
                        MaxQuantity = m,
                        State = RowStatus.NewRow
                    });
                }

            UpdateMaxQuantity(Document.DocDate);
        }

        public ICommand OrderInOpenCommand
        {
            get { return new Command(OrderInOpen, _ => CurrentRow?.WarehouseOrderIn != null); }
        }

        private void OrderInOpen(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.StoreOrderIn, CurrentRow.WarehouseOrderIn.DocCode);
        }

        public ICommand AddNomenklStoreCommand
        {
            get { return new Command(AddNomenklStore, _ => Document?.WarehouseOut != null); }
        }

        private void AddNomenklStore(object obj)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            var ctx = new DialogSelectExistNomOnSkaldViewModel(Document.WarehouseOut, Document.DocDate,
                Document.Rows.Select(_ => _.Nomenkl.DocCode).ToList());
            var okCommand = new UICommand
            {
                Caption = "Ок",
                IsCancel = false,
                IsDefault = false,
                Command = new DelegateCommand<CancelEventArgs>(
                    x => { },
                    x => !IDataErrorInfoHelper.HasErrors(ctx.CurrentSelectedNomenkl ??
                                                         new NomenklRemainsOnSkladWithPrice()))
            };

            var cancelCommand = new UICommand
            {
                Id = MessageBoxResult.Cancel,
                Caption = "Отмена",
                IsCancel = true,
                IsDefault = false
            };


            var service = this.GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(new List<UICommand> { okCommand, cancelCommand },
                    $"Запрос для склада: {Document.WarehouseOut}", ctx) == cancelCommand) return;
            if (ctx.NomenklSelectedList.Count == 0) return;
            foreach (var n in ctx.NomenklSelectedList)
            {
                if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.Nomenkl.DocCode))
                {
                    var newItem = new WarehouseOutRowWrapper(new TD_24(), myCache, myContext, myWrapperEventAggregator,
                        myMessageDialogService)
                    {
                        DocCode = Document.DocCode,
                        Code = newCode,
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        Nomenkl = n.Nomenkl,
                        Unit = n.Nomenkl.Unit as Unit,
                        Currency = n.Currency,
                        QuantityOut = n.FactOtgruz,
                        MaxQuantity = Math.Min(1, n.Quantity),
                        State = RowStatus.NewRow,
                        Parent = this
                    };
                    myContext.TD_24.Add(newItem.Model);
                    Document.Rows.Add(newItem);
                }

                newCode++;
            }

            UpdateMaxQuantity(Document.DocDate);
        }

        public override void SaveData(object data)
        {
            var isNew = Document.State == RowStatus.NewRow;
            if (isNew)
            {
                var newDC = myContext.SD_24.Any() ? myContext.SD_24.Max(_ => _.DOC_CODE) + 1 : 10240000001;
                var newNum = myContext.SD_24.Any() ? myContext.SD_24.Max(_ => _.DD_IN_NUM) + 1 : 1;
                Document.DocCode = newDC;
                Document.DocNum = newNum;
                foreach (var row in Document.Rows) row.DocCode = Document.DocCode;
            }

            myContext.SaveChanges();
            var dcs = Document.Rows.Select(row => row.DocCode).ToList();
            dcs.AddRange(deletedRows.Select(_ => _.DocCode));
            myNomenklManager.RecalcPrice(dcs);
            deletedRows.Clear();
            Document.State = getState();
            Document.RaisePropertyChanged("State");
            myEeventAggregator.GetEvent<SaveHistoryEvent<IHistory>>()
                .Publish(new SaveHistoryEventArgs<IHistory>
                {
                    history = new HistoryInfo
                    {
                        DocDC = Document.DocCode,
                        DocId = Document.Id,
                        DocType = CustomFormat.GetEnumName(DocumentType.StoreOrderOut),
                        Json = JsonConvert.SerializeObject(Document.ToJson())
                    }
                });
            if (mySubscriber != null && mySubscriber.IsConnected())
            {
                var str = isNew ? "создал" : "сохранил";
                var message = new RedisMessage
                {
                    DocumentType = DocumentType.StoreOrderOut,
                    DocCode = Document.DocCode,
                    DocDate = Document.DocDate,
                    IsDocument = true,
                    OperationType = isNew
                        ? RedisMessageDocumentOperationTypeEnum.Create
                        : RedisMessageDocumentOperationTypeEnum.Update,
                    Message = $"Пользователь '{GlobalOptions.UserInfo.Name}' {str} расх.складской ордер {Document.Description}"
                };
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                mySubscriber.Publish(new RedisChannel("WarehouseOut", RedisChannel.PatternMode.Auto), json);
            }
            if (isNew)
                MainWindowViewModel.EventAggregator.GetEvent<AfterAddNewBaseWrapperEvent<WarehouseOutWrapper>>()
                    .Publish(new AfterAddNewBaseWrapperEventArgs<WarehouseOutWrapper>
                    {
                        DocCode = Document.DocCode,
                        Id = Document.Id
                    });
            else
                MainWindowViewModel.EventAggregator.GetEvent<AFterSaveBaseWrapperEvent<WarehouseOutWrapper>>()
                    .Publish(new AFterSaveBaseWrapperEventArgs<WarehouseOutWrapper>
                    {
                        DocCode = Document.DocCode,
                        Id = Document.Id,
                        wrapper = Document
                    });
        }

        public ICommand DeleteNomenklCommand
        {
            get
            {
                return new Command(DeleteNomenkl, _ => SelectedRows.Count > 0 &&
                    SelectedRows.All(r => r.WarehouseOrderIn == null));
            }
        }

        private void DeleteNomenkl(object obj)
        {
            if (SelectedRows.Count > 0)
            {
                deletedRows.AddRange(SelectedRows.ToList());
                foreach (var row in deletedRows)
                {
                    Document.Rows.Remove(row);
                    myContext.TD_24.Remove(row.Model);
                }
            }
            else
            {
                myContext.TD_24.Remove(CurrentRow.Model);
                deletedRows.Add(CurrentRow);
                Document.Rows.Remove(CurrentRow);
            }
        }

        public override void DocDelete(object form)
        {
            var service = this.GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Уверены, что хотите удалить?";
            if (service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this) == MessageResult.Yes)
            {
                myContext.SD_24.Remove(Document.Model);
                myContext.SaveChanges();
                MainWindowViewModel.EventAggregator.GetEvent<AFterDeleteBaseWrapperEvent<WarehouseOutWrapper>>()
                    .Publish(new AFterDeleteBaseWrapperEventArgs<WarehouseOutWrapper>
                    {
                        DocCode = Document.DocCode,
                        Id = Document.Id
                    });
                if (mySubscriber != null && mySubscriber.IsConnected())
                {
                    var message = new RedisMessage
                    {
                        DocumentType = DocumentType.StoreOrderOut,
                        DocCode = Document.DocCode,
                        DocDate = Document.DocDate,
                        IsDocument = true,
                        OperationType = RedisMessageDocumentOperationTypeEnum.Delete,
                        Message = $"Пользователь '{GlobalOptions.UserInfo.Name}' удалил расх.складской ордер {Document.Description}"
                    };
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                    mySubscriber.Publish(new RedisChannel("WarehouseOut", RedisChannel.PatternMode.Auto), json);
                }
            }

            var dcs = Document.Rows.Select(row => row.DocCode).ToList();
            dcs.AddRange(deletedRows.Select(_ => _.DocCode));
            myNomenklManager.RecalcPrice(dcs);
            if (Form is OrderOutView2 frm) frm.Close();
        }

        #endregion
    }
}
