using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
using DevExpress.Xpf.Navigation;
using KursAM2.View.Base;
using KursAM2.View.KursReferences;
using KursAM2.View.KursReferences.KontragentControls;

namespace KursAM2.ViewModel.Reference
{
    public class KontragentCardWindowViewModel : RSWindowViewModelBase
    {
        private readonly decimal? GetDocCode;

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public ObservableCollection<BankAndAccounts> DeletedBankAndAccountses =
            new ObservableCollection<BankAndAccounts>();

        private TileBarItem mySelectedTab;

        public KontragentCardWindowViewModel()
        {
            RightMenuBar = MenuGenerator.KontragentCardRightBar(this);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            IsNewDoc = true;
            Categories.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_148.ToList())
                Categories.Add(new KontragentCategory(item));
        }

        public KontragentCardWindowViewModel(decimal dc) : this()
        {
            RightMenuBar = MenuGenerator.KontragentCardRightBar(this);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            if (dc < 0)
                // ReSharper disable once ObjectCreationAsStatement
                // ReSharper disable once NotResolvedInText
                new ArgumentNullException("Неправильный код контрагента");
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            IsNewDoc = false;
            GetDocCode = dc;
            RefreshData(dc);
        }

        public KontragentCardWindowViewModel(decimal dc, int groupId, bool IsCopyOld) : this()
        {
            RightMenuBar = MenuGenerator.KontragentCardRightBar(this);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            IsNewDoc = true;
            if (IsCopyOld)
                CopyData(dc);
            else
                NewData(groupId);
        }

        public ObservableCollection<KontragentCategory> Categories { set; get; }
            = new ObservableCollection<KontragentCategory>();

        public Kontragent Kontragent { set; get; }
        public List<Employee> Employees => MainReferences.Employees.Values.ToList();

        public ObservableCollection<BankAndAccounts> BankAndAccounts { set; get; } =
            new ObservableCollection<BankAndAccounts>();

        public ObservableCollection<TD_43ViewModel> Accounts { set; get; } = new ObservableCollection<TD_43ViewModel>();
        public ObservableCollection<Bank> AllBanks { set; get; } = new ObservableCollection<Bank>();
        public List<Region> Regions { set; get; } = new List<Region>();

        public List<Currency> Currencies
            => MainReferences.Currencies.Values.Where(_ => (_.CRS_ACTIVE ?? 0) == 1).ToList();

        #region Property

        private KontragentCategory myCategory;

