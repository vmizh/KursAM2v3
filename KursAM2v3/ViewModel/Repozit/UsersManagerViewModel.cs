using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using DevExpress.Xpf.Grid;
using KursAM2.View.Repozit;
using KursRepositories.ViewModels;

namespace KursAM2.ViewModel.Repozit
{
    public sealed class UsersManagerViewModel : RSWindowViewModelBase
    {
        public UsersManagerViewModel()
        {
            WindowName = "Управление пользователями и доступом";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            RefreshData(null);
        }

        #region Fields

        private UsersViewModel myUserListCurrentItem;
        private DataSourcesViewModel myCurrentCompany;
        private UserRolesViewModel myCurrentRole;
        private KursMenuItemViewModel myCurrentPermission;

        #endregion

        # region Properties

        public ObservableCollection<UsersViewModel> UserList { set; get; } =
            new ObservableCollection<UsersViewModel>();

        public ObservableCollection<KursMenuItemViewModel> PermissionsList { set; get; } =
            new ObservableCollection<KursMenuItemViewModel>();

        public ObservableCollection<DataSourcesViewModel> CompaniesList { set; get; } =
            new ObservableCollection<DataSourcesViewModel>();

        public bool IsPermissionEnable => CurrentCompany != null;

        public DataSourcesViewModel CurrentCompany
        {
            get => myCurrentCompany;
            set
            {
                if (myCurrentCompany == value)
                    return;

                myCurrentCompany = value;
                RefreshDataPermissionList();
                RaisePropertyChanged(nameof(IsPermissionEnable));
                RaisePropertyChanged();
            }
        }

        public UsersViewModel UserListCurrentItem
        {
            get => myUserListCurrentItem;
            set
            {
                if (myUserListCurrentItem == value)
                    return;

                myUserListCurrentItem = value;
                if (myUserListCurrentItem != null)
                {
                    CompaniesList.Clear();
                    using (var ctx = GlobalOptions.KursSystem())
                    {
                        var usr = ctx.Users.Include(_ => _.DataSources)
                            .FirstOrDefault(_ => _.Id == myUserListCurrentItem.Id);
                        if (usr != null)
                        {
                            foreach (var company in usr.DataSources.ToList())
                                CompaniesList.Add(new DataSourcesViewModel(company));
                            RefreshDataPermissionList();
                        }
                    }
                }

                RaisePropertyChanged();
            }
        }

        public KursMenuItemViewModel CurrentPermission
        {
            get => myCurrentPermission;
            set
            {
                if (myCurrentPermission == value)
                    return;
                myCurrentPermission = value;
                RaisePropertyChanged();
            }
        }

        public UserRolesViewModel CurrentRole

        {
            get => myCurrentRole;

            set
            {
                if (myCurrentRole == value)
                    return;
                myCurrentRole = value;
                RefreshRoleItemsList();
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<UserRolesViewModel> RoleList { get; set; } =
            new ObservableCollection<UserRolesViewModel>();

        public ObservableCollection<KursMenuItemViewModel> RoleItemsList { get; set; } =
            new ObservableCollection<KursMenuItemViewModel>();

        #endregion

        #region Methods

        private void LoadUsersData()
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                UserList.Clear();
                PermissionsList.Clear();
                CompaniesList.Clear();

                foreach (var user in ctx.Users.OrderBy(_ => _.Name).ToList()) UserList.Add(new UsersViewModel(user));

                if (CurrentCompany == null || UserListCurrentItem == null) return;
                var userdbright = ctx.UserMenuRight.Where(_ => _.DBId == CurrentCompany.Id
                                                               && _.LoginName == UserListCurrentItem.Name).ToList();
                foreach (var permission in ctx.KursMenuItem.Include(_ => _.KursMenuGroup).ToList())
                {
                    PermissionsList.Add(new KursMenuItemViewModel(permission)
                    {
                        IsSelectedItem = userdbright.FirstOrDefault(_ => _.MenuId == permission.Id) != null
                    });

                }
            }
        }

        private void LoadRoleData()
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                RoleList.Clear();
                RoleItemsList.Clear();

                foreach (var role in ctx.UserRoles.ToList()) RoleList.Add(new UserRolesViewModel(role));

