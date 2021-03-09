using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Data;
using KursRepositories.View;

namespace KursRepositories.ViewModels
{
    public class MainWindowPermissionsViewModel : RSWindowViewModelBase
    {
        public MainWindowPermissionsViewModel()
        {
            LoadUsersData();
            LoadRoleData();
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

        public DataSourcesViewModel CurrentCompany
        {
            get => myCurrentCompany;
            set
            {
                if(myCurrentCompany == value)
                    return;

                myCurrentCompany = value;
                RefreshDataPermissionList();
                RaisePropertyChanged();
            }
        }

        public UsersViewModel UserListCurrentItem
        {
            get => myUserListCurrentItem;
            set
            {
                if(myUserListCurrentItem == value)
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

        public ObservableCollection<UserRolesViewModel> RoleList { get; set; } = new ObservableCollection<UserRolesViewModel>();

        public ObservableCollection<KursMenuItemViewModel> RoleItemsList { get; set; } = new ObservableCollection<KursMenuItemViewModel>();
        
        #endregion

        #region Methods

        private void LoadUsersData()
        {
            using (var ctx = new KursSystemEntities())
            {
                UserList.Clear();
                PermissionsList.Clear();
                CompaniesList.Clear();
               
                foreach (var user in ctx.Users.ToList())
                {
                    UserList.Add(new UsersViewModel(user));
                }

                foreach (var permission in ctx.KursMenuItem.ToList())
                {
                    PermissionsList.Add(new KursMenuItemViewModel(permission)
                    {
                        IsSelectedItem = false
                    });
                }

                foreach (var company in ctx.DataSources.ToList())
                {
                    CompaniesList.Add(new DataSourcesViewModel(company));
                }
                
            }
        }
        private void LoadRoleData()
        {
            using (var ctx = new KursSystemEntities())
            {
                RoleList.Clear();
                foreach (var role in ctx.UserRoles.ToList())
                {
                    RoleList.Add(new UserRolesViewModel(role));
                }

            }
        }


        private void RefreshDataPermissionList()
        {
            if (UserListCurrentItem == null)
                return;
            if (CurrentCompany == null)
                return;
            
            using (var ctx = new KursSystemEntities())
            {
                var permissions = ctx.UserMenuRight.Include(_ => _.DataSources).Where(_ =>(_.LoginName == UserListCurrentItem.Name))
                        .Where(_=>(_.DBId == CurrentCompany.Id));

                foreach (var p in PermissionsList)
                {
                    var pp = permissions.FirstOrDefault(_ => _.MenuId == p.Id);
                    p.IsSelectedItem = pp != null;
                }
            }
        }

        private void RefreshRoleItemsList()
        {
            if(CurrentRole == null) return;

                RoleItemsList.Clear();
            using (var ctx = new KursSystemEntities())
            {
                var data = ctx.UserRoles.Include(_ => _.KursMenuItem).FirstOrDefault(_ => _.id == CurrentRole.Id);

                if (data == null)
                    return;

                foreach (var item in data.KursMenuItem)
                {
                    RoleItemsList.Add(new KursMenuItemViewModel(item));
                }
            }
        }
        #endregion

        #region Commands

        public ICommand OpenWindowCreationRoleCommand
        {
            get { return new Command(openWindowCreationRole, _ => true); }
        }

        private void openWindowCreationRole(object obj)
        {
            var ctx = new RoleCreationWindowViewModel();
            var form = new RoleCreationWindow {DataContext = ctx};
            ctx.Form = form;
            form.ShowDialog();
            if (ctx.NewRole != null)
                using (var context = new KursSystemEntities())
                {
                    var newRole = new UserRoles()
                    {
                        id = ctx.NewRole.Id,
                        Name = ctx.NewRole.Name,
                        Note = ctx.NewRole.Note,
                    };
                    
                    foreach (var item in ctx.SelectedMenuIdItems)
                    {
                        newRole.KursMenuItem.Add(item);
                        context.SaveChanges();
                    }
                    context.UserRoles.Add(newRole);
                    RoleList.Add(new UserRolesViewModel(newRole));
                    context.SaveChanges();
                }
        }


        public ICommand OpenWindowCreationUserCommand
        {
            get { return new Command(openWindowCreationUser, _ => true); }
        }

        private void openWindowCreationUser(object obj)
        {
            var ctx = new UserCreationWindowViewModel();
            var form = new UserCreationWindow {DataContext = ctx};
            ctx.Form = form;
            form.ShowDialog();
            if (ctx.NewUser != null)
            {
                using (var context = new KursSystemEntities())
                {
                    var newUser = new Users 
                    {
                        Id = ctx.NewUser.Id,
                        Name = ctx.LoginName.Trim(),
                        FullName = ctx.FullName.Trim(),
                        Note = ctx.Note,
                        ThemeName = ctx.ThemeName,
                        IsAdmin = ctx.Admin,
                        IsTester = ctx.Tester,
                        IsDeleted = ctx.Deleted,
                        Avatar = ctx.Avatar
                    };
                    context.Users.Add(newUser);
                    UserList.Add(new UsersViewModel(newUser));
                    context.SaveChanges();
                }
            }

        }

        public ICommand DeleteUserCommand
        {
            get { return new Command(deleteUser, _ => UserList.Count > 0 | UserListCurrentItem != null); }
        }

        private void deleteUser(object p)
        {
            
            using (var ctx = new KursSystemEntities())
            {
                var oldUserList = ctx.Users.ToList();
                foreach (var u in UserList)
                {
                    var rightUser = oldUserList.Where(_=>_.Id == u.Id).SingleOrDefault(_ => _.Id == UserListCurrentItem.Id);

                    u.IsDeleted = rightUser != null;
                }
                ctx.SaveChanges();
                MessageBox.Show("Пользователь присвоен статус \"Удален\"");
            }
        }

        public ICommand SaveChangesInUsersGridControlCommand
        {
            get { return new Command(saveChangesInUsersGridControl, _ => true); }
        }

        private void saveChangesInUsersGridControl(object obj)
        {
            using (var ctx = new KursSystemEntities())
            {
                var oldUserList = ctx.Users.ToList();
                foreach (var usr in UserList)
                {
                    var rightUser = oldUserList.SingleOrDefault(_ => _.Id == usr.Id);

                    if (rightUser != null)
                    {
                        rightUser.Id = usr.Id;
                        rightUser.Name = usr.Name;
                        rightUser.FullName = usr.FullName;
                        rightUser.Note = usr.Note;
                        rightUser.Avatar = usr.Avatar;
                        rightUser.IsAdmin = usr.IsAdmin;
                        rightUser.IsTester = usr.IsTester;
                        rightUser.IsDeleted = usr.IsDeleted;
                    }
                }
                ctx.SaveChanges();
                MessageBox.Show("Данные сохранены.");
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
            using (var ctx = new KursSystemEntities())
            {
                var deleteRole = ctx.UserRoles.First(_ => _.id == CurrentRole.Id);
                if (deleteRole == null) return;
                ctx.UserRoles.Remove(deleteRole);
                ctx.SaveChanges();
                MessageBox.Show("Роль успешно удалена.");
            }
        }

        #endregion
    }
}