        public KontragentCategory Category
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCategory == value) return;
                myCategory = value;
                Kontragent.Category = myCategory;
                RaisePropertiesChanged();
            }
            get => myCategory;
        }

        private Employee myOtvetstLico;

        public Employee OtvetstLico
        {
            set
            {
                if (myOtvetstLico != null && myOtvetstLico.Equals(value)) return;
                myOtvetstLico = value;
                Kontragent.OtvetstLico = myOtvetstLico;
                RaisePropertiesChanged();
            }
            get => myOtvetstLico;
        }

        private Currency myCurrentCurrencies;

        public Currency CurrentCurrencies
        {
            set
            {
                if (Equals(myCurrentCurrencies, value)) return;
                myCurrentCurrencies = value;
                RaisePropertiesChanged();
            }
            get => myCurrentCurrencies;
        }

        private Region myCurrentRegions;

        public Region CurrentRegions
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentRegions == value) return;
                myCurrentRegions = value;
                if (myCurrentRegions != null)
                    Kontragent.REGION_DC = myCurrentRegions.DocCode;
                RaisePropertiesChanged();
            }
            get => myCurrentRegions;
        }

        private BankAndAccounts myCurrentBankAndAccounts;

        public BankAndAccounts CurrentBankAndAccounts
        {
            set
            {
                if (myCurrentBankAndAccounts != null && myCurrentBankAndAccounts.Equals(value)) return;
                myCurrentBankAndAccounts = value;
                RaisePropertiesChanged();
            }
            get => myCurrentBankAndAccounts;
        }

        public bool IsCurrencyChangeEnable => Kontragent.State == RowStatus.NewRow;
        public bool IsNewDoc { set; get; }

        public TileBarItem SelectedTab
        {
            set
            {
                if (Equals(mySelectedTab, value)) return;
                mySelectedTab = value;
                if (Form is KontragentCardView w && mySelectedTab != null)
                    w.ChangedTab(mySelectedTab.Name);
                RaisePropertyChanged();
            }
            get => mySelectedTab;
        }

        #endregion

        #region Command

        public ICommand CategoryReferenceCommand
        {
            get { return new Command(CategoryReference, _ => true); }
        }

        private void CategoryReference(object obj)
        {
            var ctxKontrCat = new KontragentCategoryRefWindowViewModel();
            var form = new KontragentCategoryReferenceView
            {
                DataContext = ctxKontrCat,
                Owner = Application.Current.MainWindow
            };
            ctxKontrCat.Form = form;
            form.Show();
            form.Closed += Form_Closed;
        }

        private void Form_Closed(object sender, EventArgs e)
        {
            Categories.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_148.ToList())
                Categories.Add(new KontragentCategory(item));
            Regions.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_23.ToList())
                Regions.Add(new Region(item));
            RaisePropertiesChanged(nameof(Categories));
            RaisePropertiesChanged(nameof(Regions));
        }

        public ICommand DeleteBankCommand
        {
            get { return new Command(DeleteBank, _ => true); }
        }

        private void DeleteBank(object obj)
        {
            if (CurrentBankAndAccounts == null) return;
            var item = BankAndAccounts.FirstOrDefault(_ => _.Code == CurrentBankAndAccounts.Code);
            if (item != null)
            {
                item.DELETED = 1;
                item.State = RowStatus.Edited;
            }

            DeletedBankAndAccountses.Add(CurrentBankAndAccounts);
            BankAndAccounts.Remove(CurrentBankAndAccounts);
        }

        public ICommand AddNewBankCommand
        {
            get { return new Command(AddNewBank, _ => true); }
        }

        private void AddNewBank(object obj)
        {
            BankAndAccounts.Add(new BankAndAccounts {State = RowStatus.NewRow});
            RaisePropertiesChanged(nameof(Kontragent.KontragentBanks));
        }

        public override void RefreshData(object data)
        {
            if (Kontragent != null && IsCanSaveData)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                var f = Form as KontragentCardView;
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        LoadKontragent();
                        f?.ChangedTab("mainTab");
                        return;
                    case MessageBoxResult.No:
                        LoadKontragent();
                        f?.ChangedTab("mainTab");
                        RaisePropertiesChanged(nameof(Kontragent));
                        return;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            LoadKontragent();
        }

        private void LoadKontragent()
        {
            DeletedBankAndAccountses.Clear();
            Categories.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_148.ToList())
                Categories.Add(new KontragentCategory(item));
            AllBanks.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_44.ToList())
                AllBanks.Add(new Bank(item));
            Regions.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_23.ToList())
                Regions.Add(new Region(item));
            Accounts.Clear();
            if (GetDocCode == null) return;
            var kontr = GlobalOptions.GetEntities()
                .SD_43
                .Include(_ => _.SD_301)
                .Include(_ => _.SD_148)
                .SingleOrDefault(_ => _.DOC_CODE == GetDocCode);
            Kontragent = new Kontragent(kontr);
            RaisePropertiesChanged(nameof(Kontragent));
            RaisePropertiesChanged(nameof(Categories));
            RaisePropertiesChanged(nameof(Regions));
            Accounts.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (ctx.TD_43.FirstOrDefault(_ => _.DOC_CODE == Kontragent.DOC_CODE) != null)
                    foreach (var item in ctx.TD_43)
                        Accounts.Add(new TD_43ViewModel(item));
            }

            BankAndAccounts.Clear();
            foreach (var item in Accounts)
            {
                if (item.DELETED == 1) continue;
                if (item.DOC_CODE != Kontragent.DocCode) continue;
                BankAndAccounts.Add(new BankAndAccounts
                {
                    Id = Guid.NewGuid(),
                    RASCH_ACC = item.RASCH_ACC,
                    DISABLED = item.DISABLED,
                    BankDC = item.BANK_DC,
                    Code = item.Code,
                    Bank = AllBanks.FirstOrDefault(_ => _.DOC_CODE == item.BANK_DC),
                    State = RowStatus.NotEdited
                });
            }

            if (Kontragent.CLIENT_CATEG_DC != null)
            {
                Category = Categories.FirstOrDefault(_ => _.DocCode == Kontragent.CLIENT_CATEG_DC);
                RaisePropertyChanged(nameof(Category));
            }

            if (Kontragent.OTVETSTV_LICO != null)
                OtvetstLico = Employees.FirstOrDefault(_ => _.TabelNumber == Kontragent.OTVETSTV_LICO);
            if (Kontragent.VALUTA_DC != null)
                CurrentCurrencies = Currencies.FirstOrDefault(_ => _.DocCode == Kontragent.VALUTA_DC);
            if (Kontragent.TABELNUMBER != null)
                Employee = Employees.FirstOrDefault(_ => _.TabelNumber == Kontragent.TABELNUMBER);
            if (Kontragent.REGION_DC != null)
                CurrentRegions = Regions.FirstOrDefault(_ => _.DOC_CODE == Kontragent.REGION_DC);
            Kontragent.State = RowStatus.NotEdited;
        }

        private Employee myEmployee;

        public Employee Employee
        {
            get => myEmployee;
            set
            {
                if (myEmployee != null && myEmployee.Equals(value)) return;
                myEmployee = value;
                RaisePropertyChanged();
            }
        }

        private void CopyData(object data)
        {
            if (!(data is decimal)) return;
            var kontr = GlobalOptions.GetEntities()
                .SD_43
                .Include(_ => _.SD_43_GRUZO)
                .Include(_ => _.TD_43)
                .Include(_ => _.SD_301)
                .Include(_ => _.SD_148)
                .SingleOrDefault(_ => _.DOC_CODE == (decimal) data);
            var copy = new Kontragent(kontr);
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    copy.DocCode = ctx.SD_43.Max(_ => _.DOC_CODE) + 1;
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
            }

            copy.Name = null;
            copy.NAME_FULL = null;
            copy.Id = Guid.NewGuid();
            Kontragent = copy;
        }

        private void NewData(int? groupId)
        {
            Kontragent = new Kontragent
            {
                Id = Guid.NewGuid(),
                Group = groupId != null ? new KontragentGroup {EG_ID = groupId.Value} : null
            };
        }

        public ICommand RegionsReferenceCommand
        {
            get { return new Command(RegionsReference, _ => true); }
        }

        private void RegionsReference(object obj)
        {
            var ctxKontrCat = new RegionRefViewModel();
            var form = new TreeListFormBaseView
            {
                DataContext = ctxKontrCat,
                Owner = Application.Current.MainWindow
            };
            ctxKontrCat.Form = form;
            form.Show();
            form.Closed += Form_Closed;
        }

        public override bool IsCanSaveData => Kontragent?.State != RowStatus.NotEdited;

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    var CurrentCategory = Category?.DocCode;
                    var newDC = ctx.SD_43.Max(_ => _.DOC_CODE) + 1;
                    var OtvetstvennojeLico = OtvetstLico?.TabelNumber;
                    var myValuteDC = CurrentCurrencies?.DocCode;
                    var myTabelnumber = Employee?.TabelNumber;
                    var GetRegion = CurrentRegions?.DOC_CODE;
                    if (IsNewDoc)
                    {
                        ctx.SD_43.Add(new SD_43
                        {
                            DOC_CODE = newDC,
                            INN = Kontragent.INN,
                            NAME = Kontragent.Name,
                            HEADER = Kontragent.Header,
                            GLAVBUH = Kontragent.GlavBuh,
                            NOTES = Kontragent.Note,
                            TYPE_PROP = Kontragent.TYPE_PROP,
                            ADDRESS = Kontragent.ADDRESS,
                            TEL = Kontragent.TEL,
                            FAX = Kontragent.FAX,
                            OKONH = Kontragent.OKONH,
                            PASSPORT = Kontragent.PASSPORT,
                            SHIPPING_AUTO_DAYS = Kontragent.SHIPPING_AUTO_DAYS,
                            PAYMENT_DAYS = Kontragent.PAYMENT_DAYS,
                            EG_ID = Kontragent.EG_ID <= 0 ? null : Kontragent.EG_ID,
                            TABELNUMBER = myTabelnumber,
                            NAL_PAYER_DC = Kontragent.NAL_PAYER_DC,
                            REGION_DC = GetRegion,
                            CLIENT_CATEG_DC = CurrentCategory,
                            AUTO_CLIENT_CATEGORY = Kontragent.AUTO_CLIENT_CATEGORY,
                            AB_OTRASL_DC = Kontragent.AB_OTRASL_DC,
                            AB_BUDGET_DC = Kontragent.AB_BUDGET_DC,
                            AB_MINISTRY_DC = Kontragent.AB_MINISTRY_DC,
                            PODRAZD_CORP_GOLOVNOE = Kontragent.PODRAZD_CORP_GOLOVNOE,
                            PODRAZD_CORP_OBOSOBL = Kontragent.PODRAZD_CORP_OBOSOBL,
                            FLAG_BALANS = Kontragent.FLAG_BALANS,
                            VALUTA_DC = myValuteDC,
                            START_BALANS = Kontragent.START_BALANS,
                            START_SUMMA = Kontragent.START_SUMMA,
                            INNER_CODE = Kontragent.INNER_CODE,
                            NAME_FULL = Kontragent.NAME_FULL,
                            NO_NDS = Kontragent.NO_NDS,
                            PREFIX_IN_NUMBER = Kontragent.PREFIX_IN_NUMBER,
                            CONTAKT_LICO = Kontragent.CONTAKT_LICO,
                            KASSIR = Kontragent.KASSIR,
                            SPOSOB_OTPRAV_DC = Kontragent.SPOSOB_OTPRAV_DC,
                            KPP = Kontragent.KPP,
                            KONTR_DISABLE = Kontragent.KONTR_DISABLE,
                            MAX_KREDIT_SUM = Kontragent.MAX_KREDIT_SUM,
                            TRANSP_KOEF = Kontragent.TRANSP_KOEF,
                            TELEKS = Kontragent.TELEKS,
                            E_MAIL = Kontragent.E_MAIL,
                            WWW = Kontragent.WWW,
                            OTVETSTV_LICO = OtvetstvennojeLico,
                            LAST_MAX_VERSION = Kontragent.LAST_MAX_VERSION,
                            FLAG_0UR_1PHYS = Kontragent.FLAG_0UR_1PHYS,
                            OKPO = Kontragent.OKPO,
                            UpdateDate = DateTime.Now
                        });
                    }
                    else
                    {
                        var doc = ctx.SD_43.FirstOrDefault(_ => _.DOC_CODE == Kontragent.DocCode);
                        if (doc != null)
                        {
                            doc.INN = Kontragent.INN;
                            doc.NAME = Kontragent.Name;
                            doc.HEADER = Kontragent.Header;
                            doc.GLAVBUH = Kontragent.GlavBuh;
                            doc.NOTES = Kontragent.Note;
                            doc.TYPE_PROP = Kontragent.TYPE_PROP;
                            doc.ADDRESS = Kontragent.ADDRESS;
                            doc.TEL = Kontragent.TEL;
                            doc.FAX = Kontragent.FAX;
                            doc.OKONH = Kontragent.OKONH;
                            doc.PASSPORT = Kontragent.PASSPORT;
                            doc.SHIPPING_AUTO_DAYS = Kontragent.SHIPPING_AUTO_DAYS;
                            doc.PAYMENT_DAYS = Kontragent.PAYMENT_DAYS;
                            doc.EG_ID = Kontragent.EG_ID <= 0 ? null : Kontragent.EG_ID;
                            doc.TABELNUMBER = myTabelnumber;
                            doc.NAL_PAYER_DC = Kontragent.NAL_PAYER_DC;
                            doc.REGION_DC = GetRegion;
                            doc.CLIENT_CATEG_DC = CurrentCategory;
                            doc.AUTO_CLIENT_CATEGORY = Kontragent.AUTO_CLIENT_CATEGORY;
                            doc.AB_OTRASL_DC = Kontragent.AB_OTRASL_DC;
                            doc.AB_BUDGET_DC = Kontragent.AB_BUDGET_DC;
                            doc.AB_MINISTRY_DC = Kontragent.AB_MINISTRY_DC;
                            doc.PODRAZD_CORP_GOLOVNOE = Kontragent.PODRAZD_CORP_GOLOVNOE;
                            doc.PODRAZD_CORP_OBOSOBL = Kontragent.PODRAZD_CORP_OBOSOBL;
                            doc.FLAG_BALANS = Kontragent.FLAG_BALANS;
                            doc.VALUTA_DC = myValuteDC;
                            doc.START_BALANS = Kontragent.START_BALANS;
                            doc.START_SUMMA = Kontragent.START_SUMMA;
                            doc.INNER_CODE = Kontragent.INNER_CODE;
                            doc.NAME_FULL = Kontragent.NAME_FULL;
                            doc.NO_NDS = Kontragent.NO_NDS;
                            doc.PREFIX_IN_NUMBER = Kontragent.PREFIX_IN_NUMBER;
                            doc.CONTAKT_LICO = Kontragent.CONTAKT_LICO;
                            doc.KASSIR = Kontragent.KASSIR;
                            doc.SPOSOB_OTPRAV_DC = Kontragent.SPOSOB_OTPRAV_DC;
                            doc.KPP = Kontragent.KPP;
                            doc.KONTR_DISABLE = Kontragent.KONTR_DISABLE;
                            doc.MAX_KREDIT_SUM = Kontragent.MAX_KREDIT_SUM;
                            doc.TRANSP_KOEF = Kontragent.TRANSP_KOEF;
                            doc.TELEKS = Kontragent.TELEKS;
                            doc.E_MAIL = Kontragent.E_MAIL;
                            doc.WWW = Kontragent.WWW;
                            doc.OTVETSTV_LICO = OtvetstvennojeLico;
                            doc.LAST_MAX_VERSION = Kontragent.LAST_MAX_VERSION;
                            doc.FLAG_0UR_1PHYS = Kontragent.FLAG_0UR_1PHYS;
                            doc.OKPO = Kontragent.OKPO;
                            doc.UpdateDate = DateTime.Now;
                        }
                    }

                    foreach (var item in BankAndAccounts)
                    {
                        var getCode = ctx.TD_43.Max(_ => _.CODE) + 1;
                        if (item.State == RowStatus.NewRow)
                            ctx.TD_43.Add(new TD_43
                            {
                                RASCH_ACC = item.RASCH_ACC,
                                DOC_CODE = Kontragent.DOC_CODE,
                                DISABLED = item.DISABLED,
                                BANK_DC = item.BankDC,
                                CODE = getCode
                            });
                        if (item.State == RowStatus.Edited)
                        {
                            var i = ctx.TD_43.FirstOrDefault(_ => _.CODE == item.Code);
                            if (i != null)
                            {
                                i.BANK_DC = item.BankDC;
                                i.DISABLED = item.DISABLED;
                                i.DELETED = item.DELETED;
                                i.RASCH_ACC = item.RASCH_ACC;
                            }
                        }
                    }

                    if (DeletedBankAndAccountses != null)
                        foreach (var item in DeletedBankAndAccountses)
                        {
                            var firstOrDefault = ctx.TD_43.FirstOrDefault(_ => _.CODE == item.Code);
                            if (firstOrDefault != null)
                                firstOrDefault.DELETED = 1;
                        }

                    // ReSharper disable once PossibleNullReferenceException
                    DeletedBankAndAccountses.Clear();
                    ctx.SaveChanges();
                    if (IsNewDoc)
                    {
                        Kontragent.DocCode = newDC;
                        MainReferences.AllKontragents.Add(Kontragent.DocCode, Kontragent);
                        MainReferences.ActiveKontragents.Add(Kontragent.DocCode, Kontragent);
                        MainReferences.KontragentLastUpdate =
                            (DateTime) MainReferences.AllKontragents.Values.Select(_ => _.UpdateDate).Max();
                    }
                    else
                    {
                        MainReferences.AllKontragents[Kontragent.DocCode] = Kontragent;
                        MainReferences.ActiveKontragents[Kontragent.DocCode] = Kontragent;
                        MainReferences.KontragentLastUpdate =
                            (DateTime)MainReferences.AllKontragents.Values.Select(_ => _.UpdateDate).Max();
                    }

                    Kontragent.myState = RowStatus.NotEdited;
                }
                catch (Exception e)
                {
                    WindowManager.ShowError(e);
                }
            }
        }

        #endregion
    }
}