                foreach (var item in ctx.KursMenuItem.Include(_ => _.KursMenuGroup).ToList())
                    RoleItemsList.Add(new KursMenuItemViewModel(item)
                    {
                        GroupName = item.KursMenuGroup?.Name,
                        IsSelectedItem = false,
                    });
            }
        }

        private void RefreshDataPermissionList()
        {
            if (UserListCurrentItem == null || CurrentCompany == null)
                return;

            using (var ctx = GlobalOptions.KursSystem())
            {
                PermissionsList.Clear();
                var userdbright = ctx.UserMenuRight.Where(_ => _.DBId == CurrentCompany.Id
                                                               && _.LoginName == UserListCurrentItem.Name).ToList();
                foreach (var permission in ctx.KursMenuItem.Include(_ => _.KursMenuGroup).ToList())
                {
                    PermissionsList.Add(new KursMenuItemViewModel(permission)
                    {
                        GroupName = permission.KursMenuGroup != null ? permission.KursMenuGroup.Name : null,
                        IsSelectedItem = userdbright.FirstOrDefault(_ => _.MenuId == permission.Id) != null
                    });

                }
            }
        }

        private void RefreshRoleItemsList()
        {
            if (CurrentRole == null) return;
            RoleItemsList.Clear();
            using (var ctx = GlobalOptions.KursSystem())
            {
                var rolePermission = ctx.UserRoles.Include(_=>_.KursMenuItem).FirstOrDefault(_ => _.id == CurrentRole.Id);

                if (rolePermission == null)
                    return;
                foreach (var i in ctx.KursMenuItem.Include(_=>_.KursMenuGroup).ToList())
                {
                    RoleItemsList.Add(new KursMenuItemViewModel(i)
                    {
                        GroupName = i.KursMenuGroup?.Name,
                        IsSelectedItem = rolePermission.KursMenuItem.FirstOrDefault(_=>_.Id == i.Id) !=null
                        
                    });

                }
            }
        }

        #endregion

        #region Commands

        public override bool IsDocumentOpenAllow => UserListCurrentItem != null;
        public override bool IsDocNewCopyAllow => UserListCurrentItem != null;

        public override void RefreshData(object obj)
        {
            LoadUsersData();
            LoadRoleData();
        }

        public override void DocumentOpen(object obj)
        {
            var ctx = new UserOptionsWindowViewModel(UserListCurrentItem.Name);
            var form = new UserOptionsWindow {DataContext = ctx};
            ctx.Form = form;
            form.ShowDialog();
        }

        public ICommand OpenWindowCreationRoleCommand
        {
            get { return new Command(openWindowCreationRole, _ => true); }
        }

        private void openWindowCreationRole(object obj)
        {
            var ctx = new RoleCreationWindowViewModel();
            var form = new RoleCreationWindow() {DataContext = ctx};
            ctx.Form = form;
            form.ShowDialog();
            if(ctx.RoleIsCreate)
                LoadRoleData();
        }

        public override void DocNewEmpty(object obj)
        {
            var ctx = new UserOptionsWindowViewModel();
            var form = new UserOptionsWindow {DataContext = ctx};
            ctx.Form = form;
            form.ShowDialog();
        }

        public override void DocNewCopy(object obj)
        {
            var ctx = new UserOptionsWindowViewModel(UserListCurrentItem.Name, true);
            var form = new UserOptionsWindow {DataContext = ctx};
            ctx.Form = form;
            form.ShowDialog();
        }

        public ICommand UpdateLinkToDocumentCommand
        {
            get { return new Command(UpdateLinkToDocument, _ => true); }
        }

        private void UpdateLinkToDocument(object obj)
        {
            if (!(obj is CellValueChangedEventArgs e)) return;
            using (var ctx = GlobalOptions.KursSystem())
            {
                if ((bool) e.Value)
                {
                    var newItem = new UserMenuRight
                    {
                        Id = Guid.NewGuid(),
                        LoginName = UserListCurrentItem.Name,
                        DBId = CurrentCompany.Id,
                        MenuId = CurrentPermission.Id,
                        IsReadOnly = false
                    };
                    ctx.UserMenuRight.Add(newItem);
                }
                else
                {
                    var linkdoc = ctx.UserMenuRight.FirstOrDefault(_ => _.LoginName == UserListCurrentItem.Name
                                                                        && _.DBId == CurrentCompany.Id &&
                                                                        _.MenuId == CurrentPermission.Id);
                    if (linkdoc != null)
                    {
                        ctx.UserMenuRight.Remove(linkdoc);
                    }
                }

                ctx.SaveChanges();
            }
        }

        public ICommand UpdateLinkKursMenuItemCommand
        {
            get
            {
                return new Command(UpdateLinkKursMenuItem, _ => CurrentRole != null);
            }
        }

        private void UpdateLinkKursMenuItem(object obj)
        {
            if (!(obj is CellValueChangedEventArgs e)) return;
            using (var ctx = GlobalOptions.KursSystem())
            {
                var role = ctx.UserRoles.Include(_ => _.KursMenuItem).FirstOrDefault(_ => _.id == CurrentRole.Id);
                var menuItem = ctx.KursMenuItem.FirstOrDefault(_ => _.Id == CurrentPermission.Id);
                if (role != null && menuItem != null)
                {
                    if ((bool)e.Value)
                    {
                        role.KursMenuItem.Add(menuItem);
                    }
                    else
                    {
                        role.KursMenuItem.Remove(menuItem);
                    }
                }
                ctx.SaveChanges();
            }
        }

        public ICommand DeleteUserCommand
        {
            get { return new Command(deleteUser, _ => (UserList.Count > 0) | (UserListCurrentItem != null)); }
        }

        private void deleteUser(object p)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var oldUserList = ctx.Users.ToList();
                foreach (var u in UserList)
                {
                    var rightUser = oldUserList.Where(_ => _.Id == u.Id)
                        .SingleOrDefault(_ => _.Id == UserListCurrentItem.Id);

                    u.IsDeleted = UserListCurrentItem.IsDeleted;
                }

                ctx.SaveChanges();
                MessageBox.Show("Пользователь изменен статус \"Удален\"");
            }
        }


        public ICommand UpdateUsersViewCommand
        {
            get { return new Command(updateUsersView, _ => true); }
        }

        private void updateUsersView(object v)
        {
            LoadUsersData();
        }

        public ICommand DeleteRoleCommand
        {
            get { return new Command(deleteRole, _ => RoleList != null); }
        }

        private void deleteRole(object p)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var deleteRole = ctx.UserRoles.Include(_=>_.KursMenuItem).First(_ => _.id == CurrentRole.Id);
                if (deleteRole == null) return;
                ctx.UserRoles.Remove(deleteRole);
                ctx.SaveChanges();
                MessageBox.Show("Роль успешно удалена.");
                LoadRoleData();
            }
        }

        #endregion
    }
}