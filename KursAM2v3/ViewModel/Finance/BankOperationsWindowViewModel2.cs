using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.Bank;
using Core.EntityViewModel.CommonReferences;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Finance;
using KursAM2.View.Helper;

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

        public override void DocumentOpen(object obj)
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
                var k = StandartDialogs.OpenBankOperation(CurrentBankAccount.DocCode,
                    CurrentBankOperations,
                    MainReferences.BankAccounts[CurrentBankAccount.DocCode]);
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
                var k = StandartDialogs.AddNewBankOperation(CurrentBankAccount.DocCode, new BankOperationsViewModel(),
                    MainReferences.BankAccounts[CurrentBankAccount.DocCode]);
                if (k != null)
                {
                    k.State = RowStatus.NewRow;
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

        public override bool IsAllowHistoryShow => CurrentBankOperations != null;

        public override void ShowHistory(object data)
        {
            DocumentHistoryManager.LoadHistory(DocumentType.Bank,null,CurrentBankOperations.DocCode, CurrentBankOperations.Code);
        }
        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            decimal bs = 0;
            if (CurrentBankAccount != null)
                bs = CurrentBankAccount.DocCode;
            BankOperationsCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var cashAcc = ctx.Database.SqlQuery<MainReferences.AccessRight>(
                        $"SELECT DOC_CODE AS DocCode, USR_ID as UserId FROM HD_114 WHERE USR_ID = {GlobalOptions.UserInfo.Id}")
                    .ToList();
                foreach (var b in MainReferences.BankAccounts.Values)
                {
                    if(cashAcc.Any(_ => _.DocCode == b.DocCode))
                        BankAccountCollection.Add(b);
                }
            }
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
                    BankFrom = MainReferences.BankAccounts[CurrentBankAccount.DocCode]
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
                    if (data.Count > 0) ctx.Document.CurrencyFrom = MainReferences.Currencies[data.First()];
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
            var date = CurrentBankOperations.Date;
            manager.DeleteBankOperations(CurrentBankOperations, CurrentBankAccount.DocCode);
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
                    MainReferences.BankAccounts[CurrentBankAccount.DocCode]);
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
        public BankAccount CurrentBankAccount
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentBankAccount == value) return;
                myCurrentBankAccount = value;
                if (myCurrentBankAccount != null)
                {
                    Currency = MainReferences.BankAccounts[myCurrentBankAccount.DocCode].Currency;
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
                    manager.SaveBankOperations(myCurrentBankOperations, CurrentBankAccount.DocCode, delta);
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