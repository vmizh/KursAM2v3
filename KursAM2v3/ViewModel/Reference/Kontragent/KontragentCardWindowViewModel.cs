using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Navigation;
using KursAM2.View.Base;
using KursAM2.View.KursReferences;
using KursAM2.View.KursReferences.KontragentControls;
using KursAM2.ViewModel.Reference.Dialogs;
using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using Bank = KursDomain.Documents.Bank.Bank;
using Region = KursDomain.Documents.CommonReferences.Region;

namespace KursAM2.ViewModel.Reference.Kontragent
{
    public sealed class KontragentCardWindowViewModel : RSWindowViewModelBase
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public ObservableCollection<KontragentBank> DeletedBankAndAccountses =
            new ObservableCollection<KontragentBank>();

        public ObservableCollection<KontragentGruzoRequisite> DeletedKontragentGruzoRequisites =
            new ObservableCollection<KontragentGruzoRequisite>();

        private TileBarItem mySelectedTab;

        public KontragentCardWindowViewModel()
        {
            RightMenuBar = MenuGenerator.KontragentCardRightBar(this);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            IsNewDoc = true;
            Categories.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_148.ToList())
                Categories.Add(new KontragentClientCategory(item));
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

        public ObservableCollection<KontragentClientCategory> Categories { set; get; }
            = new ObservableCollection<KontragentClientCategory>();

        public KontragentViewModel KontragentViewModel { set; get; }
        public List<Employee> Employees => GlobalOptions.ReferencesCache.GetEmployees().Cast<Employee>().ToList();

        public ObservableCollection<KontragentBank> BankAndAccounts { set; get; } =
            new ObservableCollection<KontragentBank>();

        public ObservableCollection<Bank> AllBanks { set; get; } = new ObservableCollection<Bank>();
        public List<Region> Regions { set; get; } = new List<Region>();

        public List<Currency> Currencies
            => MainReferences.Currencies.Values.Where(_ => _.IsActive).ToList();

        #region Fields

        private readonly decimal? GetDocCode;
        private KontragentGruzoRequisite myCurrentGruzoRequisite;

        #endregion

        #region Property

        public KontragentGruzoRequisite CurrentGruzoRequisite
        {
            get => myCurrentGruzoRequisite;
            set
            {
                if (myCurrentGruzoRequisite == value) return;
                myCurrentGruzoRequisite = value;
                RaisePropertyChanged();
            }
        }

        private ClientCategory myClientCategory;

