using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Finance;
using KursAM2.View.Finance.AccruedAmount;
using KursAM2.View.Helper;
using KursAM2.ViewModel.Finance.AccruedAmount;
using KursDomain;
using KursDomain.Documents.Bank;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Currency;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Finance
{
    [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
    public sealed class BankOperationsWindowViewModel2 : RSWindowViewModelBase
    {
        #region Fields

        private readonly BankOperationsManager manager = new BankOperationsManager();

        #endregion

        #region Commands

        public override bool IsDocumentOpenAllow => CurrentBankAccount != null && CurrentBankOperations != null;

        protected override void DocumentOpen(object obj)
        {
            if (CurrentBankOperations.IsCurrencyChange)
            {
                var id = Guid.Empty;
                using (var ctx = GlobalOptions.GetEntities())
                {
                    if (CurrentBankOperations.VVT_VAL_PRIHOD > 0)
                    {
                        var i = ctx.BankCurrencyChange
                            .FirstOrDefault(_ => _.DocToDC == CurrentBankOperations.DOC_CODE
                                                 && _.DocRowToCode == CurrentBankOperations.Code);
                        if (i != null)
                            id = i.Id;
                    }
                    else
                    {
                        var i = ctx.BankCurrencyChange
                            .FirstOrDefault(_ => _.DocFromDC == CurrentBankOperations.DOC_CODE
                                                 && _.DocRowFromCode == CurrentBankOperations.Code);
                        if (i != null)
                            id = i.Id;
                    }
                }

                var dtx = new BankCurrencyChangeWindowViewModel(id)
                {
                    ParentForm = this
                };
                var form = new BankCurrencyChangeView
                {
                    Owner = Form,
                    DataContext = dtx
                };
                dtx.Form = form;
                form.Show();
            }
            else
            {
                if (CurrentBankOperations.CurrencyRateForReference == 0)
                {
                    decimal rate = 1;
                    if (((IDocCode)CurrentBankAccount.Currency).DocCode !=
                        GlobalOptions.SystemProfile.NationalCurrency.DocCode)
                    {
                        var rates = CurrencyRate.GetRate(DateTime.Today);
                        if (rates.ContainsKey(CurrentBankAccount.Currency as Currency))
                            rate = rates[CurrentBankAccount.Currency as Currency];
                        CurrentBankOperations.CurrencyRateForReference = rate;
                    }
                }

                var k = StandartDialogs.OpenBankOperation(CurrentBankAccount.DocCode,
                    CurrentBankOperations,
                    GlobalOptions.ReferencesCache.GetBankAccount(CurrentBankAccount.DocCode) as BankAccount);
                if (k != null)
                {
                    k.State = RowStatus.Edited;
                    manager.SaveBankOperations(k, CurrentBankAccount.DocCode, 0);
                    BankOperationsCollection.Add(k);
                    BankOperationsCollection.First(_ => _.DOC_CODE == k.DOC_CODE && _.Code == k.Code).State =
                        RowStatus.NotEdited;
                    UpdateValueInWindow(k);
                    CurrentPeriods = Periods.FirstOrDefault(_ => _.DateStart == k.Date && _.DateEnd == k.Date);
                    CurrentBankOperations = k;
                }
            }
        }

        public override void DocNewEmpty(object form)
        {
            if (CurrentBankAccount != null)
            {
                var acc = GlobalOptions.ReferencesCache.GetBankAccount(CurrentBankAccount.DocCode) as BankAccount;
                var k = StandartDialogs.AddNewBankOperation(CurrentBankAccount.DocCode, new BankOperationsViewModel() {Currency = acc?.Currency as Currency},
                    acc);
                if (k != null)
                {
                    k.State = RowStatus.NewRow;
                    manager.SaveBankOperations(k, CurrentBankAccount.DocCode, 0);
                    if (k.VVT_SFACT_POSTAV_DC != null)
                        using (var context = GlobalOptions.GetEntities())
                        {
                            context.Database.ExecuteSqlCommand(
                                $"EXEC dbo.GenerateSFProviderCash {CustomFormat.DecimalToSqlDecimal(k.VVT_SFACT_POSTAV_DC)}");
                        }

                    if (k.VVT_SFACT_CLIENT_DC != null)
                        using (var context = GlobalOptions.GetEntities())
                        {
                            context.Database.ExecuteSqlCommand(
                                $"EXEC dbo.GenerateSFClientCash {CustomFormat.DecimalToSqlDecimal(k.VVT_SFACT_CLIENT_DC)}");
                        }

                    BankOperationsCollection.Add(k);
                    BankOperationsCollection.First(_ => _.DOC_CODE == k.DOC_CODE && _.Code == k.Code).State =
                        RowStatus.NotEdited;
                    UpdateValueInWindow(k);
                    CurrentPeriods = Periods.FirstOrDefault(_ => _.DateStart == k.Date && _.DateEnd == k.Date);
                    CurrentBankOperations = k;
                }
            }
        }

        public override bool IsAllowHistoryShow => CurrentBankOperations != null;

        public override void ShowHistory(object data)
        {
            DocumentHistoryManager.LoadHistory(DocumentType.Bank, null, CurrentBankOperations.DocCode,
                CurrentBankOperations.Code);
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            decimal bs = 0;
            if (CurrentBankAccount != null)
                bs = CurrentBankAccount.DocCode;
            BankOperationsCollection.Clear();
            BankAccountCollection.Clear();
            BankAccountCollectionAll.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var cashAcc = ctx.Database.SqlQuery<AccessRight>(
                        $"SELECT DOC_CODE AS DocCode, USR_ID as UserId FROM HD_114 WHERE USR_ID = {GlobalOptions.UserInfo.Id}")
                    .ToList();
                var temp = new ObservableCollection<BankAccount>();
                foreach (var b in GlobalOptions.ReferencesCache.GetBankAccountAll().Cast<BankAccount>())
                    if (cashAcc.Any(_ => _.DocCode == b.DocCode))
                    {
                        var sql = "SELECT COUNT(*) FROM TD_101 " +
                                  "INNER JOIN SD_101 ON SD_101.DOC_CODE = TD_101.DOC_CODE " +
                                  // ReSharper disable once PossibleNullReferenceException
                                  $"AND VV_ACC_DC = {b.DocCode}" +
                                  $"AND VV_STOP_DATE >= '{CustomFormat.DateToString(DateTime.Today.AddDays(-180))}'";
                        var operCount = ctx.Database.SqlQuery<int>(sql);
                        b.LastYearOperationsCount = operCount.First();
                        temp.Add(b);
                    }

                foreach (var t in temp.OrderByDescending(_ => _.LastYearOperationsCount).ThenBy(_ => _.Name))
                    BankAccountCollectionAll.Add(t);
            }

            foreach (var b in BankAccountCollectionAll)
            {
                if(IsAll || !b.IsDeleted)
                    BankAccountCollection.Add(b);
            }

            RaisePropertyChanged(nameof(BankAccountCollection));
            CurrentBankAccount = BankAccountCollection.FirstOrDefault(_ => _.DocCode == bs);
            if (CurrentBankAccount != null)
                GetPeriods();
        }

        public ICommand RemoveCommand => new Command(DocDelete, _ => CurrentBankOperations != null
                                                                     && !CurrentBankOperations.IsCurrencyChange);

        public ICommand AddCurrencyChangedCommand => new Command(AddCurrencyChanged, _ => true);

        public ICommand UpdateCurrencyChangedCommand => new Command(UpdateCurrencyChanged,
            _ => CurrentBankOperations != null && CurrentBankOperations.IsCurrencyChange);

        private void UpdateCurrencyChanged(object obj)
        {
            var id = Guid.Empty;
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (CurrentBankOperations.VVT_VAL_PRIHOD > 0)
                {
                    var i = ctx.BankCurrencyChange
                        .FirstOrDefault(_ => _.DocToDC == CurrentBankOperations.DOC_CODE
                                             && _.DocRowToCode == CurrentBankOperations.Code);
                    if (i != null)
                        id = i.Id;
                }
                else
                {
                    var i = ctx.BankCurrencyChange
                        .FirstOrDefault(_ => _.DocFromDC == CurrentBankOperations.DOC_CODE
                                             && _.DocRowFromCode == CurrentBankOperations.Code);
                    if (i != null)
                        id = i.Id;
                }
            }

            var dtx = new BankCurrencyChangeWindowViewModel(id);
            var form = new BankCurrencyChangeView
            {
                Owner = Form,
                DataContext = dtx
            };
            dtx.Form = form;
            form.Show();
        }

        private void AddCurrencyChanged(object obj)
        {
            var ctx = new BankCurrencyChangeWindowViewModel(Guid.Empty)
            {
                Document =
                {
                    BankFrom = GlobalOptions.ReferencesCache.GetBankAccount(CurrentBankAccount.DocCode) as BankAccount
                },
                ParentForm = this
            };
            using (var dbctx = GlobalOptions.GetEntities())
            {
                try
                {
                    var data = dbctx.TD_101
                        .Include(_ => _.SD_101)
                        .Where(_ => _.SD_101.VV_ACC_DC == CurrentBankAccount.DocCode).Select(_ => _.VVT_CRS_DC)
                        .Distinct().ToList();
                    if (data.Count > 0)
                        ctx.Document.CurrencyFrom = GlobalOptions.ReferencesCache.GetCurrency(data.First()) as Currency;
                }
                catch (Exception e)
                {
                    WindowManager.ShowError(e);
                }
            }

            var form = new BankCurrencyChangeView
            {
                Owner = Form,
                DataContext = ctx
            };
            ctx.Form = form;
            form.Show();
        }

        public override void DocDelete(object obj)
        {
            var winManager = new WindowManager();
            var res = winManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить транзакцию?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;
            var sfPostDC = CurrentBankOperations.VVT_SFACT_POSTAV_DC;
            var sfFactDC = CurrentBankOperations.VVT_SFACT_CLIENT_DC;
            var date = CurrentBankOperations.Date;
            manager.DeleteBankOperations(CurrentBankOperations, CurrentBankAccount.DocCode);
            if (sfPostDC != null)
                using (var context = GlobalOptions.GetEntities())
                {
                    context.Database.ExecuteSqlCommand(
                        $"EXEC dbo.GenerateSFProviderCash {CustomFormat.DecimalToSqlDecimal(sfPostDC)}");
                }

            if (sfFactDC != null)
                using (var context = GlobalOptions.GetEntities())
                {
                    context.Database.ExecuteSqlCommand(
                        $"EXEC dbo.GenerateSFClientCash {CustomFormat.DecimalToSqlDecimal(sfFactDC)}");
                }

            UpdateValueInWindow(CurrentBankOperations);
            var dd = Periods.Where(_ => _.DateStart <= date && _.PeriodType == PeriodType.Day)
                .Max(_ => _.DateStart);
            var p = Periods.FirstOrDefault(_ => _.DateStart == dd && _.DateEnd == dd && _.PeriodType == PeriodType.Day);
            if (p != null)
                CurrentPeriods = p;
            switch (Parent)
            {
                case AccruedAmountOfSupplierView bw:
                {
                    if (bw.DataContext is AccruedAmountOfSupplierWindowViewModel vm)
                        vm.RefreshData(false);
                    break;
                }
            }
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentBankAccount != null)
            {
                var k = StandartDialogs.AddNewBankOperation(CurrentBankAccount.DocCode, CurrentBankOperations,
                    GlobalOptions.ReferencesCache.GetBankAccount(CurrentBankAccount.DocCode) as BankAccount);
                if (k != null)
                {
                    k.State = RowStatus.NewRow;
                    manager.SaveBankOperations(k, CurrentBankAccount.DocCode, 0);
                    BankOperationsCollection.Add(k);
                    BankOperationsCollection.First(_ => _.DOC_CODE == k.DOC_CODE && _.Code == k.Code).State =
                        RowStatus.NotEdited;
                    CurrentBankOperations = k;
                    UpdateValueInWindow(k);
                    CurrentPeriods = Periods.FirstOrDefault(_ => _.DateStart == k.Date && _.DateEnd == k.Date);
                    CurrentBankOperations = k;
                }
            }
        }

        #endregion

        #region Constructors

        public BankOperationsWindowViewModel2()
        {
        }

        public BankOperationsWindowViewModel2(Window form) : this()
        {
            Form = form;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.BankOpertionsRightBar(this);
            Periods = new List<ReminderDatePeriod>(new[]
            {
                new ReminderDatePeriod()
            });

            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #endregion

        #region Properties

        public bool IsAll
        {
            set
            {
                myIsAll = value;
                BankAccountCollection.Clear();
                BankOperationsCollection.Clear();
                BankPeriodOperationsCollection.Clear();
                foreach (var b in BankAccountCollectionAll)
                {
                    if(IsAll || !b.IsDeleted)
                        BankAccountCollection.Add(b);
                }
            }
            get => myIsAll;
        }

        public List<ReminderDatePeriod> Periods { set; get; } =
            new List<ReminderDatePeriod>();

        public ObservableCollection<BankAccount> BankAccountCollectionAll { set; get; } =
            new ObservableCollection<BankAccount>();

        public ObservableCollection<BankAccount> BankAccountCollection { set; get; } =
            new ObservableCollection<BankAccount>();

        public ObservableCollection<BankOperationsViewModel> BankOperationsCollection { set; get; } =
            new ObservableCollection<BankOperationsViewModel>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<BankPeriodsOperationsViewModel> BankPeriodOperationsCollection { set; get; } =
            new ObservableCollection<BankPeriodsOperationsViewModel>();

        public override bool IsDocNewCopyAllow =>
            CurrentBankOperations != null && !CurrentBankOperations.IsCurrencyChange;

        public override bool IsDocDeleteAllow =>
            CurrentBankOperations != null && !CurrentBankOperations.IsCurrencyChange;

        private BankAccount myCurrentBankAccount;


        public override void AddSearchList(object obj)
        {
            var form = new BankOperationsView2
            {
                Owner = Application.Current.MainWindow
            };
            form.DataContext = new BankOperationsWindowViewModel2(form);
            form.Show();
        }

        public BankAccount CurrentBankAccount
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentBankAccount == value) return;
                myCurrentBankAccount = value;
                if (myCurrentBankAccount != null)
                    //Currency =
                    //    GlobalOptions.ReferencesCache.GetBankAccount(myCurrentBankAccount.DocCode).Currency as Currency;
                    GetPeriods();

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Periods));
            }
            get => myCurrentBankAccount;
        }

        private ReminderDatePeriod myCurrentPeriods;

        public ReminderDatePeriod CurrentPeriods
        {
            set
            {
                if (Equals(myCurrentPeriods, value)) return;
                myCurrentPeriods = value;
                if (myCurrentPeriods != null)
                    GetBankOperation();
                RaisePropertyChanged();
            }
            get => myCurrentPeriods;
        }

        private BankOperationsViewModel myCurrentBankOperations;

        public BankOperationsViewModel CurrentBankOperations
        {
            set
            {
                if (Equals(myCurrentBankOperations, value)) return;
                if (myCurrentBankOperations != null && myCurrentBankOperations.State == RowStatus.Edited
                                                    && CurrentBankAccount != null &&
                                                    myCurrentBankOperations.IsCurrencyChange == false)
                {
                    var delta = Convert.ToDecimal(myCurrentBankOperations.DeltaPrihod -
                                                  myCurrentBankOperations.DeltaRashod);
                    manager.SaveBankOperations(myCurrentBankOperations, CurrentBankAccount.DocCode, delta);
                    myCurrentBankOperations.State = RowStatus.NotEdited;
                }

                myCurrentBankOperations = value;
                RaisePropertyChanged();
            }
            get => myCurrentBankOperations;
        }

        private Currency myCurrency;
        private bool myIsAll;

        public Currency Currency
        {
            get => myCurrency;
            set
            {
                if (myCurrency == value) return;
                myCurrency = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        private void GetPeriods()
        {
            Periods.Clear();
            BankOperationsCollection.Clear();
            BankPeriodOperationsCollection.Clear();
            if (CurrentBankAccount == null) return;
            //foreach (var per in
            //    Manager.GetBankPeriodOperations(CurrentBankAccount.DocCode))
            //    BankPeriodOperationsCollection.Add(per);
            Periods.AddRange(manager.GetRemains2(CurrentBankAccount.DocCode));
            RaisePropertyChanged(nameof(Periods));
            if (Form is BankOperationsView2 form) form.TreePeriods.RefreshData();
        }

        private void GetBankOperation()
        {
            if (CurrentBankAccount?.DocCode == null) return;
            BankOperationsCollection.Clear();
            var opers = manager.GetBankOperations(CurrentPeriods.DateStart, CurrentPeriods.DateEnd,
                CurrentBankAccount.DocCode);
            foreach (var op in opers)
            {
                op.State = RowStatus.NotEdited;
                BankOperationsCollection.Add(op);
            }
        }

        public void UpdateValueInWindow(BankOperationsViewModel k)
        {
            var bankDc = CurrentBankAccount.DocCode;
            RefreshData(null);
            CurrentBankAccount = BankAccountCollection.FirstOrDefault(_ => _.DocCode == bankDc);
            CurrentPeriods = Periods.FirstOrDefault(_ => _.DateStart <= k.Date && _.DateEnd >= k.Date);
            RaisePropertyChanged(nameof(Periods));
        }

        #endregion
    }
}
