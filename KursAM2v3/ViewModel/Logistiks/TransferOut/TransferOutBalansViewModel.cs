using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Data;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.View.Helper;
using KursAM2.View.Logistiks.TransferOut;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Event;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository.DocHistoryRepository;
using KursDomain.Repository.NomenklRepository;
using KursDomain.Repository.StorageLocationsRepositury;
using KursDomain.Repository.TransferOut;
using KursDomain.Repository.WarehouseRepository;
using KursDomain.Services;
using KursDomain.ViewModel.Base2;
using KursDomain.ViewModel.Dialog;
using KursDomain.Wrapper;
using KursDomain.Wrapper.TransferOut;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;

namespace KursAM2.ViewModel.Logistiks.TransferOut
{
    public class TransferOutBalansViewModel : FormViewModelBase<TransferOutBalansWrapper, Guid>, IFormMenu
    {

        protected IEventAggregator _EventAggregator;
        protected IMessageDialogService _MessageDialogService;
        private TransferOutBalansWrapper myCurrentDocument; 

        public override async Task InitializeAsync(Guid id, DocumentNewState newState = DocumentNewState.None)
        {
            var storList = await _StorageLocationsRepository.GetAllAsync();
            var disp = (Form ?? Application.Current.MainWindow)?.Dispatcher;
            if (disp == null)
            {
                var wm = new WindowManager();
                wm.ShowKursDialog("Form is null", "Ошибка", Brushes.Red, WindowManager.Confirm);
                return;
            }

            disp.Invoke(() =>
            {
                StorageLocationList.Clear();
                foreach (var stor in storList) StorageLocationList.Add(new StorageLocationsWrapper(stor));
            });

            switch (newState)
            {
                case DocumentNewState.None:
                    var d = await _transferOutBalansRepository.GetByIdAsync(id);
                    disp.Invoke(() =>
                    {
                        Document = new TransferOutBalansWrapper(d, _EventAggregator, _MessageDialogService);
                        Document.StartLoad();
                    });
                    break;
                case DocumentNewState.Empty:
                    IsNewDocument = true;
                    var dnew = _transferOutBalansRepository.New();
                    Document = new TransferOutBalansWrapper(dnew, new EventAggregator(), new MessageDialogService());
                    break;
                case DocumentNewState.Copy:
                    IsNewDocument = true;
                    var dcopy = await _transferOutBalansRepository.NewCopyAsync(id);
                    disp.Invoke(() =>
                    {
                        Document = new TransferOutBalansWrapper(dcopy, new EventAggregator(), new MessageDialogService());
                        Document.StartLoad();
                    });
                    break;
                case DocumentNewState.Requisite:
                    IsNewDocument = true;
                    var dreq = await _transferOutBalansRepository.NewCopyRequisiteAsync(id);
                    disp.Invoke(() => { Document = new TransferOutBalansWrapper(dreq, new EventAggregator(), new MessageDialogService()); });
                    break;
            }

            using (var ctx = GlobalOptions.GetEntities())
            {
                _WarehouseRepository = new WarehouseRepository(ctx);
                if (Document.Warehouse != null)
                {
                    var data = await _WarehouseRepository.GetNomenklsOnWarehouseAsync(Document.DocDate.AddDays(-1),
                        Document.Warehouse.DocCode);
                    foreach (var r in Document.Rows)
                    {
                        var n = data.FirstOrDefault(_ => _.NomenklDC == r.Nomenkl.DocCode);
                        if (n != null)
                        {
                            r.MaxCount = n.Remain;
                            r.CostPrice = n.Price;
                        }
                        else
                        {
                            r.MaxCount = r.Quatntity;
                            r.CostPrice = r.Price;
                        }
                    }
                }
            }

            foreach (var r in Document.Rows) r.PropertyChanged += Document_PropertyChanged;

            Document.StorageLocation =
                StorageLocationList.FirstOrDefault(_ => _.Id == Document.Model.StorageLocationId);
            Document.State = _transferOutBalansRepository.GetRowStatus(Document.Model);
            Document.PropertyChanged += Document_PropertyChanged;
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
            CanCurrencyChanged = Document.Rows.Count == 0;
            CanWarehouseChanged = Document.Rows.Count == 0;
        }