        public ClientCategory ClientCategory
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myClientCategory == value) return;
                myClientCategory = value;
                KontragentViewModel.ClientCategory = myClientCategory;
                RaisePropertyChanged();
            }
            get => myClientCategory;
        }

        private Employee myOtvetstLico;

        public Employee OtvetstLico
        {
            set
            {
                if (myOtvetstLico != null && myOtvetstLico.Equals(value)) return;
                myOtvetstLico = value;
                KontragentViewModel.OtvetstLico = myOtvetstLico;
                RaisePropertyChanged();
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
                RaisePropertyChanged();
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
                    KontragentViewModel.REGION_DC = myCurrentRegions.DocCode;
                RaisePropertyChanged();
            }
            get => myCurrentRegions;
        }

        private KontragentBank myCurrentBankAndAccounts;

        public KontragentBank CurrentBankAndAccounts
        {
            set
            {
                if (myCurrentBankAndAccounts != null && myCurrentBankAndAccounts.Equals(value)) return;
                myCurrentBankAndAccounts = value;
                RaisePropertyChanged();
            }
            get => myCurrentBankAndAccounts;
        }

        public bool IsCurrencyChangeEnable => KontragentViewModel.State == RowStatus.NewRow;
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

        public ICommand AddNewRequisiteCommand
        {
            get { return new Command(AddNewRequisite, _ => KontragentViewModel.State != RowStatus.NewRow); }
        }

        private void AddNewRequisite(object obj)
        {
            var d = new SD_43_GRUZO
            {
                doc_code = KontragentViewModel.DocCode,
                IsDefault = false,
                Id = Guid.NewGuid()
            };
            var newItem = new KontragentGruzoRequisite(d)
                { myState = RowStatus.NewRow };
            KontragentViewModel.GruzoRequisities.Add(newItem);
            if (KontragentViewModel.myState != RowStatus.NewRow) KontragentViewModel.myState = RowStatus.Edited;
        }

        public ICommand AddCopyRequisiteCommand
        {
            get { return new Command(AddCopyRequisite, _ => CurrentGruzoRequisite != null); }
        }

        private void AddCopyRequisite(object obj)
        {
            var d = new SD_43_GRUZO
            {
                doc_code = KontragentViewModel.DocCode,
                IsDefault = false,
                Id = Guid.NewGuid(),
                GRUZO_TEXT_NAKLAD = CurrentGruzoRequisite.GRUZO_TEXT_NAKLAD,
                GRUZO_TEXT_SF = CurrentGruzoRequisite.GRUZO_TEXT_SF,
                OKPO = CurrentGruzoRequisite.OKPO
            };
            var newItem = new KontragentGruzoRequisite(d)
                { myState = RowStatus.NewRow };
            KontragentViewModel.GruzoRequisities.Add(newItem);
            if (KontragentViewModel.myState != RowStatus.NewRow) KontragentViewModel.myState = RowStatus.Edited;
        }

        public ICommand DeleteRequisiteCommand
        {
            get { return new Command(DeleteRequisite, _ => CurrentGruzoRequisite != null); }
        }

        private void DeleteRequisite(object obj)
        {
            DeletedKontragentGruzoRequisites.Add(CurrentGruzoRequisite);
            KontragentViewModel.GruzoRequisities.Remove(CurrentGruzoRequisite);
        }

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
                Categories.Add(new KontragentClientCategory(item));
            Regions.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_23.ToList())
                Regions.Add(new Region(item));
            RaisePropertyChanged(nameof(Categories));
            RaisePropertyChanged(nameof(Regions));
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
                item.IsDeleted = true;
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
            var ctx = new BankSelectDialogViewModel();
            var service = this.GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.Cancel) return;
            if (ctx.CurrentItem == null) return;
            BankAndAccounts.Add(new KontragentBank
            {
                Bank = ctx.CurrentItem,
                State = RowStatus.NewRow
            });
            if (KontragentViewModel.myState != RowStatus.NewRow)
            {
                KontragentViewModel.myState = RowStatus.Edited;
                KontragentViewModel.RaisePropertyChanged("State");
            }
        }

        public override void RefreshData(object data)
        {
            if (KontragentViewModel != null && IsCanSaveData)
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
                        RaisePropertyChanged(nameof(KontragentViewModel));
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
                Categories.Add(new KontragentClientCategory(item));
            AllBanks.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_44.ToList())
                AllBanks.Add(new Bank(item));
            Regions.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_23.ToList())
                Regions.Add(new Region(item));
            if (GetDocCode == null) return;
            var kontr = GlobalOptions.GetEntities()
                .SD_43
                .Include(_ => _.SD_301)
                .Include(_ => _.SD_148)
                .SingleOrDefault(_ => _.DOC_CODE == GetDocCode);
            KontragentViewModel = new KontragentViewModel(kontr);
            RaisePropertyChanged(nameof(KontragentViewModel));
            RaisePropertyChanged(nameof(Categories));
            RaisePropertyChanged(nameof(Regions));
            BankAndAccounts.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var item in ctx.TD_43.Where(_ => _.DOC_CODE == KontragentViewModel.DocCode).ToList())
                    BankAndAccounts.Add(new KontragentBank(item)
                    {
                        myState = RowStatus.NotEdited
                    });
            }

            if (KontragentViewModel.CLIENT_CATEG_DC != null)
            {
                ClientCategory =
                    GlobalOptions.ReferencesCache.GetClientCategory(KontragentViewModel.CLIENT_CATEG_DC) as
                        ClientCategory;
                RaisePropertyChanged(nameof(ClientCategory));
            }

            if (KontragentViewModel.OTVETSTV_LICO != null)
                OtvetstLico = Employees.FirstOrDefault(_ => _.TabelNumber == KontragentViewModel.OTVETSTV_LICO);
            if (KontragentViewModel.VALUTA_DC != null)
                CurrentCurrencies = Currencies.FirstOrDefault(_ => _.DocCode == KontragentViewModel.VALUTA_DC);
            if (KontragentViewModel.TABELNUMBER != null)
                Employee = Employees.FirstOrDefault(_ => _.TabelNumber == KontragentViewModel.TABELNUMBER);
            if (KontragentViewModel.REGION_DC != null)
                CurrentRegions = Regions.FirstOrDefault(_ => _.DOC_CODE == KontragentViewModel.REGION_DC);
            KontragentViewModel.State = RowStatus.NotEdited;
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
                .SingleOrDefault(_ => _.DOC_CODE == (decimal)data);
            var copy = new KontragentViewModel(kontr);
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
            KontragentViewModel = copy;
        }

        private void NewData(int? groupId)
        {
            KontragentViewModel = new KontragentViewModel
            {
                Id = Guid.NewGuid(),
                Group = groupId != null ? new KontragentGroupViewModel { EG_ID = groupId.Value } : null
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

        public override bool IsCanSaveData => KontragentViewModel?.State != RowStatus.NotEdited ||
                                              BankAndAccounts.Any(_ => _.State != RowStatus.NotEdited)
                                              || KontragentViewModel.GruzoRequisities.Any(_ =>
                                                  _.State != RowStatus.NotEdited)
                                              || DeletedBankAndAccountses.Count > 0
                                              || DeletedKontragentGruzoRequisites.Count > 0;

        public override void SaveData(object data)
        {
            var WinManager = new WindowManager();
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    var CurrentCategory = ClientCategory?.DocCode;
                    var newDC = ctx.SD_43.Any() ? ctx.SD_43.Max(_ => _.DOC_CODE) + 1 : 10430000001;
                    var OtvetstvennojeLico = OtvetstLico?.TabelNumber;
                    var myValuteDC = CurrentCurrencies?.DocCode;
                    var myTabelnumber = Employee?.TabelNumber;
                    var GetRegion = CurrentRegions?.DOC_CODE;
                    if (IsNewDoc)
                    {
                        ctx.SD_43.Add(new SD_43
                        {
                            DOC_CODE = newDC,
                            INN = KontragentViewModel.INN,
                            NAME = KontragentViewModel.Name,
                            HEADER = KontragentViewModel.Header,
                            GLAVBUH = KontragentViewModel.GlavBuh,
                            NOTES = KontragentViewModel.Note,
                            TYPE_PROP = KontragentViewModel.TYPE_PROP,
                            ADDRESS = KontragentViewModel.ADDRESS,
                            TEL = KontragentViewModel.TEL,
                            FAX = KontragentViewModel.FAX,
                            OKONH = KontragentViewModel.OKONH,
                            PASSPORT = KontragentViewModel.PASSPORT,
                            SHIPPING_AUTO_DAYS = KontragentViewModel.SHIPPING_AUTO_DAYS,
                            PAYMENT_DAYS = KontragentViewModel.PAYMENT_DAYS,
                            EG_ID = KontragentViewModel.EG_ID <= 0 ? null : KontragentViewModel.EG_ID,
                            TABELNUMBER = myTabelnumber,
                            NAL_PAYER_DC = KontragentViewModel.NAL_PAYER_DC,
                            REGION_DC = GetRegion,
                            CLIENT_CATEG_DC = CurrentCategory,
                            AUTO_CLIENT_CATEGORY = KontragentViewModel.AUTO_CLIENT_CATEGORY,
                            AB_OTRASL_DC = KontragentViewModel.AB_OTRASL_DC,
                            AB_BUDGET_DC = KontragentViewModel.AB_BUDGET_DC,
                            AB_MINISTRY_DC = KontragentViewModel.AB_MINISTRY_DC,
                            PODRAZD_CORP_GOLOVNOE = KontragentViewModel.PODRAZD_CORP_GOLOVNOE,
                            PODRAZD_CORP_OBOSOBL = KontragentViewModel.PODRAZD_CORP_OBOSOBL,
                            FLAG_BALANS = KontragentViewModel.FLAG_BALANS,
                            VALUTA_DC = myValuteDC,
                            START_BALANS = KontragentViewModel.START_BALANS,
                            START_SUMMA = KontragentViewModel.START_SUMMA,
                            INNER_CODE = KontragentViewModel.INNER_CODE,
                            NAME_FULL = KontragentViewModel.NAME_FULL,
                            NO_NDS = KontragentViewModel.NO_NDS,
                            PREFIX_IN_NUMBER = KontragentViewModel.PREFIX_IN_NUMBER,
                            CONTAKT_LICO = KontragentViewModel.CONTAKT_LICO,
                            KASSIR = KontragentViewModel.KASSIR,
                            SPOSOB_OTPRAV_DC = KontragentViewModel.SPOSOB_OTPRAV_DC,
                            KPP = KontragentViewModel.KPP,
                            KONTR_DISABLE = KontragentViewModel.KONTR_DISABLE,
                            MAX_KREDIT_SUM = KontragentViewModel.MAX_KREDIT_SUM,
                            TRANSP_KOEF = KontragentViewModel.TRANSP_KOEF,
                            TELEKS = KontragentViewModel.TELEKS,
                            E_MAIL = KontragentViewModel.E_MAIL,
                            WWW = KontragentViewModel.WWW,
                            OTVETSTV_LICO = OtvetstvennojeLico,
                            LAST_MAX_VERSION = KontragentViewModel.LAST_MAX_VERSION,
                            FLAG_0UR_1PHYS = KontragentViewModel.FLAG_0UR_1PHYS,
                            OKPO = KontragentViewModel.OKPO,
                            UpdateDate = DateTime.Now
                        });
                    }
                    else
                    {
                        var doc = ctx.SD_43.FirstOrDefault(_ => _.DOC_CODE == KontragentViewModel.DocCode);
                        if (doc != null)
                        {
                            if (doc.FLAG_BALANS != KontragentViewModel.FLAG_BALANS)
                                if (ctx.AccruedAmountForClient.Any(_ => _.KontrDC == KontragentViewModel.DocCode)
                                    || ctx.AccruedAmountOfSupplier.Any(_ => _.KontrDC == KontragentViewModel.DocCode))
                                {
                                    WinManager.ShowWinUIMessageBox(@"Для контрагента есть документы " +
                                                                   "по внебалансовым начислениям! Смена статуса учета в балансе " +
                                                                   "не возможна!", "Предупреждение",
                                        MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }

                            doc.INN = KontragentViewModel.INN;
                            doc.NAME = KontragentViewModel.Name;
                            doc.HEADER = KontragentViewModel.Header;
                            doc.GLAVBUH = KontragentViewModel.GlavBuh;
                            doc.NOTES = KontragentViewModel.Note;
                            doc.TYPE_PROP = KontragentViewModel.TYPE_PROP;
                            doc.ADDRESS = KontragentViewModel.ADDRESS;
                            doc.TEL = KontragentViewModel.TEL;
                            doc.FAX = KontragentViewModel.FAX;
                            doc.OKONH = KontragentViewModel.OKONH;
                            doc.PASSPORT = KontragentViewModel.PASSPORT;
                            doc.SHIPPING_AUTO_DAYS = KontragentViewModel.SHIPPING_AUTO_DAYS;
                            doc.PAYMENT_DAYS = KontragentViewModel.PAYMENT_DAYS;
                            doc.EG_ID = KontragentViewModel.EG_ID <= 0 ? null : KontragentViewModel.EG_ID;
                            doc.TABELNUMBER = myTabelnumber;
                            doc.NAL_PAYER_DC = KontragentViewModel.NAL_PAYER_DC;
                            doc.REGION_DC = GetRegion;
                            doc.CLIENT_CATEG_DC = CurrentCategory;
                            doc.AUTO_CLIENT_CATEGORY = KontragentViewModel.AUTO_CLIENT_CATEGORY;
                            doc.AB_OTRASL_DC = KontragentViewModel.AB_OTRASL_DC;
                            doc.AB_BUDGET_DC = KontragentViewModel.AB_BUDGET_DC;
                            doc.AB_MINISTRY_DC = KontragentViewModel.AB_MINISTRY_DC;
                            doc.PODRAZD_CORP_GOLOVNOE = KontragentViewModel.PODRAZD_CORP_GOLOVNOE;
                            doc.PODRAZD_CORP_OBOSOBL = KontragentViewModel.PODRAZD_CORP_OBOSOBL;
                            doc.FLAG_BALANS = KontragentViewModel.FLAG_BALANS;
                            doc.VALUTA_DC = myValuteDC;
                            doc.START_BALANS = KontragentViewModel.START_BALANS;
                            doc.START_SUMMA = KontragentViewModel.START_SUMMA;
                            doc.INNER_CODE = KontragentViewModel.INNER_CODE;
                            doc.NAME_FULL = KontragentViewModel.NAME_FULL;
                            doc.NO_NDS = KontragentViewModel.NO_NDS;
                            doc.PREFIX_IN_NUMBER = KontragentViewModel.PREFIX_IN_NUMBER;
                            doc.CONTAKT_LICO = KontragentViewModel.CONTAKT_LICO;
                            doc.KASSIR = KontragentViewModel.KASSIR;
                            doc.SPOSOB_OTPRAV_DC = KontragentViewModel.SPOSOB_OTPRAV_DC;
                            doc.KPP = KontragentViewModel.KPP;
                            doc.KONTR_DISABLE = KontragentViewModel.KONTR_DISABLE;
                            doc.MAX_KREDIT_SUM = KontragentViewModel.MAX_KREDIT_SUM;
                            doc.TRANSP_KOEF = KontragentViewModel.TRANSP_KOEF;
                            doc.TELEKS = KontragentViewModel.TELEKS;
                            doc.E_MAIL = KontragentViewModel.E_MAIL;
                            doc.WWW = KontragentViewModel.WWW;
                            doc.OTVETSTV_LICO = OtvetstvennojeLico;
                            doc.LAST_MAX_VERSION = KontragentViewModel.LAST_MAX_VERSION;
                            doc.FLAG_0UR_1PHYS = KontragentViewModel.FLAG_0UR_1PHYS;
                            doc.OKPO = KontragentViewModel.OKPO;
                            doc.UpdateDate = DateTime.Now;
                        }
                    }

                    if (ctx.TD_43.Any())
                        foreach (var item in BankAndAccounts)
                        {
                            var getCode = ctx.TD_43.Max(_ => _.CODE) + 1;
                            if (item.State == RowStatus.NewRow)
                                ctx.TD_43.Add(new TD_43
                                {
                                    RASCH_ACC = item.AccountNumber,
                                    DOC_CODE = KontragentViewModel.DocCode,
                                    DISABLED = (short?)(item.IsDisabled ? 1 : 0),
                                    BANK_DC = item.Bank?.DocCode,
                                    USE_FOR_TLAT_TREB = (short?)(item.IsForPrint ? 1 : 0),
                                    CODE = getCode
                                });
                            if (item.State == RowStatus.Edited)
                            {
                                var i = ctx.TD_43.FirstOrDefault(_ => _.CODE == item.Code);
                                if (i != null)
                                {
                                    i.BANK_DC = item.Bank?.DocCode;
                                    i.DISABLED = (short?)(item.IsDisabled ? 1 : 0);
                                    i.DELETED = (short?)(item.IsDeleted ? 1 : 0);
                                    i.RASCH_ACC = item.AccountNumber;
                                    i.USE_FOR_TLAT_TREB = (short?)(item.IsForPrint ? 1 : 0);
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
                    foreach (var g in KontragentViewModel.GruzoRequisities.Where(_ => State != RowStatus.NotEdited))
                        switch (g.State)
                        {
                            case RowStatus.NewRow:
                                ctx.SD_43_GRUZO.Add(g.Entity);
                                break;
                            case RowStatus.Edited:
                                var old = ctx.SD_43_GRUZO.FirstOrDefault(_ => _.Id == g.Id);
                                if (old != null)
                                {
                                    old.GRUZO_TEXT_SF = g.GRUZO_TEXT_SF;
                                    old.OKPO = g.OKPO;
                                    old.GRUZO_TEXT_NAKLAD = g.GRUZO_TEXT_NAKLAD;
                                }

                                break;
                        }

                    foreach (var g in DeletedKontragentGruzoRequisites)
                    {
                        var old = ctx.SD_43_GRUZO.FirstOrDefault(_ => _.Id == g.Id);
                        ctx.SD_43_GRUZO.Remove(old);
                    }

                    ctx.SaveChanges();

                    DeletedBankAndAccountses.Clear();
                    DeletedKontragentGruzoRequisites.Clear();
                    KontragentViewModel.myState = RowStatus.NotEdited;
                    foreach (var b in BankAndAccounts) b.myState = RowStatus.NotEdited;
                    foreach (var g in KontragentViewModel.GruzoRequisities) g.myState = RowStatus.NotEdited;
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
