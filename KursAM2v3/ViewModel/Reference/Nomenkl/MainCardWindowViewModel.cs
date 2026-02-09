using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.XtraEditors.DXErrorProvider;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Base;
using KursAM2.View.KursReferences;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using Newtonsoft.Json;
using StackExchange.Redis;
using NomenklMain = Data.NomenklMain;

namespace KursAM2.ViewModel.Reference.Nomenkl
{
    public sealed class MainCardWindowViewModel : RSWindowDataErrorInfoViewModelBase, IDXDataErrorInfo
    {
        private readonly Guid myId = Guid.Empty;
        private NomenklMainViewModel myNomenklMain;
        private readonly ConnectionMultiplexer redis;
        private readonly ISubscriber mySubscriber;

        public MainCardWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.NomenklCardRightBar(this);
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();
            LoadReferences();
            
        }

        public MainCardWindowViewModel(Guid id, NomenklGroupViewModel grp) : this()
        {
            if (id != Guid.Empty)
            {
                myId = id;
                RefreshData(null);
            }
            else
            {
                LoadReferences();
                DocNewEmpty(null);
                if (grp != null)
                {
                    NomenklMain.NomenklCategory = NomenklCategoryCollection.Single(_ => _.DocCode == grp.DocCode);
                    NomenklMain.CategoryDC = grp.DocCode;
                }
            }

            NomenklMain.Parent = this;
            //NomenklMain.State = RowStatus.NotEdited;;
        }


        public MainCardWindowViewModel(NomenklMainViewModel nom) : this()
        {
            LoadReferences();
            NomenklMain = new NomenklMainViewModel(nom.Entity)
            {
                Id = Guid.NewGuid(),
                myState = RowStatus.NewRow
            };
        }

        public ReferenceWindowViewModel ParentReference { set; get; }

        public ObservableCollection<NomenklType> NomenklTypeCollection { set; get; } =
            new ObservableCollection<NomenklType>();

        public ObservableCollection<ProductType> NomenklProductCollection { set; get; } =
            new ObservableCollection<ProductType>();

        public ObservableCollection<NomenklGroup> NomenklCategoryCollection { set; get; } =
            new ObservableCollection<NomenklGroup>();

        public ObservableCollection<CountriesViewModel> CountryCollection { set; get; }
            = new ObservableCollection<CountriesViewModel>();

        public ObservableCollection<Unit> UnitCollection { set; get; } = new ObservableCollection<Unit>();

        public override bool IsCanSaveData => isCanSave();


        private bool isCanSave()
        {
            if (NomenklMain?.Unit == null) return false;
            if (NomenklMain.ProductType == null) return false;
            if (NomenklMain.NomenklType == null) return false;
            if (NomenklMain.NomenklCategory == null) return false;
            if (string.IsNullOrEmpty(NomenklMain.Name)) return false;
            return NomenklMain.State != RowStatus.NotEdited;
        }

        //private NomenklProductViewModel myNomenklProduct;
        public ProductType NomenklProduct
        {
            get => NomenklMain?.ProductType;
            set
            {
                if (NomenklMain?.ProductType != null && NomenklMain.ProductType.Equals(value)) return;
                if (NomenklMain != null) NomenklMain.ProductType = value;
                RaisePropertyChanged();
            }
        }

        public bool IsMultyCurrency =>
            NomenklMain?.NomenklCollection.Count > 1 || NomenklMain?.State == RowStatus.NewRow;

        public NomenklMainViewModel NomenklMain
        {
            get => myNomenklMain;
            set
            {
                if (myNomenklMain != null && myNomenklMain.Equals(value)) return;
                myNomenklMain = value;
                RaisePropertyChanged();
            }
        }