        public override void Initialize(Guid id, DocumentNewState newState = DocumentNewState.None)
        {
            var storList = _StorageLocationsRepository.GetAll();
            StorageLocationList.Clear();
            foreach (var stor in storList) StorageLocationList.Add(new StorageLocationsWrapper(stor));

            switch (newState)
            {
                case DocumentNewState.None:
                    var d = _transferOutBalansRepository.GetById(id);
                    Document = new TransferOutBalansWrapper(d,_EventAggregator,_MessageDialogService);
                    Document.StartLoad();
                    break;
            }

            using (var ctx = GlobalOptions.GetEntities())
            {
                _WarehouseRepository = new WarehouseRepository(ctx);
                if (Document.Warehouse != null)
                {
                    var data = _WarehouseRepository.GetNomenklsOnWarehouse(Document.DocDate.AddDays(-1),
                        Document.Warehouse.DocCode);
                    foreach (var r in Document.Rows)
                    {
                        var n = data.FirstOrDefault(_ => _.NomenklDC == r.Nomenkl.DocCode);
                        if (n != null)
                        {
                            r.MaxCount = n.Remain;
                            r.CostPrice = n.Price;
                        }
                        else
                        {
                            r.MaxCount = r.Quatntity;
                            r.CostPrice = r.Price;
                        }
                    }
                }
            }

            foreach (var r in Document.Rows) r.PropertyChanged += Document_PropertyChanged;

            Document.StorageLocation =
                StorageLocationList.FirstOrDefault(_ => _.Id == Document.Model.StorageLocationId);
            Document.State = _transferOutBalansRepository.GetRowStatus(Document.Model);
            Document.PropertyChanged += Document_PropertyChanged;
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
            CanCurrencyChanged = Document.Rows.Count == 0;
            CanWarehouseChanged = Document.Rows.Count == 0;

        }

