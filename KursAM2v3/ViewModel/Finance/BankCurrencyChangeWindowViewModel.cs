using System;
using System.Windows;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using KursAM2.Managers;

namespace KursAM2.ViewModel.Finance
{
    public class BankCurrencyChangeWindowViewModel : RSWindowViewModelBase
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
                                            && Document.CurrencyTo != null && Document.CurrencyFrom != null
                                            && Document.CrsToDC != Document.CrsFromDC;

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;
       
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

        #endregion

        #region Commands

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
        }

        public override void DocDelete(object form)
        {
            if (manager.DeleteBankCurrencyChange(Document))
                Form.Close();
        }

        #endregion
    }
}