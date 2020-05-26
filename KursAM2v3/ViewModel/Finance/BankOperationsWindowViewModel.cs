using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Finance;

namespace KursAM2.ViewModel.Finance
{
    public class BankOperationsWindowViewModel : RSWindowViewModelBase
    {
        public BankOperationsWindowViewModel()
        {
        }

        public BankOperationsWindowViewModel(Window form) : this()
        {
            Form = form;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.BankOpertionsRightBar(this);
            Periods = new ObservableCollection<RemainderCurrenciesDatePeriod>(new[]
            {
                new RemainderCurrenciesDatePeriod()
            });

            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #region

        public ObservableCollection<BankStatements> BankAccountCollection { set; get; } =
            new ObservableCollection<BankStatements>();

        public ObservableCollection<BankOperationsViewModel> BankOperationsCollection { set; get; } =
            new ObservableCollection<BankOperationsViewModel>();

        public ObservableCollection<BankPeriodsOperationsViewModel> BankPeriodOperationsCollection { set; get; } =
            new ObservableCollection<BankPeriodsOperationsViewModel>();

        public ObservableCollection<RemainderCurrenciesDatePeriod> Periods { set; get; } =
            new ObservableCollection<RemainderCurrenciesDatePeriod>();

        private readonly BankOperationsManager Manager = new BankOperationsManager();

        #endregion

        #region compendiums

        public List<Currency> CurrencysCompendium => MainReferences.Currencies.Values.ToList();
        public List<SDRSchet> SHPZList => MainReferences.SDRSchets.Values.ToList();

        #endregion

        #region

        private RemainderCurrenciesDatePeriod myCurrentPeriods;

        public RemainderCurrenciesDatePeriod CurrentPeriods
        {
            set
            {
                if (myCurrentPeriods != null && myCurrentPeriods.Equals(value)) return;
                myCurrentPeriods = value;
                if (myCurrentPeriods != null)
                    GetBankOperation();
                RaisePropertyChanged();
            }
            get => myCurrentPeriods;
        }

        private BankStatements myCurrentBankAccount;

        public BankStatements CurrentBankAccount
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentBankAccount == value) return;
                myCurrentBankAccount = value;
                if (myCurrentBankAccount != null) GetPeriods();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Periods));
            }
            get => myCurrentBankAccount;
        }

        private BankOperationsViewModel myCurrentBankOperations;

        public BankOperationsViewModel CurrentBankOperations
        {
            set
            {
                if (myCurrentBankOperations != null && myCurrentBankOperations.Equals(value)) return;
                if (myCurrentBankOperations != null && myCurrentBankOperations.State == RowStatus.Edited
                                                    && CurrentBankAccount != null && myCurrentBankOperations.IsCurrencyChange == false)
                {
                    var delta = Convert.ToDecimal(myCurrentBankOperations.DeltaPrihod -
                                                  myCurrentBankOperations.DeltaRashod);
                    Manager.SaveBankOperations(myCurrentBankOperations, CurrentBankAccount.BankDC, delta);
                    myCurrentBankOperations.State = RowStatus.NotEdited;
                }

                myCurrentBankOperations = value;
                RaisePropertyChanged();
            }
            get => myCurrentBankOperations;
        }

        #endregion

        #region Command

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            decimal bs = 0;
            if (CurrentBankAccount != null)
                bs = CurrentBankAccount.DocCode;
            BankOperationsCollection.Clear();
            BankAccountCollection = new ObservableCollection<BankStatements>(Manager.GetBankStatements());
            RaisePropertiesChanged(nameof(BankAccountCollection));
            CurrentBankAccount = BankAccountCollection.FirstOrDefault(_ => _.DocCode == bs);
            if (CurrentBankAccount != null)
                GetPeriods();
            GetAllVisibleBands();
        }

        public override bool IsCanSaveData =>
            BankOperationsCollection.Any(_ => _.State == RowStatus.NewRow || _.State == RowStatus.Edited);

        public override bool IsDocNewEmptyAllow => CurrentBankAccount != null;

        public override void DocNewEmpty(object form)
        {
            if (CurrentBankAccount != null)
            {
                var k = StandartDialogs.AddNewBankOperation(CurrentBankAccount.DocCode, new BankOperationsViewModel(),
                    MainReferences.BankAccounts[CurrentBankAccount.BankDC]);
                if (k != null)
                {
                    k.State = RowStatus.NewRow;
                    Manager.SaveBankOperations(k, CurrentBankAccount.BankDC, 0);
                    BankOperationsCollection.Add(k);
                    BankOperationsCollection.First(_ => _.DOC_CODE == k.DOC_CODE && _.Code == k.Code).State =
                        RowStatus.NotEdited;
                    UpdateValueInWindow(k);
                    CurrentPeriods = Periods.FirstOrDefault(_ => _.DateStart == k.Date && _.DateEnd == k.Date);
                    CurrentBankOperations = k;
                }

                GetAllVisibleBands();
            }
        }

        private void UpdateValueInWindow(BankOperationsViewModel k)
        {
            var bankDc = CurrentBankAccount.BankDC;
            RefreshData(null);
            CurrentBankAccount = BankAccountCollection.FirstOrDefault(_ => _.BankDC == bankDc);
            CurrentPeriods = Periods.FirstOrDefault(_ => _.DateStart <= k.Date && _.DateEnd >= k.Date);
            RaisePropertyChanged(nameof(Periods));
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentBankAccount != null)
            {
                var k = StandartDialogs.AddNewBankOperation(CurrentBankAccount.DocCode, CurrentBankOperations,
                    MainReferences.BankAccounts[CurrentBankAccount.BankDC]);
                if (k != null)
                {
                    k.State = RowStatus.NewRow;
                    Manager.SaveBankOperations(k, CurrentBankAccount.BankDC, 0);
                    BankOperationsCollection.Add(k);
                    BankOperationsCollection.First(_ => _.DOC_CODE == k.DOC_CODE && _.Code == k.Code).State =
                        RowStatus.NotEdited;
                    CurrentBankOperations = k;
                    UpdateValueInWindow(k);
                    CurrentPeriods = Periods.FirstOrDefault(_ => _.DateStart == k.Date && _.DateEnd == k.Date);
                    CurrentBankOperations = k;
                }

                GetAllVisibleBands();
            }
        }

        public override bool IsDocNewCopyAllow => CurrentBankOperations != null && !CurrentBankOperations.IsCurrencyChange;
        public override bool IsDocDeleteAllow => CurrentBankOperations != null && !CurrentBankOperations.IsCurrencyChange;

        private void GetBankOperation()
        {
            if (CurrentBankAccount?.BankDC == null) return;
            BankOperationsCollection = Manager.GetBankOperations(CurrentPeriods.DateStart, CurrentPeriods.DateEnd,
                CurrentBankAccount.BankDC);
            foreach (var i in BankOperationsCollection) i.State = RowStatus.NotEdited;
            RaisePropertyChanged(nameof(BankOperationsCollection));
        }

        private void GetAllVisibleBands()
        {
            //SetVisibleBankStatements();
            if (CurrentBankAccount == null) return;
            SetVisiblePeriodsBands();
        }

        public ICommand RemoveCommand => new Command(DocDelete, _ => CurrentBankOperations != null
                                                                     && !CurrentBankOperations.IsCurrencyChange);

        public ICommand AddCurrencyChangedCommand => new Command(AddCurrencyChanged, _ => true);

        public ICommand UpdateCurrencyChangedCommand => new Command(UpdateCurrencyChanged, _ => CurrentBankOperations != null && CurrentBankOperations.IsCurrencyChange);

        private void UpdateCurrencyChanged(object obj)
        {
            Guid id = Guid.Empty;
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
            var form = new BankCurrencyChangeView()
            {
                Owner = Application.Current.MainWindow,
                DataContext = dtx
            };
            dtx.Form = form;
            form.Show();
        }

        private void AddCurrencyChanged(object obj)
        {
            var ctx = new BankCurrencyChangeWindowViewModel(Guid.Empty)
            {
                Document = {BankFrom = MainReferences.BankAccounts[CurrentBankAccount.BankDC]}
            };
            using (var dbctx = GlobalOptions.GetEntities())
            {
                try
                {
                    var data = dbctx.TD_101
                        .Include(_ => _.SD_101)
                        .Where(_ => _.SD_101.VV_ACC_DC == CurrentBankAccount.BankDC).Select(_ => _.VVT_CRS_DC).Distinct().ToList();
                    if (data.Count > 0)
                    {
                        ctx.Document.CurrencyFrom = MainReferences.Currencies[data.First()];
                    }
                }
                catch (Exception e)
                {
                    WindowManager.ShowError(e);
                }
            }
            var form = new BankCurrencyChangeView()
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = form;
            form.Show();
        }

        public override void DocDelete(object obj)
        {
            var date = CurrentBankOperations.Date;
            Manager.DeleteBankOperations(CurrentBankOperations, CurrentBankAccount.BankDC);
            UpdateValueInWindow(CurrentBankOperations);
            var dd = Periods.Where(_ => _.DateStart <= date && _.PeriodType == PeriodType.Day)
                .Max(_ => _.DateStart);
            var p = Periods.FirstOrDefault(_ => _.DateStart == dd && _.DateEnd == dd && _.PeriodType == PeriodType.Day);
            if (p != null)
                CurrentPeriods = p;
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void SetVisiblePeriodsBands()
        {
            var treePeriods = ((BankOperationsView) Form).TreePeriods;
            if (treePeriods.Bands.Count == 0) return;
            if (Periods.Sum(_ => _.SummaEndGBP) + Periods.Sum(_ => _.SummaStartGBP) + Periods.Sum(_ => _.SummaInGBP)
                + Periods.Sum(_ => _.SummaOutGBP) == 0)
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "GBP").Visible = false;
            else
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "GBP").Visible = true;
            if (Periods.Sum(_ => _.SummaEndRUB) + Periods.Sum(_ => _.SummaStartRUB) + Periods.Sum(_ => _.SummaInRUB)
                + Periods.Sum(_ => _.SummaOutRUB) == 0)
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "RUB").Visible = false;
            else
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "RUB").Visible = true;
            if (Periods.Sum(_ => _.SummaEndUSD) + Periods.Sum(_ => _.SummaStartUSD) + Periods.Sum(_ => _.SummaInUSD)
                + Periods.Sum(_ => _.SummaOutUSD) == 0)
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "USD").Visible = false;
            else
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "USD").Visible = true;
            if (Periods.Sum(_ => _.SummaEndSEK) + Periods.Sum(_ => _.SummaStartSEK) + Periods.Sum(_ => _.SummaInSEK)
                + Periods.Sum(_ => _.SummaOutSEK) == 0)
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "SEK").Visible = false;
            else
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "SEK").Visible = true;
            if (Periods.Sum(_ => _.SummaEndEUR) + Periods.Sum(_ => _.SummaStartEUR) + Periods.Sum(_ => _.SummaInEUR)
                + Periods.Sum(_ => _.SummaOutEUR) == 0)
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "EUR").Visible = false;
            else
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "EUR").Visible = true;
            if (Periods.Sum(_ => _.SummaEndCHF) + Periods.Sum(_ => _.SummaStartCHF) + Periods.Sum(_ => _.SummaInCHF)
                + Periods.Sum(_ => _.SummaOutCHF) == 0)
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "CHF").Visible = false;
            else
                treePeriods.Bands.FirstOrDefault(_ => (string) _.Header == "CHF").Visible = true;
        }
         
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void GetPeriods()
        {
            Periods.Clear();
            BankOperationsCollection.Clear();
            BankPeriodOperationsCollection.Clear();
            if (CurrentBankAccount == null) return;

            BankPeriodOperationsCollection = Manager.GetBankPeriodOperations(CurrentBankAccount.BankDC);
            var p = Manager.GetRemains(
                CurrentBankAccount.BankDC,
                BankPeriodOperationsCollection?.Min(_ => _.Date).AddDays(-1) ?? DateTime.MinValue,
                BankPeriodOperationsCollection?.Max(_ => _.Date) ?? DateTime.MaxValue);
            if (p != null && p.Count > 0)
            {
                Periods = new ObservableCollection<RemainderCurrenciesDatePeriod>(Manager.GetRemains(
                    CurrentBankAccount.BankDC,
                    BankPeriodOperationsCollection?.Min(_ => _.Date).AddDays(-1) ?? DateTime.MinValue,
                    BankPeriodOperationsCollection?.Max(_ => _.Date) ?? DateTime.MaxValue));
            }
            else
            {
                Periods = new ObservableCollection<RemainderCurrenciesDatePeriod>();
            }

            RaisePropertyChanged(nameof(Periods));
            SetVisiblePeriodsBands();
        }

        #endregion
    }
}