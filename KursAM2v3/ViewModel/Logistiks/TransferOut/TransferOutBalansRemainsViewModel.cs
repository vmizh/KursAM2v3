using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Data;
using KursAM2.View.Logistiks.TransferOut;
using KursDomain;
using KursDomain.IDocuments.TransferOut;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository.DocHistoryRepository;
using KursDomain.Repository.NomenklRepository;
using KursDomain.Repository.StorageLocationsRepositury;
using KursDomain.Repository.TransferOut;
using KursDomain.RepositoryHelper;
using KursDomain.ViewModel.Base2;
using KursDomain.Wrapper;
using KursDomain.Wrapper.TransferOut;
using Prism.Commands;

namespace KursAM2.ViewModel.Logistiks.TransferOut
{
    public class TransferOutBalansRemainsDocument : ITransferOutBalansRows
    {
        [Display(AutoGenerateField = true, Name = "№")]
        public int DocNum => TransferOutBalans.DocNum;

        [Display(AutoGenerateField = true, Name = "Дата")]
        public DateTime DocDate => TransferOutBalans.DocDate;

        [Display(AutoGenerateField = true, Name = "Место хранения")]
        public StorageLocationsWrapper StorageLocations { get; set; }

        [Display(AutoGenerateField = true, Name = "Ном.№")]
        public string NomenklNumber => Nomenkl?.NomenklNumber;

        [Display(AutoGenerateField = false, Name = "Id")]
        public Guid Id { get; set; }

        [Display(AutoGenerateField = false, Name = "Id")]
        public Guid DocId { get; set; }

        [Display(AutoGenerateField = true, Name = "Номенклатура")]
        [ReadOnly(true)]
        public Nomenkl Nomenkl { get; set; }

