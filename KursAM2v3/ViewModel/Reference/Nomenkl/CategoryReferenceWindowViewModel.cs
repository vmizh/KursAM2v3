using System.Collections.Generic;
using System.Linq;
using KursAM2.Managers.Nomenkl;
using KursAM2.View.Base;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference.Nomenkl
{
    public class CategoryReferenceWindowViewModel : TreeReferenceBaseWindowViewModel
    {
        #region Fields

        private readonly NomenklCategoryManager Manager = new NomenklCategoryManager();

        #endregion

        #region Constructors

        public CategoryReferenceWindowViewModel()
        {
            WindowName = "Справочник категорий товара";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #endregion

        #region Properties

        public new List<NomenklGroupViewModel> Rows { set; get; } = new List<NomenklGroupViewModel>();

        #endregion

        #region Commands

        public override void SaveData(object data)
        {
            var lst = new List<NomenklGroupViewModel>();
            foreach (var row in Rows)
                if (row.State != RowStatus.NotEdited)
                    lst.Add(row);
            foreach (var row in DeletedRows)
                lst.Add((NomenklGroupViewModel) row);
            if (lst.Count <= 0) return;
            if (!Manager.Save(lst)) return;
            foreach (var r in Rows)
                r.myState = RowStatus.NotEdited;
            DeletedRows.Clear();
            RaisePropertyChanged(nameof(IsCanSaveData));
        }

        public override void ItemNewEmpty(object obj)
        {
            var parent = Rows.FirstOrDefault(_ => _.DocCode == CurrentRow?.ParentDC);
            var newItem = Manager.New(parent);
            SetNewItemInControl(newItem);
        }

        public override void ItemNewChildEmpty(object obj)
        {
            var newItem = Manager.New((NomenklGroupViewModel) CurrentRow);
            SetNewItemInControl(newItem);
        }

        public override void ItemNewCopy(object obj)
        {
            var newItem = Manager.NewCopy((NomenklGroupViewModel) CurrentRow);
            SetNewItemInControl(newItem);
        }

        public override void RefreshData(object obj)
        {
            if (!(Form is TreeListFormBaseView frm)) return;
            Rows.Clear();
            DeletedRows.Clear();
            Rows = new List<NomenklGroupViewModel>(Manager.Load());
            frm.gridDocuments.RefreshData();
        }

        public override bool IsCanSaveData =>
            (Rows != null && Rows.Any(_ => _.State != RowStatus.NotEdited)) || DeletedRows.Count > 0;

        #endregion
    }
}
