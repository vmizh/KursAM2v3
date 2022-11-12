using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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
using KursDomain;
using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
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

        public KontragentViewModel Kontragent { set; get; }
        public List<Employee> Employees => GlobalOptions.ReferencesCache.GetEmployees().Cast<Employee>().ToList();

        public ObservableCollection<KontragentBank> BankAndAccounts { set; get; } =
            new ObservableCollection<KontragentBank>();

        public ObservableCollection<Bank> AllBanks { set; get; } = new ObservableCollection<Bank>();
        public List<Region> Regions { set; get; } = new List<Region>();

        public List<Currency> Currencies
            => GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Currency>().Where(_ => _.IsActive).ToList();

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
                Kontragent.ClientCategory = myClientCategory;
                RaisePropertyChanged();
            }
            get => myClientCategory;
        }

        private Employee myOtvetstLico;

        public Employee OtvetstLico
        {
            set
            {
                if (Equals(myOtvetstLico, value)) return;
                myOtvetstLico = value;
                Kontragent.OtvetstLico = myOtvetstLico;
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
                    Kontragent.REGION_DC = myCurrentRegions.DocCode;
                RaisePropertyChanged();
            }
            get => myCurrentRegions;
        }

        private KontragentBank myCurrentBankAndAccounts;

        public KontragentBank CurrentBankAndAccounts
        {
            set
            {
                if (Equals(myCurrentBankAndAccounts, value)) return;
                myCurrentBankAndAccounts = value;
                RaisePropertyChanged();
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

        public ICommand AddNewRequisiteCommand
        {
            get { return new Command(AddNewRequisite, _ => Kontragent.State != RowStatus.NewRow); }
        }

        private void AddNewRequisite(object obj)
        {
            var d = new SD_43_GRUZO
            {
                doc_code = Kontragent.DocCode,
                IsDefault = false,
                Id = Guid.NewGuid()
            };
            var newItem = new KontragentGruzoRequisite(d)
                { myState = RowStatus.NewRow };
            Kontragent.GruzoRequisities.Add(newItem);
            if (Kontragent.myState != RowStatus.NewRow) Kontragent.myState = RowStatus.Edited;
        }

        public ICommand AddCopyRequisiteCommand
        {
            get { return new Command(AddCopyRequisite, _ => CurrentGruzoRequisite != null); }
        }

        private void AddCopyRequisite(object obj)
        {
            var d = new SD_43_GRUZO
            {
                doc_code = Kontragent.DocCode,
                IsDefault = false,
                Id = Guid.NewGuid(),
                GRUZO_TEXT_NAKLAD = CurrentGruzoRequisite.GRUZO_TEXT_NAKLAD,
                GRUZO_TEXT_SF = CurrentGruzoRequisite.GRUZO_TEXT_SF,
                OKPO = CurrentGruzoRequisite.OKPO
            };
            var newItem = new KontragentGruzoRequisite(d)
                { myState = RowStatus.NewRow };
            Kontragent.GruzoRequisities.Add(newItem);
            if (Kontragent.myState != RowStatus.NewRow) Kontragent.myState = RowStatus.Edited;
        }

        public ICommand DeleteRequisiteCommand
        {
            get { return new Command(DeleteRequisite, _ => CurrentGruzoRequisite != null); }
        }

        private void DeleteRequisite(object obj)
        {
            DeletedKontragentGruzoRequisites.Add(CurrentGruzoRequisite);
            Kontragent.GruzoRequisities.Remove(CurrentGruzoRequisite);
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
            if (Kontragent.myState != RowStatus.NewRow)
            {
                Kontragent.myState = RowStatus.Edited;
                Kontragent.RaisePropertyChanged("State");
            }
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
                        RaisePropertyChanged(nameof(Kontragent));
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
            {
                var bank = new Bank();
                bank.LoadFromEntity(item);
                AllBanks.Add(bank);
            }

            Regions.Clear();
            foreach (var item in GlobalOptions.GetEntities().SD_23.ToList())
                Regions.Add(new Region(item));
            if (GetDocCode == null) return;
            var kontr = GlobalOptions.GetEntities()
                .SD_43
                .Include(_ => _.SD_301)
                .Include(_ => _.SD_148)
                .SingleOrDefault(_ => _.DOC_CODE == GetDocCode);
            Kontragent = new KontragentViewModel(kontr);
            RaisePropertyChanged(nameof(Kontragent));
            RaisePropertyChanged(nameof(Categories));
            RaisePropertyChanged(nameof(Regions));
            BankAndAccounts.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var item in ctx.TD_43.Where(_ => _.DOC_CODE == Kontragent.DocCode).ToList())
                    BankAndAccounts.Add(new KontragentBank(item)
                    {
                        myState = RowStatus.NotEdited
                    });
            }

            if (Kontragent.CLIENT_CATEG_DC != null)
            {
                ClientCategory =
                    GlobalOptions.ReferencesCache.GetClientCategory(Kontragent.CLIENT_CATEG_DC) as
                        ClientCategory;
                RaisePropertyChanged(nameof(ClientCategory));
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
                if (Equals(myEmployee, value)) return;
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
            Kontragent = copy;
        }

        private void NewData(int? groupId)
        {
            Kontragent = new KontragentViewModel
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

        public override bool IsCanSaveData => Kontragent?.State != RowStatus.NotEdited ||
                                              BankAndAccounts.Any(_ => _.State != RowStatus.NotEdited)
                                              || Kontragent.GruzoRequisities.Any(_ =>
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
                            if (doc.FLAG_BALANS != Kontragent.FLAG_BALANS)
                                if (ctx.AccruedAmountForClient.Any(_ => _.KontrDC == Kontragent.DocCode)
                                    || ctx.AccruedAmountOfSupplier.Any(_ => _.KontrDC == Kontragent.DocCode))
                                {
                                    WinManager.ShowWinUIMessageBox(@"Для контрагента есть документы " +
                                                                   "по внебалансовым начислениям! Смена статуса учета в балансе " +
                                                                   "не возможна!", "Предупреждение",
                                        MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }

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

                    if (ctx.TD_43.Any())
                        foreach (var item in BankAndAccounts)
                        {
                            var getCode = ctx.TD_43.Max(_ => _.CODE) + 1;
                            if (item.State == RowStatus.NewRow)
                                ctx.TD_43.Add(new TD_43
                                {
                                    RASCH_ACC = item.AccountNumber,
                                    DOC_CODE = Kontragent.DocCode,
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
                    foreach (var g in Kontragent.GruzoRequisities.Where(_ => State != RowStatus.NotEdited))
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
                    Kontragent.myState = RowStatus.NotEdited;
                    foreach (var b in BankAndAccounts) b.myState = RowStatus.NotEdited;
                    foreach (var g in Kontragent.GruzoRequisities) g.myState = RowStatus.NotEdited;
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
