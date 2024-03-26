using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursAM2.View.Base;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Reference
{
    public abstract class TreeReferenceBaseWindowViewModel : RSWindowViewModelBase, ITreeKey
    {
        private RSViewModelBase myCurrentRow;

        public TreeReferenceBaseWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RaisePropertyChanged(nameof(Rows));
        }

        public virtual List<RSViewModelBase> Rows { set; get; } = new List<RSViewModelBase>();

        public virtual ObservableCollection<RSViewModelBase> DeletedRows { set; get; } =
            new ObservableCollection<RSViewModelBase>();

        public virtual ObservableCollection<RSViewModelBase> SelectedRows { set; get; } =
            new ObservableCollection<RSViewModelBase>();

        public virtual RSViewModelBase CurrentRow
        {
            get => myCurrentRow;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentRow == value) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public virtual string Key { get; set; } = "DocCode";
        public virtual string ParentKey { get; set; } = "ParentDC";

        public List<RSViewModelBase> GetAllKinds(RSViewModelBase prod, List<RSViewModelBase> listProds)
        {
            var ret = new List<RSViewModelBase> {prod};
            foreach (var p in listProds.Where(_ => _.ParentDC == prod.DocCode)) ret.AddRange(GetAllKinds(p, listProds));
            return ret;
        }

        #region Commands

        public ICommand ItemNewEmptyCommand
        {
            get { return new Command(ItemNewEmpty, _ => true); }
        }

        public virtual void ItemNewEmpty(object obj)
        {
            if (!(obj is RSViewModelBase newItem)) return;
            SetNewItemInControl(newItem);
        }

        public virtual void SetNewItemInControl(RSViewModelBase newItem)
        {
            newItem.DocCode = minId;
            Rows.Add(newItem);
            minId--;
            switch (Form)
            {
                case TreeListFormBaseView f:
                    f.gridDocuments.RefreshData();
                    break; 
                case TreeListFormBaseView2 f1:
                    f1.gridDocuments.RefreshData();
                    break;
            }
            CurrentRow = newItem;
        }

        public ICommand ItemNewChildEmptyCommand
        {
            get { return new Command(ItemNewChildEmpty, _ => CurrentRow != null); }
        }

        public virtual void ItemNewChildEmpty(object obj)
        {
            if (!(obj is RSViewModelBase newItem)) return;
            SetNewItemInControl(newItem);
        }

        public ICommand ItemNewCopyCommand
        {
            get { return new Command(ItemNewCopy, _ => CurrentRow != null); }
        }

        public int minId = -1;

        public virtual void ItemNewCopy(object obj)
        {
            if (!(obj is RSViewModelBase newItem)) return;
            SetNewItemInControl(newItem);
        }

        public virtual bool IsRowsNotContainsChilds()
        {
            var lst = new List<RSViewModelBase>(new List<RSViewModelBase>(Rows));
            lst.AddRange(new List<RSViewModelBase>(DeletedRows));
            foreach (var r in SelectedRows)
                if (lst.Any(_ => _.ParentDC == r.DocCode))
                    return false;
            return true;
        }

        public virtual bool IsCanDelete()
        {
            return CurrentRow != null;
        }

        public ICommand ItemDeleteCommand
        {
            get
            {
                return new Command(ItemDelete,
                    _ => IsCanDelete());
            }
        }

        public virtual void ItemDelete(object obj)
        {
            if (CurrentRow == null) return;
            if (Rows.Any(_ =>
                _.ParentDC == CurrentRow.DocCode &&
                (CurrentRow.State == RowStatus.Edited || CurrentRow.State == RowStatus.NotEdited))) return;
            CurrentRow.State = RowStatus.Deleted;
            DeletedRows.Add(CurrentRow);
            Rows.Remove(CurrentRow);
            if (Form is TreeListFormBaseView frm) frm.gridDocuments.RefreshData();
        }

        #endregion
    }
}
