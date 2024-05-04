using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursAM2.Managers.Nomenkl;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Reference.Nomenkl
{
    //sd_119
    public class ProductTypeReferenceWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private readonly NomenklProductTypeManager NomenklProductTypeManager = new NomenklProductTypeManager();

        #endregion

        #region Constructors

        public ProductTypeReferenceWindowViewModel()
        {
            WindowName = "Справочник типов номенклатур";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #endregion

        #region Properties

        public ObservableCollection<NomenklProductType> Rows { set; get; } =
            new ObservableCollection<NomenklProductType>();

        public ObservableCollection<NomenklProductType> DeletedRows { set; get; } =
            new ObservableCollection<NomenklProductType>();

        public ObservableCollection<NomenklProductType> SelectedRows { set; get; } =
            new ObservableCollection<NomenklProductType>();

        private NomenklProductType myCurrentRow;

        public NomenklProductType CurrentRow
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
            var lst = new List<NomenklProductType>();
            foreach (var row in Rows)
                if (row.State != RowStatus.NotEdited)
                    lst.Add(row);
            foreach (var row in DeletedRows)
                lst.Add(row);
            if (lst.Count > 0)
                if (NomenklProductTypeManager.Save(lst))
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
            Rows.Add(new NomenklProductType
            {
                DOC_CODE = -1,
                State = RowStatus.NewRow,
                MC_NAME = "",
                MC_DELETED = 0,
                MC_PROC_OTKL = 0,
                MC_TARA = 0,
                MC_TRANSPORT = 0,
                MC_PREDOPLATA = 0,
                Name = "новый"
            });
        }

        public override void RefreshData(object obj)
        {
            Rows.Clear();
            foreach (var u in NomenklProductTypeManager.LoadList()) Rows.Add(u);
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
            Rows.Add(NomenklProductTypeManager.NewCopy(CurrentRow));
        }

        public ICommand ItemsDeleteCommand
        {
            get { return new Command(ItemsDelete, _ => CurrentRow != null || SelectedRows.Count > 0); }
        }

        private void ItemsDelete(object obj)
        {
            foreach (var row in SelectedRows)
            {
                row.State = RowStatus.Deleted;
                DeletedRows.Add(row);
            }

            foreach (var row in DeletedRows) Rows.Remove(row);
        }

        #endregion
    }

    //sd_77
}
