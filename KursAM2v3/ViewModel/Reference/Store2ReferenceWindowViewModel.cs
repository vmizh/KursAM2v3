using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository.UserRepository;

namespace KursAM2.ViewModel.Reference
{
    public sealed class Store2ReferenceWindowViewModel : TreeReferenceBaseWindowViewModel, ILayoutFluentSetEditors
    {
        #region Constructors

        public Store2ReferenceWindowViewModel()
        {
            myUserRepository = new UserRepository(GlobalOptions.KursSystem());
            var usrs = myUserRepository.GetAllUsers();
            foreach (var u in usrs)
            {
                var u2 = myUserRepository.GetUsers(u.Name);
                myAllUsers.Add(new SelectUser
                {
                    IsSelected = false,
                    UserId = u2.USR_ID,
                    UserFullName = u.FullName,
                    UserName = u.Name
                });
            }

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

        #region Fields

        private readonly StoreManager Manager;
        private RSViewModelBase myCurrentStore;
        private SelectUser myCurrentUser;
        private readonly UserRepository myUserRepository;

        private readonly ObservableCollection<SelectUser> myAllUsers = new ObservableCollection<SelectUser>();

        #endregion

        #region Properties

        public new List<WarehouseViewModel> Rows { set; get; } = new List<WarehouseViewModel>();


        public override string Key => "Id";
        public override string ParentKey => "ParentId";
        public override string WindowName => "Справочник складов";
        public override string LayoutName => "Store2ReferenceWindowViewModel";

        public ObservableCollection<SelectUser> SelectedUsers { set; get; } = new ObservableCollection<SelectUser>();

        public override RSViewModelBase CurrentRow
        {
            set
            {
                if (value == myCurrentStore) return;
                myCurrentStore = value;
                LoadRightStoreForUsers();
                RaisePropertyChanged();
            }
            get => myCurrentStore;
        }

        public SelectUser CurrentUser
        {
            set
            {
                if (value == myCurrentUser) return;
                myCurrentUser = value;
                RaisePropertyChanged();
            }
            get => myCurrentUser;
        }

        #endregion

        #region Methods

        private void LoadRightStoreForUsers()
        {
            SelectedUsers.Clear();
            if (CurrentRow.State == RowStatus.NewRow) return;
            var userForStore = myUserRepository.GetUsersForStores(CurrentRow.DocCode);
            foreach (var usr in myAllUsers)
            {
                var newItem = new SelectUser
                {
                    UserName = usr.UserName,
                    UserFullName = usr.UserFullName,
                    UserId = usr.UserId,
                    IsSelected = userForStore.Any(_ => _.Name == usr.UserName)
                };
                SelectedUsers.Add(newItem);
            }
        }

        public void SetGridColumn(GridColumn col)
        {
            throw new NotImplementedException();
        }

        public void SetTreeListColumn(TreeListColumn col)
        {
            //if (!(CurrentRow is WarehouseIn cur)) return;
            switch (col.FieldName)
            {
                case nameof(WarehouseViewModel.Region):
                    var regionEdit = new ButtonEditSettings
                    {
                        TextWrapping = TextWrapping.Wrap,
                        IsTextEditable = false
                    };
                    regionEdit.DefaultButtonClick += RegionEdit_DefaultButtonClick;
                    col.EditSettings = regionEdit;
                    break;
                case nameof(WarehouseViewModel.Employee):
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
            if (!(CurrentRow is WarehouseViewModel cur)) return;
            var emp = StandartDialogs.SelectEmployee();
            cur.Employee = emp;
        }

        private void RegionEdit_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(CurrentRow is WarehouseViewModel cur)) return;
            var reg = StandartDialogs.SelectRegion();
            cur.Region = reg;
        }

        #endregion


        #region Commands

        public override void SaveData(object data)
        {
            var lst = Rows.Where(row => row.State != RowStatus.NotEdited).ToList();
            lst.AddRange(DeletedRows.Cast<WarehouseViewModel>());
            if (lst.Count <= 0) return;
            if (!Manager.Save(lst)) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var r in Rows.Where(_ => _.State != RowStatus.NotEdited))
                {
                    var ent = ctx.SD_27.FirstOrDefault(_ => _.DOC_CODE == r.DocCode);
                    if (ent == null) continue;
                    var c = new Warehouse();
                    c.LoadFromEntity(ent, GlobalOptions.ReferencesCache);
                    GlobalOptions.ReferencesCache.AddOrUpdate(c);
                    r.myState = RowStatus.NotEdited;
                }
            }

            DeletedRows.Clear();
            RaisePropertyChanged(nameof(IsCanSaveData));
        }

        public override void ItemNewEmpty(object obj)
        {
            var newItem = Manager.New(-1);
            Rows.Add(newItem);
            SetNewItemInControl(newItem);
        }

        public override void ItemNewChildEmpty(object obj)
        {
            var newItem = Manager.New((WarehouseViewModel)CurrentRow);
            Rows.Add(newItem);
            SetNewItemInControl(newItem);
        }

        public override void ItemNewCopy(object obj)
        {
            var newItem = Manager.NewCopy((WarehouseViewModel)CurrentRow);
            Rows.Add(newItem);
            SetNewItemInControl(newItem);
        }

        public override void RefreshData(object obj)
        {
            //if (!(Form is TreeListFormBaseView2 frm)) return;
            Rows.Clear();
            DeletedRows.Clear();
            Rows.AddRange(Manager.Load());
        }

        private bool IsDataCorrect()
        {
            return Rows.Where(_ => _.State != RowStatus.NotEdited).All(row =>
                row.Employee != null && row.Region != null && !string.IsNullOrEmpty(row.Name.TrimStart().TrimEnd()));
        }

        public override bool IsCanSaveData =>
            (Rows != null && Rows.Any(_ => _.State != RowStatus.NotEdited) && IsDataCorrect()) || DeletedRows.Count > 0;

        [Display(AutoGenerateField = false)]
        public ICommand CopyRightStore
        {
            get { return new Command(CopyRight, _ => true); }
        }

        private void CopyRight(object obj)
        {
        }

        [Display(AutoGenerateField = false)]
        public ICommand UpdateRightUserStoreCommand
        {
            get { return new Command(UpdateRightUserStore, _ => true); }
        }

        private void UpdateRightUserStore(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                ctx.Database.ExecuteSqlCommand(
                    CurrentUser.IsSelected
                        ? $"INSERT INTO HD_27(DOC_CODE, USR_ID) VALUES({CustomFormat.DecimalToSqlDecimal(CurrentRow.DocCode)},{CurrentUser.UserId})"
                        : $"DELETE FROM HD_27 WHERE DOC_CODE = {CustomFormat.DecimalToSqlDecimal(CurrentRow.DocCode)} AND USR_ID = {CurrentUser.UserId}");
                ctx.SaveChanges();
            }
        }

        #endregion
    }
}
