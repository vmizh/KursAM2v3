using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using KursDomain;
using KursDomain.Documents.AccruedAmount;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Reference
{
    public class AccruedAmountTypeWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Fields

        private AccruedAmountTypeViewModel myCurrentType;

        #endregion

        #region Constructors

        public AccruedAmountTypeWindowViewModel()
        {
            GenericRepository = new GenericKursDBRepository<AccruedAmountType>(UnitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #endregion

        public string this[string columnName] => null;

        [Display(AutoGenerateField = false)]
        public string Error => GetError();

        #region Fields

        public readonly GenericKursDBRepository<AccruedAmountType> GenericRepository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        #endregion

        #region Properties

        public override string ToolTipForSave => Error == null ? "Сохранить изменения" : GetError();

        public override string LayoutName => "AccruedAmountTypeViewModel";

        public ObservableCollection<AccruedAmountTypeViewModel> Types { set; get; } =
            new ObservableCollection<AccruedAmountTypeViewModel>();

        public ObservableCollection<AccruedAmountTypeViewModel> DeletedTypes { set; get; } =
            new ObservableCollection<AccruedAmountTypeViewModel>();

        public AccruedAmountTypeViewModel CurrentType
        {
            get => myCurrentType;
            set
            {
                if (myCurrentType == value) return;
                myCurrentType = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region Commands

        [Display(AutoGenerateField = false)]
        public ICommand AddNewCommand
        {
            get { return new Command(AnddNew, _ => true); }
        }

        private void AnddNew(object obj)
        {
            var d = new AccruedAmountTypeViewModel
            {
                State = RowStatus.NewRow
            };
            UnitOfWork.Context.AccruedAmountType.Add(d.Entity);
            Types.Add(d);
        }
        [Display(AutoGenerateField = false)]
        public ICommand DeleteCommand
        {
            get
            {
                return new Command(Delete, _ => CurrentType != null);
            }
        }

        private void Delete(object obj)
        {
            if (CurrentType.State != RowStatus.NewRow)
            {
                UnitOfWork.Context.AccruedAmountType.Remove(CurrentType.Entity);
                DeletedTypes.Add(CurrentType);
            }

            Types.Remove(CurrentType);
        }

        public override void SaveData(object data)
        {
            UnitOfWork.CreateTransaction();
            try
            {
                UnitOfWork.Save();
                UnitOfWork.Commit();
                DeletedTypes.Clear();
                foreach (var t in Types) t.myState = RowStatus.NotEdited;
                MainReferences.Refresh();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                WindowManager.ShowError(ex);
            }
        }

        public override void RefreshData(object obj)
        {
            var WinManager = new WindowManager();
            if (IsCanSaveData)
            {
                var res = WinManager.ShowWinUIMessageBox("В справочник внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        return;
                    case MessageBoxResult.No:
                        foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
                        DeletedTypes.Clear();
                        foreach (var t in Types) t.myState = RowStatus.NotEdited;
                        return;
                }
            }
            else
            {
                DeletedTypes.Clear();
                Types.Clear();
                foreach (var d in GenericRepository.GetAll())
                {
                    Types.Add(new AccruedAmountTypeViewModel(d)
                    {
                        State = RowStatus.NotEdited
                    });
                }
            }
        }

        public override bool IsCanSaveData
        {
            get
            {
                RaisePropertyChanged(nameof(ToolTipForSave));
                return (Types.Any(_ => _.State != RowStatus.NotEdited) || DeletedTypes.Count > 0) && Error == null;
            }
        }

        #endregion

        #region Methods
 
        private string GetError()
        {
            if (Types.Any(_ => string.IsNullOrWhiteSpace(_.Name)))
                return "Наименование типа не может быть пустым";
            return null;
        }

        #endregion
    }
}