        [Display(AutoGenerateField = true, Name = "Кол-во")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Quatntity { get; set; }

        [Display(AutoGenerateField = true, Name = "Цена")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Price { get; set; }

        [Display(AutoGenerateField = true, Name = "Сумма")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Summa => Price * Quatntity;

        [Display(AutoGenerateField = true, Name = "Примечание")]
        [ReadOnly(true)]
        public string Note { get; set; }

        [Display(AutoGenerateField = false, Name = "Id")]
        public ITransferOutBalans TransferOutBalans { get; set; }
    }

    public sealed class TransferOutBalansRemainsViewModel : FormViewModelBase<NomenklStoreLocationItem, decimal>,
        IFormMenu
    {
        #region Constructors

        public TransferOutBalansRemainsViewModel()
        {
            var context = GlobalOptions.GetEntities();
            _StorageLocationsRepositiry = new StorageLocationsRepository(context);
            _TransferOutBalansRepository = new TransferOutBalansRepository(context);

            LayoutName = "TransferOutBalansRemainsViewModel";
            Title = "Товары за балансом";

            FormControl = new TransferOutBalansRemainsView();

            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            RemainDate = DateTime.Today;
            Remains = new ObservableCollection<NomenklStoreLocationItem>();
            DocumentRows = new ObservableCollection<TransferOutBalansRemainsDocument>();
            StorageLocationList = new ObservableCollection<StorageLocationsWrapper>();
            OpenDocumentCommand = new DelegateCommand(async () => await OnOpenDocumentExecute(), CanOpenDocument);
            RemainDateChangedCommand =  new DelegateCommand(async () => await OnRemainDateChangedAsync());
        }

        #endregion

        #region Methods

        public override async Task InitializeAsync(decimal id, DocumentNewState newState = DocumentNewState.None)
        {
            var storageLoc = await _StorageLocationsRepositiry.GetAllAsync();
            Form.Dispatcher.Invoke(() =>
            {
                StorageLocationList.Add(new StorageLocationsWrapper(new StorageLocations
                {
                    Id = Guid.Empty,
                    Name = "Все места"
                }));
                foreach (var sl in storageLoc.ToList().OrderBy(_ => _.Name))
                    StorageLocationList.Add(new StorageLocationsWrapper(sl));
                StorageLocation = StorageLocationList.First();
            });
        }

        public override void Initialize(decimal id, DocumentNewState newState = DocumentNewState.None)
        {
            throw new NotImplementedException();
        }

        private async Task LoadDocumentsAsync(NomenklStoreLocationItem item)
        {
            if (item?.NomenklDC == null) return;
            var data = await _TransferOutBalansRepository.GetLocationStorageRemainDocuments(item.NomenklDC,
                StorageLocation.Id == Guid.Empty ? null : StorageLocation.Id, RemainDate);
            Form.Dispatcher.Invoke(() =>
            {
                DocumentRows.Clear();
                foreach (var r in data)
                {
                    var doc = new TransferOutBalansWrapper(r.TransferOutBalans);
                    doc.StartLoad(false);
                    var note = string.IsNullOrWhiteSpace(r.Note)
                        ? $"{r.TransferOutBalans.Note}"
                        : $"{r.TransferOutBalans.Note} / {r.Note}";
                    DocumentRows.Add(new TransferOutBalansRemainsDocument
                    {
                        TransferOutBalans = doc,
                        Id = r.Id,
                        DocId = r.DocId,
                        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(r.NomenklDC) as Nomenkl,
                        Note = note,
                        Price = r.Price,
                        Quatntity = r.Quatntity,
                        StorageLocations = new StorageLocationsWrapper(r.TransferOutBalans.StorageLocations)
                    });
                }
            });
        }

        #endregion

        #region Commands

        public ICommand OpenDocumentCommand { get; }

        private async Task OnOpenDocumentExecute()
        {
            Form.loadingIndicator.Visibility = Visibility.Visible;
            var ctx = GlobalOptions.GetEntities();
            var doc = new TransferOutBalansViewModel(new TransferOutBalansRepository(ctx),
                new StorageLocationsRepository(ctx), new NomenklRepository(ctx), new DocHistoryRepository(ctx));
            await doc.InitializeAsync(CurrentDocumentRow.DocId);
            Form.loadingIndicator.Visibility = Visibility.Hidden;
            doc.Show();
        }

        private bool CanOpenDocument()
        {
            return CurrentDocumentRow != null;
        }

        protected override bool CanRefreshData()
        {
            return !IsBusy;
        }

        public override async Task OnRefreshDataAsync()
        {
            IsBusy = true;
            Form.Dispatcher.Invoke(() => { Form.loadingIndicator.Visibility = Visibility.Visible; });
            ((DelegateCommand)RefreshDataCommand).RaiseCanExecuteChanged();
            Guid? slId = null;
            Form.Dispatcher.Invoke(() =>
            {
                slId = (StorageLocation?.Id ?? Guid.Empty) != Guid.Empty ? StorageLocation.Id : null;
            });
            var data = await _TransferOutBalansRepository.GetLocationStoreRemainAsync(slId, RemainDate);
            Form.Dispatcher.Invoke(() =>
            {
                DocumentRows.Clear();
                Remains.Clear();
                foreach (var d in data.ToList())
                {
                    d.Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as Nomenkl;
                    d.StorageLocations = StorageLocationList.FirstOrDefault(_m => _m.Id == d.StorageLocationId);
                    Remains.Add(d);
                }

                Form.loadingIndicator.Visibility = Visibility.Hidden;
            });

            IsBusy = false;
            ((DelegateCommand)RefreshDataCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)OpenDocumentCommand).RaiseCanExecuteChanged();
            isStart = false;
        }

        public ICommand RemainDateChangedCommand { get; set; }

        private async Task OnRemainDateChangedAsync()
        {
            if (isStart) return;
            await OnRefreshDataAsync();
        }

        #endregion

        #region Fields

        private readonly IStorageLocationsRepositiry _StorageLocationsRepositiry;
        private readonly ITransferOutBalansRepository _TransferOutBalansRepository;
        private DateTime myRemainDate;
        private StorageLocationsWrapper myStorageLocation;
        private NomenklStoreLocationItem myCurrentRemain;
        private TransferOutBalansRemainsDocument myCurrentDocumentRow;
        private bool isStart = true;

        #endregion

        #region Properties

        public ObservableCollection<MenuButtonInfo> RightMenuBar { get; set; }
        public ObservableCollection<MenuButtonInfo> LeftMenuBar { get; set; }
        public ObservableCollection<NomenklStoreLocationItem> Remains { get; set; }
        public ObservableCollection<TransferOutBalansRemainsDocument> DocumentRows { set; get; }

        public ObservableCollection<StorageLocationsWrapper> StorageLocationList { set; get; }

        public StorageLocationsWrapper StorageLocation
        {
            set
            {
                if (Equals(value, myStorageLocation)) return;
                myStorageLocation = value;
                OnPropertyChanged();
                Task.Run(OnRefreshDataAsync);
            }
            get => myStorageLocation;
        }

        public TransferOutBalansRemainsDocument CurrentDocumentRow
        {
            set
            {
                if (Equals(value, myCurrentDocumentRow)) return;
                myCurrentDocumentRow = value;
                OnPropertyChanged();
                ((DelegateCommand)OpenDocumentCommand).RaiseCanExecuteChanged();
            }
            get => myCurrentDocumentRow;
        }

        public NomenklStoreLocationItem CurrentRemain
        {
            set
            {
                if (Equals(value, myCurrentRemain)) return;
                myCurrentRemain = value;
                DocumentRows.Clear();
                OnPropertyChanged();
                Task.Run(() => LoadDocumentsAsync(myCurrentRemain));
                ((DelegateCommand)OpenDocumentCommand).RaiseCanExecuteChanged();
            }
            get => myCurrentRemain;
        }

        public DateTime RemainDate
        {
            set
            {
                if (value.Equals(myRemainDate)) return;
                myRemainDate = value;
                OnPropertyChanged();
            }
            get => myRemainDate;
        }

        #endregion
    }
}
