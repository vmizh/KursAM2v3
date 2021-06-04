using System;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.Bank;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.Managers;
using KursAM2.View.Finance;

namespace KursAM2.ViewModel.Finance
{
    public sealed class BankCurrencyChangeWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private BankCurrencyChangeViewModel myDocument;
        private readonly BankOperationsManager manager = new BankOperationsManager();
        private DateTime oldDate;

        #endregion

        #region Constructors

        public BankCurrencyChangeWindowViewModel()
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = false;
            IsDocDeleteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
        }

        public BankCurrencyChangeWindowViewModel(Guid id) : this()
        {
            Id = id;
            RefreshData(null);
        }

        #endregion

        #region Properties

        public override bool IsCanSaveData => Document != null && Document.State != RowStatus.NotEdited
                                                               && Document.CurrencyTo != null &&
                                                               Document.CurrencyFrom != null
                                                               && Document.CrsToDC != Document.CrsFromDC;
        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsDocNewCopyAllow => Document != null && Document.State != RowStatus.NewRow;
        public BankCurrencyChangeViewModel Document
        {
            get => myDocument;
            set
            {
                if (myDocument == value) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }
        public BankOperationsWindowViewModel2 ParentForm { set; get; }
        #endregion

        #region Commands

        public override void DocNewCopy(object form)
        {
            var ctx = new BankCurrencyChangeWindowViewModel(Guid.Empty)
            {
                Document = manager.NewCopyBankCurrencyChange(Document)
            };
            var frm = new BankCurrencyChangeView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (Id == Guid.Empty)
            {
                Document = manager.NewBankCurrencyChange();
                Id = Document.Id;
                return;
            }
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
            if (Document?.Id != Guid.Empty)
            {
                Document = manager.LoadBankCurrencyChange(Id);
                if (Document == null)
                {
                    WinManager.ShowWinUIMessageBox($"Не найден документ с кодом {Id}!",
                        "Ошибка обращения к базе данных", MessageBoxButton.OK, MessageBoxImage.Error,
                        MessageBoxResult.None, MessageBoxOptions.None);
                    return;
                }
                oldDate = Document.DocDate;
            }
            RaisePropertyChanged(nameof(Document));
            Document.myState = RowStatus.NotEdited;
        }

        public override void SaveData(object data)
        {
            if (Document.State == RowStatus.NewRow)
            {
                manager.AddBankCurrencyChange(Document);
            }
            else
            {
                if (Document.DocDate == oldDate)
                {
                    manager.SaveBankCurrencyChange(Document);
                }
                else
                {
                    manager.DeleteBankCurrencyChange(Document);
                    manager.AddBankCurrencyChange(Document);
                }
            }
            if (ParentForm != null)
            {
                var date = ParentForm.CurrentBankOperations.Date;
                ParentForm.UpdateValueInWindow(ParentForm.CurrentBankOperations);
                var dd = ParentForm.Periods.Where(_ => _.DateStart <= date 
                                                       && _.PeriodType == PeriodType.Day)
                    .Max(_ => _.DateStart);
                var p = ParentForm.Periods.FirstOrDefault(_ => _.DateStart == dd 
                                                               && _.DateEnd == dd 
                                                               && _.PeriodType == PeriodType.Day);
                if (p != null)
                    ParentForm.CurrentPeriods = p;
            }
        }

        public override void DocDelete(object form)
        {
            if (manager.DeleteBankCurrencyChange(Document))
                Form.Close();
        }

        #endregion
    }
}