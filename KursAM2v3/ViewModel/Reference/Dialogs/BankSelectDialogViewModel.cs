﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using KursAM2.View.Base;
using KursDomain;
using KursDomain.Documents.Bank;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference.Dialogs
{
    public sealed class BankSelectDialogViewModel : RSWindowViewModelBase
    {
        #region Fields

        private Bank myCurrentItem;

        #endregion

        #region Constructors

        public BankSelectDialogViewModel()
        {
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор банка";
            RefreshData(null);
        }

        #endregion

        #region Properties

        private ICurrentWindowService winCurrentService;
        public UserControl DataUserControl { set; get; } = new GridControlUC();
        public override string LayoutName => "BankSelectDialogViewModel";

        public ObservableCollection<Bank> Items { set; get; } = new ObservableCollection<Bank>();

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
            Items.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var b in ctx.SD_44.ToList())
                {
                    var bank = new Bank();
                    bank.LoadFromEntity(b);
                    Items.Add(bank);
                }
            }
        }

        public override bool IsOkAllow()
        {
            return CurrentItem != null;
        }

        public override void Ok(object obj)
        {
            winCurrentService = this.GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                DialogResult = true;
                winCurrentService.Close();
            }
        }

        public override void Cancel(object obj)
        {
            winCurrentService = this.GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                CurrentItem = null;
                winCurrentService.Close();
                DialogResult = false;
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

        public override void DocNewEmpty(object form)
        {
            var bankref = new BankReferenceWindowViewModel();
            var formbank = new KursStandartFormView
            {
                Owner = Application.Current.MainWindow,
                DataContext = bankref
            };
            formbank.Show();
            if (winCurrentService == null) return;
            DialogResult = true;
            winCurrentService.Close();
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
                    Items.Remove(CurrentItem);
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
                DocCode = Items.Max(_ => _.DocCode) + 1,
                Name = "Новый",
                CorrAccount = " ",
                BIK = " "
            };
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    ctx.SD_44.Add(new SD_44
                    {
                        DOC_CODE = newBank.DocCode,
                        BANK_NAME = newBank.Name,
                        CORRESP_ACC = newBank.CorrAccount,
                        ADDRESS = newBank.Address,
                        POST_CODE = newBank.BIK
                    });
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(Application.Current.MainWindow, ex);
                }
            }

            Items.Add(newBank);
            CurrentItem = newBank;
        }

        public override void Search(object obj)
        {
            Items.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var b in ctx.SD_44.Where(_ => _.ADDRESS.ToUpper().Contains(SearchText.ToUpper()) ||
                                                       _.BANK_NAME.ToUpper().Contains(SearchText.ToUpper())
                                                       || _.BANK_NICKNAME.ToUpper().Contains(SearchText.ToUpper())
                                                       || _.CORRESP_ACC.ToUpper().Contains(SearchText.ToUpper())
                                                       || _.POST_CODE.ToUpper().Contains(SearchText.ToUpper()))
                             .ToList())
                {
                    var bank = new Bank();
                    bank.LoadFromEntity(b);
                    Items.Add(bank);
                }
            }
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
            RefreshData(null);
        }

        #endregion
    }
}
