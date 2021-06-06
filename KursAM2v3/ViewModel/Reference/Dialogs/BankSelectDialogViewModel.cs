using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.Bank;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.View.DialogUserControl;

namespace KursAM2.ViewModel.Reference.Dialogs
{
    public class BankSelectDialogViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public BankSelectDialogViewModel()
        {
            myDataUserControl = new StandartDialogSelectUC2(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор банка";
            ItemsCollection = new ObservableCollection<Bank>();
            RefreshData(null);
        }

        #endregion

        #region Fields

        private StandartDialogSelectUC2 myDataUserControl;
        private Bank myCurrentItem;

        #endregion

        #region Properties

        public StandartDialogSelectUC2 DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Bank> ItemsCollection { set; get; }

        public Bank CurrentItem
        {
            get => myCurrentItem;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentItem == value) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            ItemsCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var b in ctx.SD_44.ToList()) ItemsCollection.Add(new Bank(b));
            }
        }

        public ICommand AddNewBankCommand
        {
            get { return new Command(AddNewBank, _ => true); }
        }

        public ICommand DeleteBankCommand
        {
            get { return new Command(DeleteBank, _ => CurrentItem != null); }
        }

        private void DeleteBank(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    var b = ctx.SD_44.FirstOrDefault(_ => _.DOC_CODE == CurrentItem.DocCode);
                    if (b == null) return;
                    ctx.SD_44.Remove(b);
                    ctx.SaveChanges();
                    ItemsCollection.Remove(CurrentItem);
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(Application.Current.MainWindow, ex);
                }
            }
        }

        private void AddNewBank(object obj)
        {
            var newBank = new Bank
            {
                DocCode = ItemsCollection.Max(_ => _.DocCode) + 1,
                Name = "Новый",
                CORRESP_ACC = " ",
                POST_CODE = " "
            };
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    ctx.SD_44.Add(new SD_44
                    {
                        DOC_CODE = newBank.DocCode,
                        BANK_NAME = newBank.Name,
                        CORRESP_ACC = newBank.CORRESP_ACC,
                        ADDRESS = newBank.ADDRESS,
                        POST_CODE = newBank.POST_CODE
                    });
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(Application.Current.MainWindow, ex);
                }
            }

            ItemsCollection.Add(newBank);
            CurrentItem = newBank;
        }

        public override void Search(object obj)
        {
            ItemsCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var b in ctx.SD_44.Where(_ => _.ADDRESS.ToUpper().Contains(SearchText.ToUpper()) ||
                                                       _.BANK_NAME.ToUpper().Contains(SearchText.ToUpper())
                                                       || _.BANK_NICKNAME.ToUpper().Contains(SearchText.ToUpper())
                                                       || _.CORRESP_ACC.ToUpper().Contains(SearchText.ToUpper())
                                                       || _.POST_CODE.ToUpper().Contains(SearchText.ToUpper()))
                    .ToList())
                    ItemsCollection.Add(new Bank(b));
            }
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
            RefreshData(null);
        }

        public override void Cancel(object obj)
        {
            CurrentItem = null;
            base.Cancel(obj);
        }

        #endregion
    }
}