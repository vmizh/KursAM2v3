using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Data;
using DevExpress.Xpf.Grid;
using KursAM2.View.Repozit;
using KursDomain.Documents.Systems;
using KursDomain.Menu;
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
            CheckUserForAdmin(GlobalOptions.UserInfo.NickName);
        }

        #region Fields

        private UsersViewModel myUserListCurrentItem;
        private DataSourcesViewModel myCurrentCompany;
        private UserRolesViewModel myCurrentRole;
        private KursMenuItemViewModel myCurrentPermission;
        private bool myIsAdminUser;

        #endregion

        # region Properties

        public bool IsAdminUser
        {
            get => myIsAdminUser;
            set
            {
                if (value == myIsAdminUser) return;
                myIsAdminUser = value;
                RaisePropertyChanged();
            }
        }

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

        private void CheckUserForAdmin(string user)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var verifiedUser = ctx.Users.FirstOrDefault(_ => _.Name == user);
                if (verifiedUser != null) IsAdminUser = verifiedUser.IsAdmin;
            }
        }

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
                    PermissionsList.Add(new KursMenuItemViewModel(permission)
                    {
                        IsSelectedItem = userdbright.FirstOrDefault(_ => _.MenuId == permission.Id) != null
                    });
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
                        IsSelectedItem = false
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
                    PermissionsList.Add(new KursMenuItemViewModel(permission)
                    {
                        GroupName = permission.KursMenuGroup != null ? permission.KursMenuGroup.Name : null,
                        IsSelectedItem = userdbright.FirstOrDefault(_ => _.MenuId == permission.Id) != null
                    });
            }
        }

        private void RefreshRoleItemsList()
        {
            if (CurrentRole == null) return;
            RoleItemsList.Clear();
            using (var ctx = GlobalOptions.KursSystem())
            {
                var rolePermission = ctx.UserRoles.Include(_ => _.KursMenuItem)
                    .FirstOrDefault(_ => _.id == CurrentRole.Id);

                if (rolePermission == null)
                    return;
                foreach (var i in ctx.KursMenuItem.Include(_ => _.KursMenuGroup).ToList())
                    RoleItemsList.Add(new KursMenuItemViewModel(i)
                    {
                        GroupName = i.KursMenuGroup?.Name,
                        IsSelectedItem = rolePermission.KursMenuItem.FirstOrDefault(_ => _.Id == i.Id) != null
                    });
            }
        }

        #endregion

        #region Commands

        public override bool IsDocNewEmptyAllow => UserListCurrentItem != null && IsAdminUser;
        public override bool IsDocNewCopyAllow => UserListCurrentItem != null && IsAdminUser;

        public override bool IsDocumentOpenAllow =>
            UserListCurrentItem != null && UserListCurrentItem.Name == GlobalOptions.UserInfo.NickName || IsAdminUser;

        public override void RefreshData(object obj)
        {
            LoadUsersData();
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
            var ctx = new UserOptionsWindowViewModel(TypeChangeUser.CopyUser, UserListCurrentItem.Name);
            var form = new UserOptionsWindow {DataContext = ctx};
            ctx.Form = form;
            form.ShowDialog();
        }

        public override void DocumentOpen(object obj)
        {
            var ctx = IsAdminUser
                ? new UserOptionsWindowViewModel(TypeChangeUser.AdminUpdateUser, UserListCurrentItem.Name)
                : new UserOptionsWindowViewModel(TypeChangeUser.UserSelfUpdate, UserListCurrentItem.Name);
            var form = new UserOptionsWindow {DataContext = ctx};
            ctx.Form = form;
            form.ShowDialog();
        }

        public ICommand DeleteUserCommand
        {
            get
            {
                return new Command(DeleteUser, _ => UserList.Count > 0 && UserListCurrentItem != null && IsAdminUser);
            }
        }

        private void DeleteUser(object p)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var oldUserList = ctx.Users.ToList();
                var rightUser = oldUserList.FirstOrDefault(_ => _.Id == UserListCurrentItem.Id);
                if (rightUser != null)
                    rightUser.IsDeleted = true;

                ctx.SaveChanges();
                MessageBox.Show("Пользователю присвоен статус \"Удален\"");
                LoadUsersData();
            }
        }

        public ICommand OpenWindowCreationRoleCommand
        {
            get { return new Command(OpenWindowCreationRole, _ => IsAdminUser); }
        }

        private void OpenWindowCreationRole(object obj)
        {
            var ctx = new RoleCreationWindowViewModel();
            var form = new RoleCreationWindow {DataContext = ctx};
            ctx.Form = form;
            form.ShowDialog();
            if (ctx.RoleIsCreate)
                LoadRoleData();
        }

        public ICommand UpdateLinkToDocumentCommand
        {
            get { return new Command(UpdateLinkToDocument, _ => true); }
        }

        private void UpdateLinkToDocument(object obj)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var old = ctx.UserMenuRight.FirstOrDefault(_ => _.LoginName == UserListCurrentItem.Name
                                                                && _.MenuId == CurrentPermission.Id && _.DBId == CurrentCompany.Id);
                if (obj is CellValueChangedEventArgs o && (bool)o.Value )
                {
                    if (old == null)
                        ctx.UserMenuRight.Add(new UserMenuRight
                        {
                            Id = Guid.NewGuid(),
                            DBId = CurrentCompany.Id,
                            MenuId = CurrentPermission.Id,
                            LoginName = UserListCurrentItem.Name
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
                    if ((bool) e.Value)
                        role.KursMenuItem.Add(menuItem);
                    else
                        role.KursMenuItem.Remove(menuItem);
                }

                ctx.SaveChanges();
            }
        }

        public ICommand UpdateUsersViewCommand
        {
            get { return new Command(UpdateUsersView, _ => true); }
        }

        private void UpdateUsersView(object v)
        {
            LoadUsersData();
        }

        public ICommand DeleteRoleCommand
        {
            get { return new Command(DeleteRole, _ => RoleList.Count > 0 && IsAdminUser); }
        }

        private void DeleteRole(object p)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var deleteRole = ctx.UserRoles.Include(_ => _.KursMenuItem).First(_ => _.id == CurrentRole.Id);
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