        public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            switch (propertyName)
            {
                case "Name":
                    if (string.IsNullOrEmpty(Name))
                        SetErrorInfo(info,
                            "Наименование не может быть пустым.",
                            ErrorType.Critical);
                    break;
            }
        }

        public void GetError(ErrorInfo info)
        {
            //throw new NotImplementedException();
        }

        private void LoadReferences()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                NomenklTypeCollection.Clear();
                NomenklCategoryCollection.Clear();
                CountryCollection.Clear();
                UnitCollection.Clear();
                NomenklProductCollection.Clear();
                foreach (var t in ctx.Countries.ToList())
                    CountryCollection.Add(new CountriesViewModel(t));
                foreach (var t in ctx.SD_119.ToList())
                {
                    var item = new NomenklType();
                    item.LoadFromEntity(t);
                    NomenklTypeCollection.Add(item);
                }

                foreach (var c in ctx.SD_82.ToList())
                {
                    var n = new NomenklGroup();
                    n.LoadFromEntity(c);
                    NomenklCategoryCollection.Add(n);
                }

                foreach (var c in ctx.SD_175.ToList())
                {
                    var u = new Unit();
                    u.LoadFromEntity(c);
                    UnitCollection.Add(u);
                }

                foreach (var c in ctx.SD_50.ToList())
                {
                    var p = new ProductType();
                    p.LoadFromEntity(c);
                    NomenklProductCollection.Add(p);
                }
            }
        }

        public override void DocNewEmpty(object form)
        {
            Id = Guid.NewGuid();
            NomenklMain = new NomenklMainViewModel
            {
                State = RowStatus.NewRow
            };
        }

        public override void RefreshData(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (myId == Guid.Empty || (myNomenklMain != null && myNomenklMain.State == RowStatus.NewRow))
                    return;
                LoadReferences();
                var d =
                    ctx.NomenklMain
                        .Include(_ => _.SD_119)
                        .Include(_ => _.SD_175)
                        .Include(_ => _.SD_82)
                        .Include(_ => _.SD_83)
                        .Include(_ => _.SD_50)
                        .Include(_ => _.Countries)
                        .AsNoTracking()
                        .FirstOrDefault(_ => _.Id == myId);
                if (d == null) return;
                NomenklMain = new NomenklMainViewModel(d) { myState = RowStatus.NotEdited };
            }
        }

        public override void SaveData(object data)
        {
            var savedListId = new List<Guid>();
            var savedListDC = new List<decimal>();
            decimal newDC = 0;
            var state = NomenklMain.State;

            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        switch (NomenklMain.State)
                        {
                            case RowStatus.NewRow:
                                newDC = ctx.SD_83.Any() ? ctx.SD_83.Max(_ => _.DOC_CODE) + 1 : 10830000001;
                                if (string.IsNullOrEmpty(NomenklMain.NomenklNumber))
                                {
                                    var isNotUni = true;
                                    while (isNotUni)
                                    {
                                        var nDC = (newDC - 10830000000).ToString();
                                        while (ctx.NomenklMain.Any(_ => _.NomenklNumber == nDC))
                                            nDC = (Convert.ToInt32(nDC) + 1).ToString();
                                        if (!ctx.NomenklMain.Any(_ => _.NomenklNumber == nDC))
                                        {
                                            NomenklMain.NomenklNumber = nDC;
                                            isNotUni = false;
                                        }
                                        else
                                        {
                                            newDC++;
                                        }
                                    }
                                }

                                var newItem = new NomenklMain
                                {
                                    Id = NomenklMain.Id,
                                    Name = NomenklMain.Name,
                                    FullName = NomenklMain.FullName,
                                    CategoryDC = NomenklMain.CategoryDC,
                                    TypeDC = NomenklMain.NomenklType.DocCode,
                                    CountryId = NomenklMain.CountryId,
                                    NomenklNumber = NomenklMain.NomenklNumber,
                                    Note = NomenklMain.Note,
                                    IsComplex = false,
                                    IsNakladExpense = NomenklMain.IsNakladExpense,
                                    IsUsluga = NomenklMain.IsUsluga,
                                    UnitDC = NomenklMain.UnitDC,
                                    ProductDC = ((IDocCode)NomenklMain.ProductType).DocCode,
                                    IsRentabelnost = NomenklMain.IsRentabelnost,
                                    IsCurrencyTransfer = NomenklMain.IsCurrencyTransfer,
                                    IsOnlyState = NomenklMain.IsOnlyState,
                                    UpdateDate = DateTime.Now
                                };
                                ctx.NomenklMain.Add(newItem);
                                savedListId.Add(newItem.Id);
                                var crsDC = GlobalOptions.SystemProfile.NationalCurrency.DocCode;
                                var newNomItem = new SD_83
                                {
                                    DOC_CODE = newDC,
                                    NOM_NAME = NomenklMain.Name,
                                    NOM_NOMENKL = newItem.NomenklNumber + " " +
                                                  GlobalOptions.SystemProfile.NationalCurrency.Name,
                                    NOM_NOTES = newItem.Note,
                                    NOM_SALE_CRS_DC = crsDC,
                                    NOM_FULL_NAME = newItem.Name,
                                    NOM_ED_IZM_DC = newItem.UnitDC,
                                    NOM_CATEG_DC = newItem.CategoryDC,
                                    NOM_0MATER_1USLUGA = newItem.IsUsluga ? 1 : 0,
                                    NOM_1PROD_0MATER = 0,
                                    NOM_1NAKLRASH_0NO = newItem.IsNakladExpense ? 1 : 0,
                                    NOM_PRODUCT_DC = newItem.ProductDC,
                                    Id = Guid.NewGuid(),
                                    MainId = newItem.Id,
                                    IsUslugaInRent = newItem.IsRentabelnost,
                                    IsCurrencyTransfer = NomenklMain.IsCurrencyTransfer
                                };
                                ctx.SD_83.Add(newNomItem);
                                savedListDC.Add(newDC);

                                break;
                            case RowStatus.Edited:
                                var nomFormMain = ctx.SD_83.Where(_ => _.MainId == NomenklMain.Id).ToList();

                                if (nomFormMain.Count > 0)
                                    foreach (var n in nomFormMain)
                                    {
                                        n.NOM_NAME = NomenklMain.Name;
                                        n.NOM_0MATER_1USLUGA = NomenklMain.IsUsluga ? 1 : 0;
                                        n.NOM_1NAKLRASH_0NO = NomenklMain.IsNakladExpense ? 1 : 0;
                                        n.IsUslugaInRent = NomenklMain.IsRentabelnost;
                                        n.IsCurrencyTransfer = NomenklMain.IsCurrencyTransfer;
                                    }

                                var old = ctx.NomenklMain.SingleOrDefault(_ => _.Id == NomenklMain.Id);
                                if (old == null) return;
                                old.Name = NomenklMain.Name;
                                old.FullName = NomenklMain.FullName;
                                old.CategoryDC = NomenklMain.CategoryDC;
                                old.TypeDC = NomenklMain.NomenklType.DocCode;
                                old.CountryId = NomenklMain.CountryId;
                                old.NomenklNumber = NomenklMain.NomenklNumber;
                                old.Note = NomenklMain.Note;
                                old.IsComplex = false;
                                old.IsNakladExpense = NomenklMain.IsNakladExpense;
                                old.IsUsluga = NomenklMain.IsUsluga;
                                old.UnitDC = NomenklMain.UnitDC;
                                old.ProductDC = ((IDocCode)NomenklMain.ProductType).DocCode;
                                old.IsRentabelnost = NomenklMain.IsRentabelnost;
                                old.IsCurrencyTransfer = NomenklMain.IsCurrencyTransfer;
                                old.IsOnlyState = NomenklMain.IsOnlyState;
                                old.UpdateDate = DateTime.Now;
                                var oldNoms = ctx.SD_83.Where(_ => _.MainId == NomenklMain.Id);
                                foreach (var nom in oldNoms)
                                {
                                    nom.NOM_NAME = NomenklMain.Name;
                                    nom.IsCurrencyTransfer = NomenklMain.IsCurrencyTransfer;
                                }

                                savedListId.Add(NomenklMain.Id);
                                break;
                        }

                        ctx.SaveChanges();
                        tnx.Commit();
                        NomenklMain.myState = RowStatus.NotEdited;
                        RaisePropertyChanged(nameof(IsCanSaveData));

                        savedListDC.AddRange(from nom in NomenklMain.NomenklCollection
                            let n = GlobalOptions.ReferencesCache.GetNomenkl(nom.DocCode)
                            select nom.DocCode);

                        if (mySubscriber != null && mySubscriber.IsConnected())
                        {
                            var str = "обновление справочника NomenklMain";
                            var message = new RedisMessage
                            {
                                DocumentType = DocumentType.None,
                                DocCode = null,
                                Id = NomenklMain.Id,
                                DocDate = DateTime.Now,
                                IsDocument = false,
                                OperationType = RedisMessageDocumentOperationTypeEnum.Update,
                                Message =
                                    $"Пользователь '{GlobalOptions.UserInfo.Name}' {str} номенклатура: {NomenklMain.Id} {NomenklMain.Name}"
                            };
                            message.ExternalValues.Add("NomenklDC", NomenklMain.NomenklCollection);
                            message.ExternalValues.Add("RedisKey", $"Cache:NomenklMain:{NomenklMain.Id}@{DateTime.Now}");
                            var jsonSerializerSettings = new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.All
                            };
                            var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                            mySubscriber.Publish(
                                new RedisChannel(RedisMessageChannels.NomenklMainReference,
                                    RedisChannel.PatternMode.Auto), json);
                        }
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(ex);
                        return;
                    }

                    ParentReference?.LoadNomMainForCategory(null);
                    GlobalOptions.ReferencesCache.GetNomenklMain(NomenklMain.Id);
                }

                if (state == RowStatus.NewRow)
                    GlobalOptions.ReferencesCache.GetNomenkl(newDC);
            }
        }

        public override void DocNewCopy(object obj)
        {
            var newNom = new NomenklMainViewModel
            {
                State = RowStatus.NewRow,
                NomenklNumber = null,
                Id = Guid.NewGuid(),
                Name = NomenklMain.Name,
                FullName = NomenklMain.FullName,
                NomenklCategory = NomenklMain.NomenklCategory,
                NomenklType = NomenklMain.NomenklType,
                Country = NomenklMain.Country,
                Note = NomenklMain.Note,
                IsComplex = false,
                IsNakladExpense = NomenklMain.IsNakladExpense,
                IsUsluga = NomenklMain.IsUsluga,
                Unit = NomenklMain.Unit,
                ProductType = NomenklMain.ProductType,
                IsRentabelnost = NomenklMain.IsRentabelnost
            };
            var ctx = new MainCardWindowViewModel
            {
                ParentReference = ParentReference,
                NomenklMain = newNom
            };
            // ReSharper disable once UseObjectOrCollectionInitializer
            var form = new NomenklMainCardView { Owner = Application.Current.MainWindow, DataContext = ctx };
            //form.DataContext = ctx;
            form.Show();
        }

        private void SetErrorInfo(ErrorInfo info, string errorText, ErrorType errorType)
        {
            info.ErrorText = errorText;
            info.ErrorType = errorType;
        }

        #region Command

        public ICommand ClearCountryCommand
        {
            get { return new Command(ClearCountry, _ => NomenklMain?.Country != null); }
        }

        private void ClearCountry(object obj)
        {
            NomenklMain.Country = null;
        }

        public ICommand CategoryEditCommand
        {
            get { return new Command(CategoryEdit, _ => true); }
        }

        private void CategoryEdit(object obj)
        {
            var frm = new TreeListFormBaseView
            {
                LayoutManagerName = "NomenklCategory",
                Owner = Application.Current.MainWindow
            };
            var dtx = new CategoryReferenceWindowViewModel { Form = frm };
            dtx.RefreshData(null);
            frm.DataContext = dtx;
            frm.Closing += delegate { LoadReferences(); };
            frm.Show();
        }

        public ICommand TypeProductEditCommand
        {
            get { return new Command(TypeProductEdit, _ => true); }
        }

        private void TypeProductEdit(object obj)
        {
            var frm = new GridFormBaseView
            {
                DataContext = new ProductTypeReferenceWindowViewModel(),
                LayoutManagerName = "NomenklProductType",
                Owner = Application.Current.MainWindow
            };
            frm.Closing += delegate { LoadReferences(); };
            frm.Show();
        }

        public ICommand VidProductEditCommand
        {
            get { return new Command(VidProductEdit, _ => true); }
        }

        private void VidProductEdit(object obj)
        {
            var frm = new TreeListFormBaseView
            {
                LayoutManagerName = "NomenklProductKind",
                Owner = Application.Current.MainWindow
            };
            var dtx = new NomenklKindReferenceWindowViewModel { Form = frm };
            dtx.RefreshData(null);
            frm.DataContext = dtx;
            frm.Closing += delegate { LoadReferences(); };
            frm.Show();
        }

        public ICommand UnitEditCommand
        {
            get { return new Command(UnitEdit, _ => true); }
        }

        private void UnitEdit(object obj)
        {
            var frm = new GridFormBaseView
            {
                DataContext = new UnitReferenceWindowViewModel(),
                LayoutManagerName = "Unit",
                Owner = Application.Current.MainWindow
            };
            frm.Closing += delegate { LoadReferences(); };
            frm.Show();
        }

        public ICommand CountryEditCommand
        {
            get { return new Command(CountryEdit, _ => true); }
        }

        private void CountryEdit(object obj)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        #endregion
    }
}
