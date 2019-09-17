using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Base;

namespace KursAM2.ViewModel.Reference
{
    public class Store2ReferenceWindowViewModel : TreeReferenceBaseWindowViewModel, ILayoutFluentSetEditors
    {
        #region Fields

        private readonly StoreManager Manager;

        #endregion

        #region Constructors

        public Store2ReferenceWindowViewModel()
        {
            WindowName = "Справочник складов";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            var errorManager = new StandartErrorManager(GlobalOptions.GetEntities(), Name, true)
            {
                FormName = "Справочник складов"
            };
            Manager = new StoreManager(errorManager);
            RefreshData(null);
        }

        #endregion

        #region Properties

        public new List<Warehouse> Rows { set; get; } = new List<Warehouse>();

        #endregion

        public override string Key => "Id";
        public override string ParentKey => "ParentId";

        public void SetGridColumn(GridColumn col)
        {
            throw new NotImplementedException();
        }

        public void SetTreeListColumn(TreeListColumn col)
        {
            //if (!(CurrentRow is WarehouseIn cur)) return;
            switch (col.FieldName)
            {
                case nameof(Warehouse.Region):
                    var regionEdit = new ButtonEditSettings
                    {
                        TextWrapping = TextWrapping.Wrap,
                        IsTextEditable = false
                    };
                    regionEdit.DefaultButtonClick += RegionEdit_DefaultButtonClick;
                    col.EditSettings = regionEdit;
                    break;
                case nameof(Warehouse.Employee):
                    var empEdit = new ButtonEditSettings
                    {
                        TextWrapping = TextWrapping.Wrap,
                        IsTextEditable = false
                    };
                    empEdit.DefaultButtonClick += EmpEdit_DefaultButtonClick;
                    col.EditSettings = empEdit;
                    break;
            }
        }

        public void SetDataLayout(DataLayoutItem item, string propertyName)
        {
            throw new NotImplementedException();
        }

        private void EmpEdit_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(CurrentRow is Warehouse cur)) return;
            var emp = StandartDialogs.SelectEmployee();
            cur.Employee = emp;
        }

        private void RegionEdit_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(CurrentRow is Warehouse cur)) return;
            var reg = StandartDialogs.SelectRegion();
            cur.Region = reg;
        }

        #region Commands

        public override void SaveData(object data)
        {
            var lst = new List<Warehouse>();
            foreach (var row in Rows)
                if (row.State != RowStatus.NotEdited)
                    lst.Add(row);
            foreach (var row in DeletedRows)
                lst.Add((Warehouse) row);
            if (lst.Count <= 0) return;
            if (!Manager.Save(lst)) return;
            foreach (var r in Rows)
                r.myState = RowStatus.NotEdited;
            DeletedRows.Clear();
            RaisePropertyChanged(nameof(IsCanSaveData));
        }

        public override void ItemNewEmpty(object obj)
        {
            if (CurrentRow != null)
            {
                var newItem = Manager.New((Warehouse) CurrentRow);
                newItem.Parent = CurrentRow;
                Rows.Add(newItem);
                SetNewItemInControl(newItem);
            }
            else
            {
                var newItem = Manager.New();
                Rows.Add(newItem);
                SetNewItemInControl(newItem);
            }
        }

        public override void ItemNewChildEmpty(object obj)
        {
            var newItem = Manager.New((Warehouse) CurrentRow);
            Rows.Add(newItem);
            SetNewItemInControl(newItem);
        }

        public override void ItemNewCopy(object obj)
        {
            var newItem = Manager.NewCopy((Warehouse) CurrentRow);
            Rows.Add(newItem);
            SetNewItemInControl(newItem);
        }

        public override void RefreshData(object obj)
        {
            if (!(Form is TreeListFormBaseView frm)) return;
            Rows.Clear();
            DeletedRows.Clear();
            Rows.AddRange(Manager.Load());
            //if (Rows.Count == 0) ItemNewEmpty(null);
            frm.gridDocuments.RefreshData();
        }

        private bool IsDataCorrect()
        {
            return Rows.Where(_ => _.State != RowStatus.NotEdited).All(row =>
                row.Employee != null && row.Region != null && !string.IsNullOrEmpty(row.Name.TrimStart().TrimEnd()));
        }

        public override bool IsCanSaveData =>
            Rows != null && Rows.Any(_ => _.State != RowStatus.NotEdited) && IsDataCorrect() || DeletedRows.Count > 0;

        #endregion
    }
}