        private void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            HasChanges = true;
            Document.UpdateSummaries();
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)AddNomenklCommand).RaiseCanExecuteChanged();
        }

        #region Constructors

        public TransferOutBalansViewModel(ITransferOutBalansRepository transferOutBalansRepository,
            IStorageLocationsRepositiry storageLocationsRepository, INomenklRepository nomenklRepository,
            IDocHistoryRepository documentHistoryRepository)
        {
            _transferOutBalansRepository = transferOutBalansRepository;
            _StorageLocationsRepository = storageLocationsRepository;
            _NomenklRepository = nomenklRepository;
            _documentHistoryRepository = documentHistoryRepository;
            _EventAggregator = new EventAggregator();
            _MessageDialogService = new MessageDialogService();

            LayoutName = "TransferOutBalans";
            Title = "Списание товаров на забаланс";

            FormControl = new TransferOutBalansView();

            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);

            AddNomenklCommand = new DelegateCommand(OnAddNomenklExecute, CanAddNomenkl);
            DeleteRowCommand = new DelegateCommand(OnDeleteRowExecute, CanDeleteRow);

            ClickCommand = new DelegateCommand(OnClickExecute);
        }

        private void OnClickExecute()
        {
            if (Document.Rows.Count > 0)
                CurrentRow = Document.Rows.Last();
        }

        public ICommand ClickCommand { get; }

        #endregion

        #region Fields

        private readonly ITransferOutBalansRepository _transferOutBalansRepository;
        private readonly IStorageLocationsRepositiry _StorageLocationsRepository;
        private readonly IDocHistoryRepository _documentHistoryRepository;
        private readonly INomenklRepository _NomenklRepository;
        private TransferOutBalansRowsWrapper _CurrentRow;
        private IWarehouseRepository _WarehouseRepository;
        private bool myCanRefreshData = true;
        private readonly List<Nomenkl> deleteNomenkls = new List<Nomenkl>();
        private bool myCanWarehouseChanged;
        private bool myCanCurrencyChanged;

        #endregion

        #region Properties

        public List<KursDomain.References.Warehouse> WarehouseList { get; set; } = GlobalOptions.ReferencesCache
            .GetWarehousesAll().Cast<KursDomain.References.Warehouse>().ToList();

        public List<Currency> CurrencyList { get; set; } = GlobalOptions.ReferencesCache
            .GetCurrenciesAll().Cast<Currency>().ToList();

        public List<StorageLocationsWrapper> StorageLocationList { get; set; } = new List<StorageLocationsWrapper>();

        public ObservableCollection<MenuButtonInfo> RightMenuBar { get; set; }
        public ObservableCollection<MenuButtonInfo> LeftMenuBar { get; set; }

        public TransferOutBalansRowsWrapper CurrentRow
        {
            get => _CurrentRow;
            set
            {
                if (Equals(value, _CurrentRow)) return;
                _CurrentRow = value;
                OnPropertyChanged();
                ((DelegateCommand)DeleteRowCommand).RaiseCanExecuteChanged();
            }
        }

        public bool CanWarehouseChanged
        {
            set
            {
                if (value == myCanWarehouseChanged) return;
                myCanWarehouseChanged = value;
                OnPropertyChanged();
            }
            get => myCanWarehouseChanged;
        }


        public bool CanCurrencyChanged
        {
            set
            {
                if (value == myCanCurrencyChanged) return;
                myCanCurrencyChanged = value;
                OnPropertyChanged();
            }
            get => myCanCurrencyChanged;
        }

        #endregion

        #region Methods

        #endregion

        #region Commands

        protected override async Task OnDoсDeleteExecuteAsync()
        {
            var wm = new WindowManager();
            var dlgRslt = wm.ShowKursDialog("Вы действительно хотите удалить документ?", "Запрос",
                Brushes.Blue, WindowManager.YesNo);
            if (dlgRslt == WindowManager.KursDialogResult.No) return;
            try
            {
                foreach (var row in Document.Rows) _transferOutBalansRepository.RemoveRow(row.Model);

                _transferOutBalansRepository.Remove(Document.Model);
                await _transferOutBalansRepository.SaveAsync();
                Close();
                MainWindowViewModel.EventAggregator.GetEvent<AfterTransferOutBalansWrapperEvent>()
                    .Publish(new AfterTransferOutBalansWrapperEventArgs
                    {
                        Id = Document.Id,
                        DocCode = Document.DocCode,
                        Document = Document,
                        Operation = EnumAfterSaveOperation.Delete
                    });
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public ICommand DeleteRowCommand { get; }

        private bool CanDeleteRow()
        {
            return CurrentRow != null;
        }


        private void OnDeleteRowExecute()
        {
            _transferOutBalansRepository.RemoveRow(CurrentRow.Model);
            deleteNomenkls.Add(CurrentRow.Nomenkl);
            Document.Rows.Remove(CurrentRow);
            Document.UpdateSummaries();
            HasChanges = true;
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
            CanCurrencyChanged = Document.Rows.Count == 0;
            CanWarehouseChanged = Document.Rows.Count == 0;
        }

        public ICommand AddNomenklCommand { get; }

        private bool CanAddNomenkl()
        {
            return Document.Warehouse != null && Document.Currency != null;
        }

        private async void OnAddNomenklExecute()
        {
            var vm = new WarehouseRemainsViewModel(Document.DocDate, Document.Warehouse,
                new WarehouseRepository(GlobalOptions.GetEntities()), Document.Currency?.DocCode);
            await vm.ShowAsync();
            switch (vm.Result)
            {
                case KursDialogResult.Ok:
                    foreach (var item in vm.SelectDocumentItems)
                    {
                        var newItem = new TransferOutBalansRows
                        {
                            DocId = Document.Id,
                            Id = Guid.NewGuid(),
                            NomenklDC = item.Nomenkl.DocCode,
                            Price = item.Price,
                            Quatntity = item.Remain
                        };
                        _transferOutBalansRepository.AddRow(newItem);
                        var newRow = new TransferOutBalansRowsWrapper(newItem, _EventAggregator, _MessageDialogService)
                        {
                            Nomenkl = item.Nomenkl,
                            MaxCount = item.Remain,
                            CostPrice = item.Price
                        };
                        newRow.PropertyChanged += Document_PropertyChanged;

                        Document.Rows.Add(newRow);
                    }


                    Document.State = _transferOutBalansRepository.GetRowStatus(Document.Model);
                    Document.UpdateSummaries();
                    HasChanges = true;
                    ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
                    break;
                case KursDialogResult.Cancel:
                    break;
            }
            CanCurrencyChanged = Document.Rows.Count == 0;
            CanWarehouseChanged = Document.Rows.Count == 0;
        }

        protected override bool CanSaveData()
        {
            if (IsBusy) return false;
            return Document is { Warehouse: not null, StorageLocation: not null, Currency: not null } &&
                   (HasChanges || IsNewDocument);
        }

        protected override async Task OnSaveDataAsync()
        {
            IsBusy = true;
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)RefreshDataCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)CloseWindowCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)DoсDeleteCommand).RaiseCanExecuteChanged();
            try
            {
                _transferOutBalansRepository.BeginTransaction();
                if (Document.DocNum < 0) Document.DocNum = _transferOutBalansRepository.GetNewDocumentNumber();
                _documentHistoryRepository.Add(new DocHistory
                {
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Code = null,
                    Date = DateTime.Now,
                    DocDC = null,
                    DocType = CustomFormat.GetEnumName(DocumentType.TransferOutBalans),
                    DocData = JsonConvert.SerializeObject(Document.ToJson()),
                    UserName = GlobalOptions.UserInfo.NickName
                });
                await _transferOutBalansRepository.SaveAsync();
                await _NomenklRepository.RecalcPriceAsync(Document.Rows.Select(_ => _.Nomenkl.DocCode).Distinct()
                    .Union(deleteNomenkls.Select(_ => _.DocCode))
                    .ToList());
                foreach (var n in Document.Rows.Select(_ => _.Nomenkl.DocCode))
                {
                    var q = await _NomenklRepository.GetNomenklQuantityAsync(Document.Warehouse.DocCode, n,
                        Document.DocDate, Document.DocDate);
                    var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                    if (m >= 0) continue;
                    var nom = GlobalOptions.ReferencesCache.GetNomenkl(n) as Nomenkl;
                    _transferOutBalansRepository.Rollback();
                    // ReSharper disable once PossibleNullReferenceException
                    WindowManager.ShowMessage($"По товару {nom.NomenklNumber} {nom.Name} " +
                                              // ReSharper disable once PossibleInvalidOperationException
                                              $"склад {Document.Warehouse} в кол-ве {q.First().OstatokQuantity} ",
                        "Отрицательные остатки", MessageBoxImage.Error);
                    IsBusy = false;
                    ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
                    ((DelegateCommand)RefreshDataCommand).RaiseCanExecuteChanged();
                    ((DelegateCommand)CloseWindowCommand).RaiseCanExecuteChanged();
                    ((DelegateCommand)DoсDeleteCommand).RaiseCanExecuteChanged();
                    return;
                }

                Document.State = _transferOutBalansRepository.GetRowStatus(Document.Model);
                MainWindowViewModel.EventAggregator.GetEvent<AfterTransferOutBalansWrapperEvent>()
                    .Publish(new AfterTransferOutBalansWrapperEventArgs
                    {
                        Id = Document.Id,
                        DocCode = Document.DocCode,
                        Document = Document,
                        Operation = IsNewDocument ? EnumAfterSaveOperation.Add : EnumAfterSaveOperation.Update
                    });
                _transferOutBalansRepository.CommitTransaction();
                IsNewDocument = false;
                Document.myState = _transferOutBalansRepository.GetRowStatus(Document.Model);
                Document.RaisePropertyChanged("State");
                HasChanges = false;
            }
            catch (Exception ex)
            {
                _transferOutBalansRepository.Rollback();
                WindowManager.ShowError(ex);
            }

            IsBusy = false;
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)RefreshDataCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)CloseWindowCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)DoсDeleteCommand).RaiseCanExecuteChanged();
        }

        protected override bool CanRefreshData()
        {
            return myCanRefreshData && !IsBusy;
        }

        protected override bool CanDocNewEmpty()
        {
            return true;
        }

        protected override bool CanDocNewCopyRequisite()
        {
            return true;
        }

        protected override bool CanNewCopyEmpty()
        {
            return false;
        }

        protected override async Task OnDocNewEmptyExecueAsync()
        {
            var ctx = GlobalOptions.GetEntities();
            var doc = new TransferOutBalansViewModel(new TransferOutBalansRepository(ctx),
                new StorageLocationsRepository(ctx), new NomenklRepository(ctx), new DocHistoryRepository(ctx));
            await doc.InitializeAsync(Guid.Empty, DocumentNewState.Empty);
            doc.Show();
        }

        protected override async Task OnDocNewCopyRequisiteExecueAsync()
        {
            var ctx = GlobalOptions.GetEntities();
            var doc = new TransferOutBalansViewModel(new TransferOutBalansRepository(ctx),
                new StorageLocationsRepository(ctx), new NomenklRepository(ctx), new DocHistoryRepository(ctx));
            await doc.InitializeAsync(Document.Id, DocumentNewState.Requisite);
            doc.Show();
        }

        
        public override async Task OnRefreshDataAsync()
        {
            myCanRefreshData = false;
            ((DelegateCommand)RefreshDataCommand).RaiseCanExecuteChanged();
            if (HasChanges)
            {
                var wm = new WindowManager();
                var dlgRslt = wm.ShowKursDialog("В справочник внесены изменения, сохранить?", "Запрос",
                    Brushes.Blue, WindowManager.YesNoCancel);
                switch (dlgRslt)
                {
                    case WindowManager.KursDialogResult.Cancel:
                        return;
                    case WindowManager.KursDialogResult.No:
                        break;
                    case WindowManager.KursDialogResult.Yes:
                        await OnSaveDataAsync();
                        break;
                }
            }

            _transferOutBalansRepository.ContextRollback();
            Document.RaisePropertiesChanged();

            var entities = _transferOutBalansRepository.GetEntites();
            var entrIds = new List<Guid>();
            foreach (var ent in entities.Select(_ => _.Entity))
            {
                if (ent is not TransferOutBalansRows m) continue;
                entrIds.Add(m.Id);
            }

            var delIds = Document.Rows.Select(_ => _.Id).Except(entrIds).ToList();
            foreach (var r in delIds.Select(id => Document.Rows.FirstOrDefault(_ => _.Id == id))
                         .Where(r => r != null))
                Document.Rows.Remove(r);

            foreach (var entity in entities.Where(_ => _.State == EntityState.Unchanged))
            {
                if (entity.Entity is not TransferOutBalansRows m) continue;
                var row = Document.Rows.FirstOrDefault(_ => _.Id == m.Id);
                if (row == null) Document.Rows.Add(new TransferOutBalansRowsWrapper(m, _EventAggregator, _MessageDialogService));
            }

            foreach (var row in Document.Rows) row.RaisePropertiesChanged();
            myCanRefreshData = true;
            Document.myState = _transferOutBalansRepository.GetRowStatus(Document.Model);
            Document.RaisePropertyChanged("State");
            HasChanges = false;
            ((DelegateCommand)RefreshDataCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }

        protected override void OnShowHistoryExecute()
        {
            DocumentHistoryManager.LoadHistory(DocumentType.TransferOutBalans, Document.Id, 0);
        }


        protected override void UpdateVisualObject()
        {
            if (FormControl is TransferOutBalansView form)
            {
                form.gridRows.TotalSummary.Clear();
                foreach (var col in form.gridRows.Columns)
                {
                    switch (col.FieldName)
                    {
                        case nameof(TransferOutBalansRowsWrapper.Summa):
                        case nameof(TransferOutBalansRowsWrapper.CostSumma):
                            var summ = new GridSummaryItem
                            {
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Sum,
                                DisplayFormat = "n2",
                                 
                            };
                            form.gridRows.TotalSummary.Add(summ);
                            break;
                        case nameof(TransferOutBalansRowsWrapper.State):
                            col.Visible = false;
                            break;
                    }
                }
            }

        }

        #endregion
    }
}
