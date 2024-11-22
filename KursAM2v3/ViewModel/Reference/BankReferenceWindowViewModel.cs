using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using KursAM2.Repositories;
using KursAM2.View.KursReferences.UC;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository;

namespace KursAM2.ViewModel.Reference
{
    public sealed class BankReferenceWindowViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public BankReferenceWindowViewModel()
        {
            //BaseBankRepository = new GenericKursDBRepository<SD_301>(unitOfWork);
            BankRepository = new BankRepozitories(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartReestrRightBar(this);
            RefreshData(null);
        }

        #endregion

        #region Fields

        private bool isBankDeleted;

        public readonly IBankRepozitories BankRepository;

        //public readonly GenericKursDBRepository<SD_301> BaseBankRepository;

        // ReSharper disable once InconsistentNaming
        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private BankViewModel myBankBank;

        #endregion

        #region Properties

        public override string WindowName => "Справочник банков";
        public override string LayoutName => "BankReferenceWindowViewModel";

        public BankViewModel CurrentBank
        {
            get => myBankBank;
            set
            {
                if (myBankBank == value) return;
                myBankBank = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<BankViewModel> BankCollection { set; get; } =
            new ObservableCollection<BankViewModel>();

        public ObservableCollection<BankViewModel> SelectedBanks { set; get; } =
            new ObservableCollection<BankViewModel>();

        
        public BankReferenceUC DocumentUserControl { set; get; } = new BankReferenceUC();

        #endregion

        #region Commands

        public ICommand RemoveItemCommand
        {
            get { return new Command(RemoveItem, _ => CurrentBank != null && !CurrentBank.IsUsed); }
        }

        private void RemoveItem(object obj)
        {
            var service = this.GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Уверены, что хотите удалить?";
            var res = service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this);
            switch (res)
            {
                case MessageResult.Yes:
                    unitOfWork.Context.SD_44.Remove(CurrentBank.Entity);
                    BankCollection.Remove(CurrentBank);
                    isBankDeleted = true;
                    return;
            }
        }

        public ICommand AddNewItemCommand
        {
            get { return new Command(AddNewItem, _ => true); }
        }

        private void AddNewItem(object obj)
        {

            var newItem = new BankViewModel(new SD_44
            {
                DOC_CODE = BankCollection.Any() ? BankCollection.Max(_ => _.DocCode) +1 : 440000001,
                });
            unitOfWork.Context.SD_44.Add(newItem.Entity);
            BankCollection.Add(newItem);
            CurrentBank = newItem;
        }

        public override bool IsCanSaveData =>
            BankCollection.Any(_ => _.State != RowStatus.NotEdited) || isBankDeleted;

        public override void RefreshData(object obj)
        {
            BankCollection.Clear();
            foreach (var c in BankRepository.GetAllBanks())
            {
                c.State = RowStatus.NotEdited;
                BankCollection.Add(c);
            }

            isBankDeleted = false;
        }

        public override void SaveData(object data)
        {
            unitOfWork.CreateTransaction();
            try
            {
                unitOfWork.Save();
                unitOfWork.Commit();
                foreach (var b in BankCollection.Where(_ => _.State != RowStatus.NotEdited))
                {
                    if (string.IsNullOrEmpty(b.Name))
                    {
                        unitOfWork.Context.SD_44.Remove(CurrentBank.Entity);
                        BankCollection.Remove(CurrentBank);
                        isBankDeleted = b.State != RowStatus.NewRow;
                        continue;
                    }

                    var c = new Bank();
                    c.LoadFromEntity(b.Entity);
                    GlobalOptions.ReferencesCache.AddOrUpdate(c);
                }
                isBankDeleted = false;
                foreach(var b in BankCollection)
                    b.State = RowStatus.NotEdited;
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
