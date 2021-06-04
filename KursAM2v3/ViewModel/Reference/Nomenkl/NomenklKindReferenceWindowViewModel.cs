using System.Collections.Generic;
using System.Linq;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.Managers.Nomenkl;
using KursAM2.View.Base;

namespace KursAM2.ViewModel.Reference.Nomenkl
{
    public class NomenklKindReferenceWindowViewModel : TreeReferenceBaseWindowViewModel
    {
        #region Fields

        private readonly NomenklProductKindManager NomenklProductKindManager = new NomenklProductKindManager();

        #endregion

        #region Constructors

        public NomenklKindReferenceWindowViewModel()
        {
            WindowName = "Справочник видов продукции";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            Rows = new List<NomenklProductKind>();
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #endregion

        #region Properties

        public new List<NomenklProductKind> Rows { set; get; }

        #endregion

        #region Commands

        public override void SaveData(object data)
        {
            var lst = new List<NomenklProductKind>();
            foreach (var row in Rows)
                if (row.State != RowStatus.NotEdited)
                    lst.Add(row);
            foreach (var row in DeletedRows)
                lst.Add((NomenklProductKind) row);
            if (lst.Count <= 0) return;
            if (!NomenklProductKindManager.Save(lst)) return;
            foreach (var r in Rows)
                r.myState = RowStatus.NotEdited;
            DeletedRows.Clear();
            RaisePropertyChanged(nameof(IsCanSaveData));
        }

        public override void ItemNewEmpty(object obj)
        {
            var parent = Rows.FirstOrDefault(_ => _.DocCode == CurrentRow?.ParentDC);
            var newItem = NomenklProductKindManager.New(parent);
            SetNewItemInControl(newItem);
        }

        public override void ItemNewChildEmpty(object obj)
        {
            var newItem = NomenklProductKindManager.New((NomenklProductKind) CurrentRow);
            SetNewItemInControl(newItem);
        }

        public override void ItemNewCopy(object obj)
        {
            var newItem = NomenklProductKindManager.NewCopy((NomenklProductKind) CurrentRow);
            SetNewItemInControl(newItem);
        }

        public override void RefreshData(object obj)
        {
            if (!(Form is TreeListFormBaseView frm)) return;
            Rows.Clear();
            DeletedRows.Clear();
            Rows.AddRange(NomenklProductKindManager.Load());
            frm.gridDocuments.RefreshData();
        }

        public override void SetNewItemInControl(RSViewModelBase newItem)
        {
            var item = newItem as NomenklProductKind;
            if (item == null) return;
            newItem.DocCode = minId;
            Rows.Add(item);
            minId--;
            if (Form is TreeListFormBaseView frm) frm.gridDocuments.RefreshData();
            CurrentRow = newItem;
        }

        public override void ItemDelete(object obj)
        {
            if (CurrentRow == null) return;
            if (Rows.Any(_ =>
                _.ParentDC == CurrentRow.DocCode &&
                (CurrentRow.State == RowStatus.Edited || CurrentRow.State == RowStatus.NotEdited))) return;
            CurrentRow.State = RowStatus.Deleted;
            DeletedRows.Add(CurrentRow);
            Rows.Remove(CurrentRow as NomenklProductKind);
            if (Form is TreeListFormBaseView frm) frm.gridDocuments.RefreshData();
        }

        public override bool IsCanSaveData =>
            Rows != null && Rows.Any(_ => _.State != RowStatus.NotEdited) || DeletedRows.Count > 0;

        #endregion
    }
}