using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using KursAM2.View.Base;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Reference
{
    public class RegionRefViewModel : TreeReferenceBaseWindowViewModel, ILayoutFluentSetEditors
    {
        private int newDC = -1;

        public RegionRefViewModel()
        {
            WindowName = "Справочник регионов";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            //var errorManager = new StandartErrorManager(GlobalOptions.GetEntities(), Name, true)
            //{
            //    FormName = "Справочник складов"
            //};
            RefreshData(null);
        }

        public void SetGridColumn(GridColumn col)
        {
            throw new NotImplementedException();
        }

        public void SetTreeListColumn(TreeListColumn col)
        {
            //if (!(CurrentRow is WarehouseIn cur)) return;
            //switch (col.FieldName)
            //{
            //case nameof(Warehouse.Region):
            //    var regionEdit = new ButtonEditSettings
            //    {
            //        TextWrapping = TextWrapping.Wrap,
            //        IsTextEditable = false
            //    };
            //    regionEdit.DefaultButtonClick += RegionEdit_DefaultButtonClick;
            //    col.EditSettings = regionEdit;
            //    break;
            //case nameof(Warehouse.Employee):
            //    var empEdit = new ButtonEditSettings
            //    {
            //        TextWrapping = TextWrapping.Wrap,
            //        IsTextEditable = false
            //    };
            //    empEdit.DefaultButtonClick += EmpEdit_DefaultButtonClick;
            //    col.EditSettings = empEdit;
            //    break;
            //}
        }

        public void SetDataLayout(DataLayoutItem item, string propertyName)
        {
            throw new NotImplementedException();
        }

        #region command

        public override void RefreshData(object o)
        {
            Rows = new List<Region>(MainReferences.Regions.Values);
            RaisePropertyChanged(nameof(Rows));
        }

        public override void ItemNewEmpty(object obj)
        {
            if (CurrentRow != null)
            {
                var newItem = new Region
                {
                    DocCode = newDC,
                    Parent = CurrentRow.Parent,
                    ParentDC = CurrentRow.ParentDC,
                    myState = RowStatus.NewRow
                };
                Rows.Add(newItem);
                SetNewItemInControl(newItem);
            }
            else
            {
                var newItem = new Region
                {
                    DocCode = newDC,
                    myState = RowStatus.NewRow
                };
                Rows.Add(newItem);
                SetNewItemInControl(newItem);
            }

            newDC--;
        }

        public override void ItemNewChildEmpty(object obj)
        {
            var newItem = new Region
            {
                DocCode = newDC,
                Parent = CurrentRow.Parent,
                ParentDC = CurrentRow.DocCode,
                REG_NAME = null,
                myState = RowStatus.NewRow
            };
            Rows.Add(newItem);
            SetNewItemInControl(newItem);
            newDC--;
        }

        public override void ItemNewCopy(object obj)
        {
            var newItem = new Region
            {
                DocCode = newDC,
                Parent = CurrentRow,
                ParentDC = CurrentRow.ParentDC,
                myState = RowStatus.NewRow
            };
            Rows.Add(newItem);
            SetNewItemInControl(newItem);
            newDC--;
        }

        public override void ItemDelete(object obj)
        {
            if (CurrentRow == null) return;
            CurrentRow.myState = RowStatus.Deleted;
            DeletedRows.Add(CurrentRow);
            Rows.Remove(CurrentRow as Region);
            if (Form is TreeListFormBaseView frm) frm.gridDocuments.RefreshData();
        }

        #endregion

        #region properties

        public override string Key => "DOC_CODE";
        public override string ParentKey => "REG_PARENT_DC";

        public new List<Region> Rows { set; get; } =
            new List<Region>();

        public override bool IsCanSaveData =>
            (Rows != null && Rows.Any(_ => _.State != RowStatus.NotEdited)) || DeletedRows.Count > 0;

        public override bool IsCanDelete()
        {
            return CurrentRow != null && Rows.All(_ => _.ParentDC != CurrentRow.DocCode);
        }

        private bool Save(List<Region> listProds)
        {
            if (listProds == null || !listProds.Any()) return true;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tn = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        // ReSharper disable once LocalVariableHidesMember
                        var newDC = ctx.SD_23.Any() ? ctx.SD_23.Max(_ => _.DOC_CODE) + 1 : 10230000001;
                        foreach (var u in listProds)
                            switch (u.State)
                            {
                                case RowStatus.NewRow:
                                    if (listProds.Any(_ => _.ParentDC == u.DocCode))
                                        foreach (var prd in listProds.Where(_ => _.ParentDC == u.DocCode))
                                            prd.ParentDC = newDC;
                                    var sd23 = new SD_23
                                    {
                                        DOC_CODE = newDC,
                                        REG_NAME = u.Name,
                                        REG_PARENT_DC = u.ParentDC
                                    };
                                    ctx.SD_23.Add(sd23);
                                    newDC++;
                                    break;
                                case RowStatus.Edited:
                                    var old1 = ctx.SD_23.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old1 != null)
                                    {
                                        old1.REG_NAME = u.Name;
                                        old1.REG_PARENT_DC = u.ParentDC;
                                    }

                                    break;
                                case RowStatus.Deleted:
                                    var old = ctx.SD_23.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old != null)
                                        ctx.SD_23.Remove(old);
                                    break;
                            }

                        ctx.SaveChanges();
                        tn.Commit();
                        MainReferences.Refresh();
                        DeletedRows.Clear();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                        tn.Rollback();
                        return false;
                    }
                }
            }
        }

        public override void SaveData(object data)
        {
            var lst = new List<Region>();
            foreach (var row in Rows)
                if (row.State != RowStatus.NotEdited)
                    lst.Add(row);
            foreach (var row in DeletedRows)
                lst.Add(row as Region);
            if (lst.Count <= 0) return;
            if (Save(lst))
            {
                RefreshData(null);
                return;
            }

            foreach (var r in Rows)
                r.myState = RowStatus.NotEdited;
            DeletedRows.Clear();
            RaisePropertyChanged(nameof(IsCanSaveData));
        }

        #endregion
    }
}
