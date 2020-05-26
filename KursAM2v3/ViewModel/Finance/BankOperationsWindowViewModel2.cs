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
using Core.WindowsManager;
using DevExpress.XtraSpreadsheet.Model.History;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Finance;

namespace KursAM2.ViewModel.Finance
{
    public class BankOperationsWindowViewModel2 : RSWindowViewModelBase
    {
        #region Fields

        private readonly BankOperationsManager Manager = new BankOperationsManager();

        #endregion

        #region Commands

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
                        .Where(_ => _.SD_101.VV_ACC_DC == CurrentBankAccount.BankDC).Select(_ => _.VVT_CRS_DC)
                        .Distinct().ToList();
                    if (data.Count > 0) ctx.Document.CurrencyFrom = MainReferences.Currencies[data.First()];
                }
                catch (Exception e)
                {
                    WindowManager.ShowError(e);
                }
            }
            var form = new BankCurrencyChangeView
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
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
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

        public List<ReminderDatePeriod> Periods { set; get; } =
            new List<ReminderDatePeriod>();
        public ObservableCollection<BankStatements> BankAccountCollection { set; get; } =
            new ObservableCollection<BankStatements>();
        public ObservableCollection<BankOperationsViewModel> BankOperationsCollection { set; get; } =
            new ObservableCollection<BankOperationsViewModel>();
        public ObservableCollection<BankPeriodsOperationsViewModel> BankPeriodOperationsCollection { set; get; } =
            new ObservableCollection<BankPeriodsOperationsViewModel>();
        public override bool IsDocNewCopyAllow =>
            CurrentBankOperations != null && !CurrentBankOperations.IsCurrencyChange;
        public override bool IsDocDeleteAllow =>
            CurrentBankOperations != null && !CurrentBankOperations.IsCurrencyChange;
        private BankStatements myCurrentBankAccount;
        public BankStatements CurrentBankAccount
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentBankAccount == value) return;
                myCurrentBankAccount = value;
                if (myCurrentBankAccount != null)
                {
                    Currency = MainReferences.BankAccounts[myCurrentBankAccount.BankDC].Currency;
                    GetPeriods();
                }
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
                if (myCurrentPeriods != null && myCurrentPeriods.Equals(value)) return;
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
                if (myCurrentBankOperations != null && myCurrentBankOperations.Equals(value)) return;
                if (myCurrentBankOperations != null && myCurrentBankOperations.State == RowStatus.Edited
                                                    && CurrentBankAccount != null &&
                                                    myCurrentBankOperations.IsCurrencyChange == false)
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
        private Currency myCurrency;
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
            foreach (var per in
                Manager.GetBankPeriodOperations(CurrentBankAccount.BankDC))
                BankPeriodOperationsCollection.Add(per);
            Periods.AddRange(Manager.GetRemains2(CurrentBankAccount.BankDC));
            RaisePropertyChanged(nameof(Periods));
            if (Form is BankOperationsView2 form) form.TreePeriods.RefreshData();
        }

        private void GetBankOperation()
        {
            if (CurrentBankAccount?.BankDC == null) return;
            BankOperationsCollection.Clear();
            var opers = Manager.GetBankOperations(CurrentPeriods.DateStart, CurrentPeriods.DateEnd,
                CurrentBankAccount.BankDC);
            foreach (var op in opers)
            {
                op.State = RowStatus.NotEdited;
                BankOperationsCollection.Add(op);
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
        /*<MenuItem Header="Открыть" Command="{Binding DocOpenCommand}"/>
                                    <MenuItem Header="Добавить">
                                        <MenuItem Header="Добавить приход/расход"
                                                  Command="{Binding DocNewEmptyCommand}"/>
                                        <MenuItem Header="Добавить обмен валюты"
                                                  Command="{Binding AddCurrencyChangedCommand}" />
                                    </MenuItem>
                                    <MenuItem Header="Изменить документ"
                                              Command="{Binding UpdateCurrencyChangedCommand}" />
                                    <MenuItem Header="Копировать"
                                              Command="{Binding DocNewCopyCommand}" />
                                    <MenuItem Header="Удалить"
                                              Command="{Binding RemoveCommand}" />*/

        #endregion
    }
}