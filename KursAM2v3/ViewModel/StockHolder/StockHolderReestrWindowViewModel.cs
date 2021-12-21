using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.StockHolder;
using Core.EntityViewModel.Systems;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using KursAM2.Dialogs;
using KursAM2.View.StockHolder;

namespace KursAM2.ViewModel.StockHolder
{
    public sealed class StockHolderReestrWindowViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public StockHolderReestrWindowViewModel()
        {
            repository =
                new StockHolderRepository(unitOfWork);
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartReestrRightBar(this);
            RefreshData(null);
        }

        #endregion

        #region Fields

        private readonly IStockHolderRepository repository;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private bool isHoldersDeleted;
        List<Users> Users = GlobalOptions.KursSystem().Users.Where(_ => !_.IsDeleted).AsNoTracking().ToList();

        #endregion

        #region Properties

        public override string WindowName => "Реестр акционеров";
        public override string LayoutName => "StockHolderReestrWindowViewModel";

        public ObservableCollection<StockHolderViewModel> StockHolders { set; get; } =
            new ObservableCollection<StockHolderViewModel>();

        public ObservableCollection<StockHolderViewModel> SelectedHolders { set; get; } =
            new ObservableCollection<StockHolderViewModel>();

        private StockHolderViewModel myCurrentHolder;

        public StockHolderViewModel CurrentHolder
        {
            get => myCurrentHolder;
            set
            {
                if (myCurrentHolder == value) return;
                myCurrentHolder = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override bool IsCanSaveData =>
            StockHolders.Any(_ => _.State != RowStatus.NotEdited) || isHoldersDeleted;

        public ICommand SelectKontragentCommand
        {
            get { return new Command(SelectKontragent, _ => CurrentHolder != null); }
        }

        private void SelectKontragent(object obj)
        {
            var kontr = StandartDialogs.SelectKontragent();
            if (kontr == null) return;
            CurrentHolder.Kontragent = kontr;
            CurrentHolder.RaisePropertyChanged(nameof(IsCanSaveData));
            if (Form is StockHolderReestrView frm) frm.TableViewStockHolder.CloseEditor();
        }

        public ICommand DeleteKontragentCommand
        {
            get
            {
                return new Command(DeleteKontragent, _ => CurrentHolder != null
                                                          && CurrentHolder.Kontragent != null);
            }
        }

        private void DeleteKontragent(object obj)
        {
            CurrentHolder.Kontragent = null;
            CurrentHolder.RaisePropertyChanged(nameof(IsCanSaveData));
            if (Form is StockHolderReestrView frm) frm.TableViewStockHolder.CloseEditor();
        }

        public ICommand SelectEmployeeCommand
        {
            get { return new Command(SelectEmployee, _ => CurrentHolder != null); }
        }

        private void SelectEmployee(object obj)
        {
            var emp = StandartDialogs.SelectEmployee();
            if (emp == null) return;
            CurrentHolder.Employee = emp;
            CurrentHolder.RaisePropertyChanged(nameof(IsCanSaveData));
            if (Form is StockHolderReestrView frm) frm.TableViewStockHolder.CloseEditor();
        }

        public ICommand DeleteEmployeeCommand
        {
            get
            {
                return new Command(DeleteEmployee, _ => CurrentHolder != null
                                                        && CurrentHolder.Employee != null);
            }
        }

        private void DeleteEmployee(object obj)
        {
            CurrentHolder.Employee = null;
            CurrentHolder.RaisePropertyChanged(nameof(IsCanSaveData));
            if (Form is StockHolderReestrView frm) frm.TableViewStockHolder.CloseEditor();
        }

        public ICommand AddStockHolderCommand
        {
            get { return new Command(AddStockHolder, _ => true); }
        }

        private void AddStockHolder(object obj)
        {
            var newEntity = new StockHolders
            {
                Id = Guid.NewGuid()
            };
            unitOfWork.Context.StockHolders.Add(newEntity);
            var newItem = new StockHolderViewModel(newEntity)
            {
                myState = RowStatus.NewRow

            };
            foreach (var u in Users)
            {
                newItem.UserRights.Add(new UserRights
                {
                    User = new KursUser(u),
                    IsSelected = false,
                    Parent = newItem
                });
            }
            StockHolders.Add(newItem);
            if (Form is StockHolderReestrView frm)
            {
                frm.TableViewStockHolder.FocusedRowHandle =
                    frm.GridControlStockHolder.GetRowHandleByListIndex(((IList)frm.GridControlStockHolder.ItemsSource)
                        .Count - 1);
                frm.GridControlStockHolder.SelectedItems.Clear();
                frm.GridControlStockHolder.SelectedItems.Add(CurrentHolder);
            }
        }

        public ICommand DeleteStockHolderCommand
        {
            get { return new Command(DeleteStockHolder, _ => CurrentHolder != null); }
        }

        private void DeleteStockHolder(object obj)
        {
            if (CurrentHolder.State == RowStatus.NewRow)
            {
                unitOfWork.Context.Entry(CurrentHolder.Entity).State = EntityState.Detached;
                StockHolders.Remove(CurrentHolder);
            }
            else
            {
                isHoldersDeleted = true;
                repository.GenericRepository.Context.StockHolders.Remove(CurrentHolder.Entity);
                StockHolders.Remove(CurrentHolder);
            }
        }

        public override void RefreshData(object obj)
        {
            if (IsCanSaveData)
            {
                var service = GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                if (service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this) == MessageResult.Yes)
                {
                    SaveData(null);
                    return;
                }
            }

            StockHolders.Clear();
            try
            {
                foreach (var sh in repository.GenericRepository.GetAll())
                    StockHolders.Add(new StockHolderViewModel(sh)
                    {
                        State = RowStatus.NotEdited
                    });
                foreach (var sh in StockHolders)
                {
                    var rights = unitOfWork.Context.StockHolderUserRights
                        .Where(_ => _.StockHolderId == sh.Id).ToList();
                    foreach (var u in Users)
                    {
                        if (rights.Any(_ => _.UserId == u.Id))
                        {
                            sh.UserRights.Add(new UserRights
                            {
                                User = new KursUser(u),
                                IsSelected = true,
                                myState = RowStatus.NotEdited,
                                Parent = sh
                            });
                        }
                        else
                        {
                            sh.UserRights.Add(new UserRights
                            {
                                User = new KursUser(u),
                                IsSelected = false,
                                myState = RowStatus.NotEdited,
                                Parent = sh
                            });
                        }
                    }
                }

                isHoldersDeleted = false;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SaveData(object data)
        {
            var dellist = unitOfWork.Context.StockHolderUserRights.ToList();
            foreach (var d in dellist)
            {
                unitOfWork.Context.StockHolderUserRights.Remove(d);
            }

            try
            {
                unitOfWork.CreateTransaction();
                foreach (var sh in StockHolders)
                {
                    foreach (var r in sh.UserRights.Where(_ => _.IsSelected))
                    {
                        unitOfWork.Context.StockHolderUserRights.Add(new StockHolderUserRights
                        {
                            Id = Guid.NewGuid(),
                            StockHolderId = sh.Id,
                            UserId = r.User.Id
                        });
                    }
                }

                unitOfWork.Save();
                unitOfWork.Commit();
                foreach (var s in StockHolders) s.myState = RowStatus.NotEdited;
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
    }
}