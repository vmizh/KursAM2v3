using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using DevExpress.XtraEditors.DXErrorProvider;
using KursAM2.View.Base;

namespace KursAM2.ViewModel.Reference.Nomenkl
{
    public class MainCardWindowViewModel : RSWindowDataErrorInfoViewModelBase
    {
        private readonly Guid myId = Guid.Empty;
        private NomenklMainViewModel myNomenklMain;

        public MainCardWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            LoadReferences();
        }

        public MainCardWindowViewModel(Guid id, NomenklGroup grp) : this()
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

        public ObservableCollection<NomenklProductType> NomenklTypeCollection { set; get; } =
            new ObservableCollection<NomenklProductType>();

        public ObservableCollection<NomenklProductKind> NomenklProductCollection { set; get; } =
            new ObservableCollection<NomenklProductKind>();

        public ObservableCollection<NomenklGroup> NomenklCategoryCollection { set; get; } =
            new ObservableCollection<NomenklGroup>();

        public ObservableCollection<Country> CountryCollection { set; get; } = new ObservableCollection<Country>();
        public ObservableCollection<Unit> UnitCollection { set; get; } = new ObservableCollection<Unit>();

        public override bool IsCanSaveData => NomenklMain != null && NomenklMain.State != RowStatus.NotEdited &&
                                              NomenklMain.Unit != null && NomenklMain.ProductType != null &&
                                              NomenklMain.NomenklCategory != null &&
                                              !string.IsNullOrEmpty(NomenklMain.Name);

        //private NomenklProductViewModel myNomenklProduct;
        public NomenklProductKind NomenklProduct
        {
            get => NomenklMain?.ProductType;
            set
            {
                if (NomenklMain?.ProductType != null && NomenklMain.ProductType.Equals(value)) return;
                if (NomenklMain != null) NomenklMain.ProductType = value;
                RaisePropertyChanged();
            }
        }

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
                    CountryCollection.Add(new Country(t));
                foreach (var t in ctx.SD_119.ToList())
                    NomenklTypeCollection.Add(new NomenklProductType(t));
                foreach (var c in ctx.SD_82.ToList())
                    NomenklCategoryCollection.Add(new NomenklGroup(c));
                foreach (var c in ctx.SD_175.ToList())
                    UnitCollection.Add(new Unit(c));
                foreach (var c in ctx.SD_50.ToList())
                    NomenklProductCollection.Add(new NomenklProductKind(c));
            }
        }

        public override void DocNewEmpty(object form)
        {
            Id = Guid.NewGuid();
            NomenklMain = new NomenklMainViewModel();
            NomenklMain.State = RowStatus.NewRow;
        }

        public override void RefreshData(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (myId == Guid.Empty || myNomenklMain != null && myNomenklMain.State == RowStatus.NewRow)
                    return;
                LoadReferences();
                var d =
                    ctx.NomenklMain
                        .AsNoTracking()
                        .FirstOrDefault(_ => _.Id == myId);
                if (d == null) return;
                NomenklMain = new NomenklMainViewModel(d) {myState = RowStatus.NotEdited};
            }
        }

        public override void SaveData(object data)
        {
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
                                    TypeDC = NomenklMain.NomenklTypeDC,
                                    CountryId = NomenklMain.CountryId,
                                    NomenklNumber = NomenklMain.NomenklNumber,
                                    Note = NomenklMain.Note,
                                    IsComplex = false,
                                    IsNakladExpense = NomenklMain.IsNakladExpense,
                                    IsUsluga = NomenklMain.IsUsluga,
                                    UnitDC = NomenklMain.UnitDC,
                                    ProductDC = NomenklMain.ProductType.DOC_CODE,
                                    IsRentabelnost = NomenklMain.IsRentabelnost
                                };
                                ctx.NomenklMain.Add(newItem);
                                var newNomItem = new SD_83
                                {
                                    DOC_CODE = newDC,
                                    NOM_NAME = newItem.Name,
                                    NOM_NOMENKL = newItem.NomenklNumber + " " +
                                                  GlobalOptions.SystemProfile.MainCurrency.Name,
                                    NOM_NOTES = newItem.Note,
                                    NOM_SALE_CRS_DC = GlobalOptions.SystemProfile.MainCurrency.DocCode,
                                    NOM_FULL_NAME = newItem.Name,
                                    NOM_ED_IZM_DC = newItem.UnitDC,
                                    NOM_CATEG_DC = newItem.CategoryDC,
                                    NOM_0MATER_1USLUGA = newItem.IsUsluga ? 1 : 0,
                                    NOM_1PROD_0MATER = 0,
                                    NOM_1NAKLRASH_0NO = newItem.IsNakladExpense ? 1 : 0,
                                    NOM_PRODUCT_DC = newItem.ProductDC,
                                    Id = Guid.NewGuid(),
                                    MainId = newItem.Id,
                                    IsUslugaInRent = newItem.IsRentabelnost
                                };
                                ctx.SD_83.Add(newNomItem);
                                break;
                            case RowStatus.Edited: 
                                var nomFormMain = ctx.SD_83.Where(_ => _.MainId == NomenklMain.Id).ToList();
                                
                                if (nomFormMain.Count > 0)
                                {
                                    foreach (var n in nomFormMain)
                                    {
                                        n.NOM_0MATER_1USLUGA = NomenklMain.IsUsluga ? 1 : 0;
                                        n.NOM_1NAKLRASH_0NO = NomenklMain.IsNakladExpense ? 1 : 0;
                                        n.IsUslugaInRent = NomenklMain.IsRentabelnost;
                                    }
                                }
                                var old = ctx.NomenklMain.SingleOrDefault(_ => _.Id == NomenklMain.Id);
                                if (old == null) return;
                                old.Name = NomenklMain.Name;
                                old.FullName = NomenklMain.FullName;
                                old.CategoryDC = NomenklMain.CategoryDC;
                                old.TypeDC = NomenklMain.NomenklTypeDC;
                                old.CountryId = NomenklMain.CountryId;
                                old.NomenklNumber = NomenklMain.NomenklNumber;
                                old.Note = NomenklMain.Note;
                                old.IsComplex = false;
                                old.IsNakladExpense = NomenklMain.IsNakladExpense;
                                old.IsUsluga = NomenklMain.IsUsluga;
                                old.UnitDC = NomenklMain.UnitDC;
                                old.ProductDC = NomenklMain.ProductType.DOC_CODE;
                                old.IsRentabelnost = NomenklMain.IsRentabelnost;

                               
                                break;
                        }
                        ctx.SaveChanges();
                        tnx.Commit();
                        NomenklMain.myState = RowStatus.NotEdited;
                        RaisePropertyChanged(nameof(IsCanSaveData));
                        ParentReference?.LoadNomMainForCategory(null);
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(ex);
                        return;
                    }
                }

                if (state == RowStatus.NewRow)
                    MainReferences.GetNomenkl(newDC);
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
                default:
                    SetErrorInfo(info,
                        "Наименование не может быть пустым.",
                        ErrorType.Critical);
                    break;
            }
        }

        private void SetErrorInfo(ErrorInfo info, string errorText, ErrorType errorType)
        {
            info.ErrorText = errorText;
            info.ErrorType = errorType;
        }

        public void GetError(ErrorInfo info)
        {
            //throw new NotImplementedException();
        }

        #region Command

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
            var dtx = new CategoryReferenceWindowViewModel {Form = frm};
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
            var dtx = new NomenklKindReferenceWindowViewModel {Form = frm};
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