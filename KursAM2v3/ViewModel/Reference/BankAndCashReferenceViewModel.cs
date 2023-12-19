using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.Dialogs;
using KursAM2.View.KursReferences;
using KursDomain;
using KursDomain.Documents.Bank;
using KursDomain.Documents.Cash;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference
{
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    public class BankAndCashReferenceViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public BankAndCashReferenceViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            RefreshData(null);
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

        public override bool IsCanSaveData => Cashs.Any(_ =>
            _.State != RowStatus.NotEdited || _.StartRemains.Any(s => s.State != RowStatus.NotEdited) || Banks.Any(b =>
                b.State != RowStatus.NotEdited));

        public bool IsCanUserRight => !IsCanSaveData;
        public ObservableCollection<CashReference> Cashs { set; get; } = new ObservableCollection<CashReference>();

        public ObservableCollection<BankAccountReference> Banks { set; get; } =
            new ObservableCollection<BankAccountReference>();

        public ObservableCollection<BankAccountReference> SelectedBanks { set; get; } =
            new ObservableCollection<BankAccountReference>();

        public ObservableCollection<BankCashItem> BankCash { set; get; } = new ObservableCollection<BankCashItem>();

        public ObservableCollection<SDRSchet> SHPZList { set; get; } =
            new ObservableCollection<SDRSchet>(GlobalOptions.ReferencesCache.GetSDRSchetAll().Cast<SDRSchet>());

        public ObservableCollection<BankCashItem> SelectedBankCash { set; get; } =
            new ObservableCollection<BankCashItem>();

        public ObservableCollection<SelectUser> Users { set; get; } = new ObservableCollection<SelectUser>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<SelectUser> SelectedUsers { set; get; } = new ObservableCollection<SelectUser>();
        private BankCashItem myCurrentBankCash;

        public BankCashItem CurrentBankCash
        {
            get => myCurrentBankCash;
            set
            {
                if (myCurrentBankCash == value) return;
                myCurrentBankCash = value;
                if (myCurrentBankCash != null)
                {
                    Users.Clear();
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var us = ctx.EXT_USERS.ToList();
                        switch (CurrentBankCash.TypeItem)
                        {
                            case "Касса":
                                var d1 = ctx.Database.SqlQuery<UserRight>("SELECT DOC_CODE as DocCode," +
                                                                          " USR_ID as UserId from HD_22 " +
                                                                          $" WHERE DOC_CODE = {CustomFormat.DecimalToSqlDecimal(myCurrentBankCash.DocCode)}");
                                foreach (var u in us)
                                {
                                    var usr = new SelectUser
                                    {
                                        UserId = u.USR_ID,
                                        UserFullName = u.USR_FULLNAME,
                                        UserName = u.USR_NICKNAME
                                    };
                                    usr.IsSelected = d1.Any(_ => _.UserId == u.USR_ID);
                                    Users.Add(usr);
                                }

                                break;
                            case "Банк":
                                var d2 = ctx.Database.SqlQuery<UserRight>("SELECT DOC_CODE as DocCode," +
                                                                          " USR_ID as UserId from HD_114 " +
                                                                          $" WHERE DOC_CODE = {CustomFormat.DecimalToSqlDecimal(myCurrentBankCash.DocCode)}");
                                foreach (var u in us)
                                {
                                    var usr = new SelectUser
                                    {
                                        UserId = u.USR_ID,
                                        UserFullName = u.USR_FULLNAME,
                                        UserName = u.USR_NICKNAME
                                    };
                                    usr.IsSelected = d2.Any(_ => _.UserId == u.USR_ID);
                                    Users.Add(usr);
                                }

                                break;
                        }
                    }
                }

                RaisePropertyChanged();
            }
        }

        private SelectUser myCurrentUser;

        public SelectUser CurrentUser
        {
            get => myCurrentUser;
            set
            {
                if (myCurrentUser == value) return;
                myCurrentUser = value;
                RaisePropertyChanged();
            }
        }

        public List<Currency> Currencies => GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Currency>().ToList();
        private CashStartRemains myCurrentRemain;

        public CashStartRemains CurrentRemain
        {
            get => myCurrentRemain;
            set
            {
                if (Equals(myCurrentRemain, value)) return;
                myCurrentRemain = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Currencies));
            }
        }

        private CashReference myCurrentCash;

        public CashReference CurrentCash
        {
            get => myCurrentCash;
            set
            {
                if (Equals(myCurrentCash, value)) return;
                myCurrentCash = value;
                RaisePropertyChanged();
            }
        }

        private BankAccountReference myCurrentBank;

        public BankAccountReference CurrentBank
        {
            get => myCurrentBank;
            set
            {
                if (Equals(myCurrentBank, value)) return;
                myCurrentBank = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public sealed override void RefreshData(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                Cashs.Clear();
                Banks.Clear();
                var cashs = ctx.SD_22.Include(_ => _.TD_22)
                    .Include(_ => _.SD_40).AsNoTracking().ToList();
                var banks = ctx.SD_114.Include(_ => _.SD_44)
                    .Include(_ => _.TD_43).Include(_ => _.SD_40).AsNoTracking()
                    .ToList();
                foreach (var c in cashs)
                    Cashs.Add(new CashReference(c)
                    {
                        State = RowStatus.NotEdited
                    });
                foreach (var b in banks)
                    Banks.Add(new BankAccountReference(b)
                    {
                        State = RowStatus.NotEdited
                    });
            }
        }

        public ICommand DeleteBankCommand
        {
            get { return new Command(DeleteBank, _ => CurrentBank != null); }
        }

        private void DeleteBank(object obj)
        {
            var WinManager = new WindowManager();
            if (CurrentBank == null) return;
            var res = MessageBox.Show($"Вы уверены, что хотите удалить данный банк {CurrentBank}?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        using (var transaction = ctx.Database.BeginTransaction())
                        {
                            try
                            {
                                if (ctx.SD_101.Any(_ => _.VV_ACC_DC == CurrentBank.DocCode))
                                {
                                    WinManager.ShowWinUIMessageBox(
                                        $"По банку {CurrentBank} существуют документы. Удаление не возможно.",
                                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }

                                ctx.Database.ExecuteSqlCommand("DELETE FROM HD_114 WHERE DOC_CODE = {0}",
                                    CurrentBank.DocCode);
                                var old = ctx.SD_114.FirstOrDefault(_ => _.DOC_CODE == CurrentBank.DocCode);
                                if (old == null) return;
                                ctx.SD_114.Remove(old);
                                ctx.SaveChanges();
                                transaction.Commit();
                                Banks.Remove(CurrentBank);
                                
                            }
                            catch (Exception ex)
                            {
                                if (transaction.UnderlyingTransaction.Connection != null)
                                    transaction.Rollback();
                                else
                                    transaction.Rollback();
                                WindowManager.ShowError(ex);
                            }
                        }
                    }

                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        public ICommand AddBankCommand
        {
            get { return new Command(AddBank, _ => true); }
        }

        private void AddBank(object obj)
        {
            var bank = StandartDialogs.SelectBank();
            if (bank == null) return;
            var newBank = new BankAccountReference
            {
                State = RowStatus.NewRow,
                Name = bank.NickName,
                BA_BANK_ACCOUNT = 1,
                CO = GlobalOptions.ReferencesCache.GetCentrResponsibilitiesAll().Count() == 1
                    ? GlobalOptions.ReferencesCache.GetCentrResponsibilitiesAll().First() as CentrResponsibility
                    : null,
                BA_CURRENCY = 1,
                IsNegative = false,
                BA_BANK_NAME = bank.Name,
                BA_TRANSIT = 0,
                BA_BANK_AS_KONTRAGENT_DC = null,
                Bank = bank
            };
            if (string.IsNullOrWhiteSpace(newBank.Name))
                newBank.Name = bank.Name;
            Banks.Add(newBank);
            if (Form is CashAndBanksReferenceView frm)
            {
                frm.tableViewBank.FocusedRowHandle =
                    frm.gridBank.GetRowHandleByListIndex(((IList)frm.gridBank.ItemsSource).Count - 1);
                frm.gridBank.SelectedItems.Clear();
                frm.gridBank.SelectedItems.Add(CurrentBank);
            }

            RaisePropertyChanged(nameof(IsCanUserRight));
        }

        public ICommand DeleteRemainCommand
        {
            get { return new Command(DeleteRemain, _ => CurrentRemain != null); }
        }

        private void DeleteRemain(object obj)
        {
            var WinManager = new WindowManager();
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (ctx.SD_33.Any(_ => _.CA_DC == CurrentRemain.DocCode && _.CRS_DC == CurrentRemain.CRS_DC)
                            || ctx.SD_34.Any(_ => _.CA_DC == CurrentRemain.DocCode && _.CRS_DC == CurrentRemain.CRS_DC)
                            || ctx.SD_251.Any(_ =>
                                _.CH_CASH_DC == CurrentRemain.DocCode && (_.CH_CRS_IN_DC == CurrentRemain.CRS_DC
                                                                          || _.CH_CRS_OUT_DC == CurrentRemain.CRS_DC)))
                        {
                            WinManager.ShowWinUIMessageBox(
                                $"По кассе {CurrentCash} и валюте {CurrentRemain.Currency} существуют документы. Удаление не возможно.",
                                "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }

                        var rem = ctx.TD_22.FirstOrDefault(_ =>
                            _.DOC_CODE == CurrentRemain.DocCode && _.CODE == CurrentRemain.Code);
                        if (rem != null)
                            ctx.TD_22.Remove(rem);
                        //var s39 = ctx.SD_39.Where(_ =>
                        //    _.CA_DC == CurrentRemain.Cash.DocCode && _.CRS_DC == CurrentRemain.Currency.DocCode);
                        //foreach (var s in s39) ctx.SD_39.Remove(s);
                        ctx.SaveChanges();
                        transaction.Commit();
                        CurrentRemain.Cash.StartRemains.Remove(CurrentRemain);
                    }
                    catch (Exception ex)
                    {
                        if (transaction.UnderlyingTransaction.Connection != null)
                            transaction.Rollback();
                        transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public ICommand AddRemainCommand
        {
            get { return new Command(AddRemain, _ => CurrentCash != null); }
        }

        private void AddRemain(object obj)
        {
            if (CurrentCash == null) return;
            var crs = StandartDialogs.SelectCurrency(
                new List<Currency>(CurrentCash.StartRemains.Select(_ => _.Currency).ToList()));
            if (crs == null) return;
            CurrentCash.StartRemains.Add(new CashStartRemains
            {
                DocCode = CurrentCash.DocCode,
                Code = CurrentCash.StartRemains.Count + 1,
                DATE_START = DateTime.Today,
                SUMMA_START = 0,
                State = RowStatus.NewRow,
                Currency = crs
            });
            if (CurrentCash.myState == RowStatus.NotEdited)
                CurrentCash.myState = RowStatus.Edited;
        }

        public ICommand AddCashCommand
        {
            get { return new Command(AddCash, _ => true); }
        }

        private void AddCash(object obj)
        {
            var newCash = new CashReference
            {
                Name = "Новая",
                DefaultCurrency = GlobalOptions.SystemProfile.MainCurrency,
                IsCanNegative = false,
                StartRemains = new ObservableCollection<CashStartRemains>(),
                CO = GlobalOptions.ReferencesCache.GetCentrResponsibilitiesAll().Count() == 1
                    ? GlobalOptions.ReferencesCache.GetCentrResponsibilitiesAll().First() as CentrResponsibility
                    : null,
                State = RowStatus.NewRow
            };
            foreach (var c in GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Currency>().ToList())
                newCash.StartRemains.Add(new CashStartRemains
                {
                    Currency = c,
                    SUMMA_START = 0,
                    DATE_START = DateTime.Today,
                    State = RowStatus.NewRow
                });
            Cashs.Add(newCash);
            CurrentCash = newCash;
        }

        public ICommand DeleteCashCommand
        {
            get { return new Command(DeleteCash, _ => CurrentCash != null); }
        }

        private void DeleteCash(object obj)
        {
            var res = MessageBox.Show($"Вы уверены, что хотите удалить кассу {CurrentCash} ?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    deleteCash();
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        private void deleteCash()
        {
            var WinManager = new WindowManager();
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (ctx.SD_33.Any(_ => _.CA_DC == CurrentCash.DocCode)
                            || ctx.SD_34.Any(_ => _.CA_DC == CurrentCash.DocCode)
                            || ctx.SD_251.Any(_ => _.CH_CASH_DC == CurrentCash.DocCode))
                        {
                            WinManager.ShowWinUIMessageBox(
                                $"По кассе {CurrentCash} существуют документы. Удаление не возможно.",
                                "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }

                        ctx.Database.ExecuteSqlCommand(
                            $"DELETE FROM HD_22 WHERE DOC_CODE = {CustomFormat.DecimalToSqlDecimal(CurrentCash.DocCode)}");
                        var rems = ctx.TD_22.Where(_ => _.DOC_CODE == CurrentCash.DocCode);
                        var ch = ctx.SD_22.FirstOrDefault(_ => _.DOC_CODE == CurrentCash.DocCode);
                        if (rems.Any())
                            foreach (var r in rems)
                                ctx.TD_22.Remove(r);
                        var sd39 = ctx.SD_39.Where(_ => _.CA_DC == CurrentCash.DocCode);
                        foreach (var s in sd39) ctx.SD_39.Remove(s);
                        if (ch != null)
                            ctx.SD_22.Remove(ch);
                        ctx.SaveChanges();
                        transaction.Commit();
                        Cashs.Remove(CurrentCash);
                    }
                    catch (Exception ex)
                    {
                        if (transaction.UnderlyingTransaction.Connection != null)
                            transaction.Rollback();
                        else
                            transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var r in Cashs.Where(_ => _.State != RowStatus.NotEdited))
                            switch (r.State)
                            {
                                case RowStatus.NewRow:
                                    addNewCash(ctx, r);
                                    break;
                                case RowStatus.Edited:
                                    updateCash(ctx, r);
                                    break;
                            }

                        var newKontrAcc = ctx.TD_43.Where(_ =>
                            _.DOC_CODE == GlobalOptions.SystemProfile.OwnerKontragent.DocCode);
                        var newKontrAccCode = 1;
                        if (newKontrAcc.Any())
                            newKontrAccCode = newKontrAcc.Max(c => c.CODE) + 1;
                        var newDC = ctx.SD_114.Any() ? ctx.SD_114.Max(_ => _.DOC_CODE) + 1 : 11140000001;
                        foreach (var b in Banks.Where(_ => _.State != RowStatus.NotEdited))
                            switch (b.State)
                            {
                                case RowStatus.NewRow:
                                    addNewBank(ctx, b, newKontrAccCode, newDC);
                                    newKontrAccCode++;
                                    newDC++;
                                    break;
                                case RowStatus.Edited:
                                    updateBank(ctx, b);
                                    break;
                            }

                        ctx.SaveChanges();
                        transaction.Commit();
                        foreach (var c in Cashs)
                        {
                            foreach (var r in c.StartRemains)
                                r.myState = RowStatus.NotEdited;
                            c.myState = RowStatus.NotEdited;
                        }

                        foreach (var b in Banks) b.myState = RowStatus.NotEdited;
                        RaisePropertyChanged(nameof(IsCanSaveData));
                        RaisePropertyChanged(nameof(IsCanUserRight));
                    }
                    catch (Exception ex)
                    {
                        if (transaction.UnderlyingTransaction.Connection != null)
                            transaction.Rollback();
                        //transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        private void updateBank(ALFAMEDIAEntities ctx, BankAccountReference bAcc)
        {
            var old = ctx.SD_114.FirstOrDefault(_ => _.DOC_CODE == bAcc.DocCode);
            if (old == null) return;
            old.BA_BANK_NAME = bAcc.BA_BANK_NAME;
            old.BA_ACC_SHORTNAME = bAcc.BA_ACC_SHORTNAME;
            //old.BA_BANKDC = bAcc.BA_BANKDC;
            old.BA_CENTR_OTV_DC = bAcc.BA_CENTR_OTV_DC;
            old.BA_BANK_ACCOUNT = bAcc.BA_BANK_ACCOUNT;
            old.BA_CURRENCY = bAcc.BA_CURRENCY;
            old.BA_NEGATIVE_RESTS = bAcc.BA_NEGATIVE_RESTS;
            old.BA_BANK_AS_KONTRAGENT_DC = bAcc.BA_BANK_AS_KONTRAGENT_DC;
            old.BA_RASH_ACC = bAcc.BA_RASH_ACC;
            old.BA_RASH_ACC_CODE = bAcc.BA_RASH_ACC_CODE;
            old.BA_TRANSIT = bAcc.BA_TRANSIT;
            old.CurrencyDC = bAcc.Currency?.DocCode;
            old.StartSumma = bAcc.StartSumma;
            old.StartDate = bAcc.StartDate;
            old.IsDeleted = bAcc.IsDeleted;
            old.DateNonZero = bAcc.DateNonZero;
        }

        private void addNewBank(ALFAMEDIAEntities ctx, BankAccountReference bAcc, int newKontrAccCode, decimal newDC)
        {
            ctx.TD_43.Add(new TD_43
            {
                DOC_CODE = GlobalOptions.SystemProfile.OwnerKontragent.DocCode,
                CODE = newKontrAccCode,
                BANK_DC = bAcc.BA_BANKDC,
                DELETED = 0,
                DISABLED = 0,
                RASCH_ACC = bAcc.BA_RASH_ACC
            });
            ctx.SD_114.Add(new SD_114
            {
                DOC_CODE = newDC,
                BA_RASH_ACC_CODE = newKontrAccCode,
                BA_BANK_NAME = bAcc.BA_BANK_NAME,
                BA_ACC_SHORTNAME = bAcc.BA_ACC_SHORTNAME,
                BA_BANKDC = bAcc.BA_BANKDC,
                BA_CENTR_OTV_DC = bAcc.BA_CENTR_OTV_DC,
                BA_BANK_ACCOUNT = bAcc.BA_BANK_ACCOUNT,
                BA_CURRENCY = bAcc.BA_CURRENCY,
                BA_NEGATIVE_RESTS = bAcc.BA_NEGATIVE_RESTS,
                BA_BANK_AS_KONTRAGENT_DC = bAcc.BA_BANK_AS_KONTRAGENT_DC,
                BA_RASH_ACC = bAcc.BA_RASH_ACC,
                BA_TRANSIT = bAcc.BA_TRANSIT,
                IsDeleted = bAcc.IsDeleted,
                CurrencyDC = bAcc.Currency?.DocCode,
                StartSumma = bAcc.StartSumma,
                StartDate = bAcc.StartDate,
                DateNonZero = bAcc.DateNonZero
            });
            bAcc.DocCode = newDC;
            bAcc.Code = newKontrAccCode;
        }

        private void addNewCash(ALFAMEDIAEntities ctx, CashReference cash)
        {
            var newDC = ctx.SD_22.Any() ? ctx.SD_22.Max(_ => _.DOC_CODE) + 1 : 10220000001;
            var newCash = new SD_22
            {
                DOC_CODE = newDC,
                CA_NAME = cash.Name,
                CA_CENTR_OTV_DC = cash.CO?.DocCode,
                CA_CRS_DC = cash.DefaultCurrency.DocCode,
                CA_NEGATIVE_RESTS = (short?)(cash.IsCanNegative ? 1 : 0),
                CA_NO_BALANS = 0,
                CA_KONTRAGENT_DC = null,
                CA_KONTR_DC = null
            };
            ctx.SD_22.Add(newCash);
            var code = 1;
            foreach (var r in cash.StartRemains)
            {
                var codeCashDC = ctx.SD_39.Any() ? ctx.SD_39.Max(_ => _.DOC_CODE) + 1 : 10390000001;
                ctx.SD_39.Add(new SD_39
                {
                    DOC_CODE = codeCashDC,
                    CA_DC = newDC,
                    CRS_DC = r.Currency.DocCode,
                    DATE_CASS = r.DATE_START,
                    MONEY_START = r.SUMMA_START,
                    MONEY_STOP = r.SUMMA_START
                });
                var newRem = new TD_22
                {
                    DOC_CODE = newDC,
                    CODE = code,
                    CRS_DC = r.Currency.DocCode,
                    DATE_START = r.DATE_START,
                    SUMMA_START = r.SUMMA_START,
                    CASH_DATE_DC = codeCashDC
                };
                ctx.TD_22.Add(newRem);
                ctx.SaveChanges();
                r.DocCode = newDC;
                r.Code = code;
                code++;
            }

            cash.DocCode = newDC;
        }

        private void updateCash(ALFAMEDIAEntities ctx, CashReference cash)
        {
            var WinManager = new WindowManager();
            var old = ctx.SD_22.FirstOrDefault(_ => _.DOC_CODE == cash.DocCode);
            if (old == null) return;
            old.CA_NAME = cash.Name;
            old.CA_CENTR_OTV_DC = cash.CO?.DocCode;
            old.CA_CRS_DC = cash.DefaultCurrency.DocCode;
            old.CA_NEGATIVE_RESTS = (short?)(cash.IsCanNegative ? 1 : 0);
            old.CA_NO_BALANS = 0;
            old.CA_KONTRAGENT_DC = null;
            old.CA_KONTR_DC = null;
            foreach (var r in cash.StartRemains)
                switch (r.State)
                {
                    case RowStatus.Edited:
                        var oldTD22 = ctx.TD_22.FirstOrDefault(_ =>
                            _.DOC_CODE == cash.DocCode && _.CRS_DC == r.Currency.DocCode);
                        if (oldTD22 == null)
                        {
                            var mcode = ctx.TD_22.Any(_ => _.DOC_CODE == cash.DocCode)
                                ? ctx.TD_22.Where(_ => _.DOC_CODE == cash.DocCode).Max(_ => _.CODE) + 1
                                : 1;
                            var newRem2 = new TD_22
                            {
                                DOC_CODE = cash.DocCode,
                                CODE = mcode,
                                CRS_DC = r.Currency.DocCode,
                                DATE_START = r.DATE_START,
                                SUMMA_START = r.SUMMA_START,
                                CASH_DATE_DC = null
                            };
                            ctx.TD_22.Add(newRem2);
                        }
                        else
                        {
                            if (oldTD22.DATE_START < r.DATE_START)
                                if (ctx.SD_33.Any(_ => _.CA_DC == r.DocCode && _.CRS_DC == r.CRS_DC)
                                    || ctx.SD_34.Any(_ => _.CA_DC == r.DocCode && _.CRS_DC == r.CRS_DC)
                                    || ctx.SD_251.Any(_ => _.CH_CASH_DC == r.DocCode && (_.CH_CRS_IN_DC == r.CRS_DC
                                        || _.CH_CRS_OUT_DC ==
                                        r.CRS_DC)))
                                {
                                    WinManager.ShowWinUIMessageBox(
                                        $"По кассе {CurrentCash} и валюте {r.Currency} существуют документы до {r.DATE_START} . Изменение не возможно.",
                                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }

                            oldTD22.CASH_DATE_DC = null;
                            oldTD22.SUMMA_START = r.SUMMA_START;
                            oldTD22.DATE_START = r.DATE_START;
                        }

                        break;
                    case RowStatus.NewRow: // ReSharper disable once TooWideLocalVariableScope
                        var code = ctx.TD_22.Any(_ => _.DOC_CODE == cash.DocCode)
                            ? (int)ctx.TD_22.Where(_ => _.DOC_CODE == cash.DocCode).Max(_ => _.CODE) + 1
                            : 1;
                        var newRem = new TD_22
                        {
                            DOC_CODE = cash.DocCode,
                            CODE = code,
                            CRS_DC = r.Currency.DocCode,
                            DATE_START = r.DATE_START,
                            SUMMA_START = r.SUMMA_START,
                            CASH_DATE_DC = null
                        };
                        ctx.TD_22.Add(newRem);
                        r.Code = code;
                        break;
                }
        }

        #endregion

        #region Methods

        #endregion
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class BankCashItem
    {
        [Display(Name = "DocCode", AutoGenerateField = false)]
        [ReadOnly(true)]
        public decimal DocCode { set; get; }

        [Display(Name = "Тип", Description = "Банк/Касса")]
        [ReadOnly(true)]
        public string TypeItem { set; get; }

        [Display(Name = "Наименование")]
        [ReadOnly(true)]
        public string Name { set; get; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SelectUser
    {
        [Display(Name = "Дать права")] public bool IsSelected { set; get; }

        [Display(Name = "Пользователь")]
        [ReadOnly(true)]
        public string UserName { set; get; }

        [Display(Name = "Имя пользователя")]
        [ReadOnly(true)]
        public string UserFullName { set; get; }

        [Display(Name = "Id", AutoGenerateField = false)]
        [ReadOnly(true)]
        public int UserId { set; get; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class UserRight
    {
        public decimal DocCode { set; get; }
        public int UserId { set; get; }
    }
}
