using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.Systems;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using DevExpress.Xpf.Grid;
using KursRepositories.View;

namespace KursRepositories.ViewModels
{
    public sealed class MainWindowPermissionsViewModel : RSWindowViewModelBase
    {
        public MainWindowPermissionsViewModel()
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
            new();

        public ObservableCollection<KursMenuItemViewModel> PermissionsList { set; get; } =
            new();

        public ObservableCollection<DataSourcesViewModel> CompaniesList { set; get; } =
            new();

        public bool IsPermissionEnable => CurrentCompany != null;


        private KursMenuItemViewModel myCurrentMenuItem;

        public KursMenuItemViewModel CurrentMenuItem
        {
            get => myCurrentMenuItem;
            set
            {
                if (myCurrentMenuItem == value) return;
                myCurrentMenuItem = value;
                RaisePropertyChanged();
            }
        }

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
                RefreshDataPermissionList();
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
            new();

        public ObservableCollection<KursMenuItemViewModel> RoleItemsList { get; set; } =
            new();

        #endregion

        #region Methods

        private void LoadUsersData()
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                UserList.Clear();
                PermissionsList.Clear();
                CompaniesList.Clear();

                foreach (var user in ctx.Users.ToList()) UserList.Add(new UsersViewModel(user));

                foreach (var permission in ctx.KursMenuItem.ToList())
                    PermissionsList.Add(new KursMenuItemViewModel(permission)
                    {
                        IsSelectedItem = false
                    });

                foreach (var company in ctx.DataSources.ToList()) CompaniesList.Add(new DataSourcesViewModel(company));
            }
        }

        private void LoadRoleData()
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                RoleList.Clear();
                RoleItemsList.Clear();

                foreach (var role in ctx.UserRoles.ToList()) RoleList.Add(new UserRolesViewModel(role));

                foreach (var item in ctx.KursMenuItem.ToList())
                    RoleItemsList.Add(new KursMenuItemViewModel(item)
                    {
                        IsSelectedItem = false
                    });
            }
        }

        private void RefreshDataPermissionList()
        {
            if (UserListCurrentItem == null)
                return;
            if (CurrentCompany == null)
                return;

            using (var ctx = GlobalOptions.KursSystem())
            {
                var permissions = ctx.UserMenuRight.Include(_ => _.DataSources)
                    .Where(_ => _.LoginName == UserListCurrentItem.Name)
                    .Where(_ => _.DBId == CurrentCompany.Id);

                foreach (var p in PermissionsList)
                {
                    var pp = permissions.FirstOrDefault(_ => _.MenuId == p.Id);
                    p.IsSelectedItem = pp != null;
                }
            }
        }

        private void RefreshRoleItemsList()
        {
            if (CurrentRole == null) return;

            using (var ctx = GlobalOptions.KursSystem())
            {
                var userPermission = ctx.UserRoles.Include(_ => _.KursMenuItem)
                    .FirstOrDefault(_ => _.id == CurrentRole.Id);

                if (userPermission == null)
                    return;

                foreach (var v in RoleItemsList)
                {
                    var rPermission = userPermission.KursMenuItem.FirstOrDefault(_ => _.Id == v.Id);
                    v.IsSelectedItem = rPermission != null;
                }
            }
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            LoadUsersData();
            LoadRoleData();
        }

        public ICommand EditUserCommand
        {
            get { return new Command(EditUser, _ => UserListCurrentItem != null); }
        }

        private void EditUser(object obj)
        {
            var ctx = new UserCreationWindowViewModel(UserListCurrentItem.Name);
            var form = new UserCreationWindow { DataContext = ctx };
            ctx.Form = form;
            form.ShowDialog();
        }

        public ICommand OpenWindowCreationRoleCommand
        {
            get { return new Command(openWindowCreationRole, _ => true); }
        }

        private void openWindowCreationRole(object obj)
        {
            var ctx = new RoleCreationWindowViewModel1();
            var form = new RoleCreationWindow { DataContext = ctx };
            ctx.Form = form;
            form.ShowDialog();
            /*if (ctx.NewRole != null)
                using (var context = GlobalOptions.KursSystem())
                {
                    var newRole = new UserRoles()
                    {
                        id = ctx.NewRole.Id,
                        Name = ctx.NewRole.Name,
                        Note = ctx.NewRole.Note,
                        KursMenuItem = new List<KursMenuItem>()
                    };
                    
                    context.SaveChanges();
                    newRole.KursMenuItem.Clear();
                    foreach (var item in ctx.SelectedMenuIdItems)
                    {
                        newRole.KursMenuItem.Add(item);
                    }
                    
                    context.UserRoles.Add(newRole);
                    RoleList.Add(new UserRolesViewModel(newRole));
                    context.SaveChanges();
                }*/
        }

        public ICommand UpdateLinkKursMenuItemCommand
        {
            get { return new Command(UpdateLinkKursMenuItem, _ => CurrentRole != null); }
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
                        role.KursMenuItem.Add(menuItem);
                    else
                        role.KursMenuItem.Remove(menuItem);
                }

                ctx.SaveChanges();
            }
        }

        public ICommand OpenWindowCreationUserCommand
        {
            get { return new Command(openWindowCreationUser, _ => true); }
        }

        private void openWindowCreationUser(object obj)
        {
            var ctx = new UserCreationWindowViewModel();
            var form = new UserCreationWindow { DataContext = ctx };
            ctx.Form = form;
            form.ShowDialog();
            // Чистой воды перепрограммирование
            //if (ctx.NewUser != null)
            //    using (var context = GlobalOptions.KursSystem())
            //    {
            //        var newUser = new Users
            //        {
            //            Id = ctx.NewUser.Id,
            //            Name = ctx.LoginName.Trim(),
            //            FullName = ctx.FullName.Trim(),
            //            Note = ctx.Note,
            //            ThemeName = ctx.ThemeName,
            //            IsAdmin = ctx.Admin,
            //            IsTester = ctx.Tester,
            //            IsDeleted = ctx.Deleted,
            //            Avatar = ctx.Avatar
            //        };

            //        context.Users.Add(newUser);
            //        UserList.Add(new UsersViewModel(newUser));
            //        context.SaveChanges();
            //    }
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

                    u.IsDeleted = rightUser != null;
                }

                ctx.SaveChanges();
                MessageBox.Show("Пользователь присвоен статус \"Удален\"");
            }
        }

        public ICommand PermissionChangeCommand
        {
            get { return new Command(PermissionChange, _ => true); }
        }

        private void PermissionChange(object obj)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var old = ctx.UserMenuRight.FirstOrDefault(_ => _.LoginName == GlobalOptions.UserInfo.NickName
                && _.MenuId == CurrentMenuItem.Id && _.DBId == CurrentCompany.Id);
                if (CurrentMenuItem.IsSelectedItem)
                {
                    if (old == null)
                        ctx.UserMenuRight.Add(new UserMenuRight
                        {
                            Id = Guid.NewGuid(),
                            DBId = CurrentCompany.Id,
                            MenuId = CurrentMenuItem.Id,
                            LoginName = GlobalOptions.UserInfo.NickName
                        });
                }
                else
                {
                    if (old != null)
                        ctx.UserMenuRight.Remove(old);
                }

                ctx.SaveChanges();

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
                var deleteRole = ctx.UserRoles.First(_ => _.id == CurrentRole.Id);
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