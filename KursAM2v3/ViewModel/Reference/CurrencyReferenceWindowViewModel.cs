﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using KursAM2.Repositories;
using KursAM2.View.KursReferences.UC;

namespace KursAM2.ViewModel.Reference
{
    public sealed class CurrencyReferenceWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private bool isCurrencyDeleted;

        public readonly ICurrencyRepozitories CurrencyRepository;

        public readonly GenericKursDBRepository<SD_301> BaseCurrencyRepository;

        // ReSharper disable once InconsistentNaming
        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private CurrencyRef myCurrentCurrency;

        #endregion

        #region Constructors

        public CurrencyReferenceWindowViewModel()
        {
            BaseCurrencyRepository = new GenericKursDBRepository<SD_301>(unitOfWork);
            CurrencyRepository = new CurrencyRepozitories(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartReestrRightBar(this);
            RefreshData(null);
        }

        #endregion

        #region Properties

        public override string WindowName => "Справочник валют";
        public override string LayoutName => "CurrencyReferenceWindowViewModel";

        public CurrencyRef CurrentCurrency
        {
            get => myCurrentCurrency;
            set
            {
                if (myCurrentCurrency == value) return;
                myCurrentCurrency = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<CurrencyRef> CurrencyCollection { set; get; } = new ObservableCollection<CurrencyRef>();

        public CurrencyReferenceUC DocumentUserControl { set; get; } = new CurrencyReferenceUC();

        #endregion

        #region Commands

        public ICommand RemoveItemCommand
        {
            get { return new Command(RemoveItem, _ => CurrentCurrency != null); }
        }

        private void RemoveItem(object obj)
        {
            var service = GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Уверены, что хотите удалить?";
            var res = service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this);
            switch (res)
            {
                case MessageResult.Yes:
                    unitOfWork.Context.SD_301.Remove(CurrentCurrency.Entity);
                    CurrencyCollection.Remove(CurrentCurrency);
                    isCurrencyDeleted = true;
                    return;
            }
        }

        public ICommand AddNewItemCommand
        {
            get { return new Command(AddNewItem, _ => true); }
        }

        private void AddNewItem(object obj)
        {
            var newItem = new CurrencyRef(new SD_301()
            {
                DOC_CODE = -1,
                Id = Guid.NewGuid()
            })
            {
                myState = RowStatus.NewRow
            };
            unitOfWork.Context.SD_301.Add(newItem.Entity);
            CurrencyCollection.Add(newItem);
        }

        public override bool IsCanSaveData =>
            CurrencyCollection.Any(_ => _.State != RowStatus.NotEdited) || isCurrencyDeleted;

        public override void RefreshData(object obj) {
        CurrencyCollection.Clear();
            foreach (var c in CurrencyRepository.GetAllCurrencies())
            {
                CurrencyCollection.Add(c);
            }
            isCurrencyDeleted = false;
        }

        public override void SaveData(object data)
        {
            unitOfWork.CreateTransaction();
            try
            {
                unitOfWork.Save();
                unitOfWork.Commit();
                foreach (var c in CurrencyCollection)
                {
                    c.myState = RowStatus.NotEdited;
                }
                isCurrencyDeleted = false;
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                WindowManager.ShowError(ex);
            }
        }

        #endregion

        #region Methods

        #endregion

        #region IDataErrorInfo

        #endregion
    }
}