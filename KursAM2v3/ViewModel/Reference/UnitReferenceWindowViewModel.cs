using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursAM2.Managers.Nomenkl;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference
{
    public class UnitReferenceWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private readonly UnitManager UnitManager = new UnitManager();

        #endregion

        #region Constructors

        public UnitReferenceWindowViewModel()
        {
            WindowName = "Справочник единиц измерения";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #endregion

        #region Properties

        public ObservableCollection<UnitViewModel> Rows { set; get; } = new ObservableCollection<UnitViewModel>();

        public ObservableCollection<UnitViewModel> DeletedRows { set; get; } =
            new ObservableCollection<UnitViewModel>();

        public ObservableCollection<UnitViewModel> SelectedRows { set; get; } =
            new ObservableCollection<UnitViewModel>();

        private UnitViewModel myCurrentRow;

        public UnitViewModel CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (Equals(myCurrentRow,value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override void SaveData(object data)
        {
            var lst = new List<UnitViewModel>();
            foreach (var row in Rows)
                if (row.State != RowStatus.NotEdited)
                    lst.Add(row);
            foreach (var row in DeletedRows)
                lst.Add(row);
            if (lst.Count > 0)
                if (UnitManager.Save(lst))
                {
                    foreach (var r in Rows)
                        r.myState = RowStatus.NotEdited;
                    DeletedRows.Clear();
                    RaisePropertyChanged(nameof(IsCanSaveData));
                }
        }

        public ICommand ItemNewEmptyCommand
        {
            get { return new Command(ItemNewEmpty, _ => true); }
        }

        private void ItemNewEmpty(object obj)
        {
            Rows.Add(UnitManager.New(null));
        }

        public override void RefreshData(object obj)
        {
            Rows.Clear();
            foreach (var u in UnitManager.LoadList()) Rows.Add(u);
        }

        public override bool IsCanSaveData =>
            (Rows != null && Rows.Any(_ => _.State != RowStatus.NotEdited)) || DeletedRows.Count > 0;

        //    ItemNewCopyCommand
        //}" />
        //</MenuItem>
        //<Separator />
        //<MenuItem Header = "Удалить выделенные строки" Command="{Binding ItemsDeleteCommand}"
        public ICommand ItemNewCopyCommand
        {
            get { return new Command(ItemNewCopy, _ => CurrentRow != null); }
        }

        private void ItemNewCopy(object obj)
        {
            Rows.Add(UnitManager.NewCopy(CurrentRow));
        }

        public ICommand ItemsDeleteCommand
        {
            get { return new Command(ItemsDelete, _ => CurrentRow != null || SelectedRows.Count > 0); }
        }

        private void ItemsDelete(object obj)
        {
            var dList = new List<UnitViewModel>();
            foreach (var row in SelectedRows)
                if (row.State == RowStatus.NewRow)
                    dList.Add(row);
            foreach (var d in dList) Rows.Remove(d);
            foreach (var row in SelectedRows)
            {
                row.State = RowStatus.Deleted;
                DeletedRows.Add(row);
            }

            foreach (var row in DeletedRows) Rows.Remove(row);
        }

        #endregion
    }
}
