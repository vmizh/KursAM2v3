using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using KursAM2.View.StockHolder;
using KursDomain;
using KursDomain.Documents.StockHolder;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.Repository;

namespace KursAM2.ViewModel.StockHolder
{
    public sealed class StockHolderAccrualTypeWindowViewModel : RSWindowViewModelBase
    {

        #region Fields
        
        private StockHolderAccrualTypeViewModel myCurrentHolderAccrual;
        private bool isDeletedAccruals;
        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));
        private readonly IStockHolderAccrualTypeRepository repository;

        #endregion

        #region Constructors

        public StockHolderAccrualTypeWindowViewModel()
        {
            repository =
                new StockHolderAccrualTypeRepository(unitOfWork);
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartReestrRightBar(this);
            RefreshData(null);
        }

        #endregion

        #region Properties

        public override string WindowName => "Типы начислений для акционеров";
        public override string LayoutName => "StockHolderAccrualTypeWindowViewModel";

        public override bool IsCanSaveData => StockHolderAccruals.Any(_ => _.State != RowStatus.NotEdited)
                                              || isDeletedAccruals;

        public ObservableCollection<StockHolderAccrualTypeViewModel> StockHolderAccruals { set; get; }
            = new ObservableCollection<StockHolderAccrualTypeViewModel>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<StockHolderAccrualTypeViewModel> SelectedHolderAccruals { set; get; }
            = new ObservableCollection<StockHolderAccrualTypeViewModel>();
        
        public StockHolderAccrualTypeViewModel CurrentHolderAccrual
        {
            get => myCurrentHolderAccrual;
            set
            {
                if (myCurrentHolderAccrual == value) return;
                myCurrentHolderAccrual = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand AddStockHolderAccrualCommand
        {
            get { return new Command(AddStockHolderAccrual, _ => true); }
        }

        private void AddStockHolderAccrual(object obj)
        {
            var newEntity = new StockHolderAccrualType
            {
                Id = Guid.NewGuid()
            };
            unitOfWork.Context.StockHolderAccrualType.Add(newEntity);
            var newItem = new StockHolderAccrualTypeViewModel(newEntity)
            {
                myState = RowStatus.NewRow

            };
            StockHolderAccruals.Add(newItem);
            if (Form is StockHolderAccrualTypeView frm)
            {
                frm.TableViewStockHolder.FocusedRowHandle =
                    frm.GridControlStockHolderAccrual.GetRowHandleByListIndex(((IList)frm.GridControlStockHolderAccrual.ItemsSource)
                        .Count - 1);
                frm.GridControlStockHolderAccrual.SelectedItems.Clear();
                frm.GridControlStockHolderAccrual.SelectedItems.Add(CurrentHolderAccrual);
            }
        }

        public ICommand DeleteStockHolderAccrualCommand
        {
            get { return new Command(DeleteStockHolderAccrual, _ => CurrentHolderAccrual != null); }
        }

        private void DeleteStockHolderAccrual(object obj)
        {
            if (CurrentHolderAccrual.State == RowStatus.NewRow)
            {
                unitOfWork.Context.Entry(CurrentHolderAccrual.Entity).State = EntityState.Detached;
                StockHolderAccruals.Remove(CurrentHolderAccrual);
            }
            else
            {
                isDeletedAccruals = true;
                unitOfWork.Context.StockHolderAccrualType.Remove(CurrentHolderAccrual.Entity);
                StockHolderAccruals.Remove(CurrentHolderAccrual);
            }
        }

        public override void RefreshData(object obj)
        {
            if (IsCanSaveData)
            {
                var service = this.GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                if (service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this) == MessageResult.Yes)
                {
                    SaveData(null);
                    return;
                }
            }

            StockHolderAccruals.Clear();
            try
            {
                foreach (var sh in repository.GenericRepository.GetAll())
                    StockHolderAccruals.Add(new StockHolderAccrualTypeViewModel(sh)
                    {
                        State = RowStatus.NotEdited
                    });
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SaveData(object data)
        {
            try
            {
                unitOfWork.CreateTransaction();
                unitOfWork.Save();
                unitOfWork.Commit();
                foreach (var a in SelectedHolderAccruals)
                    a.myState = RowStatus.NotEdited;
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
