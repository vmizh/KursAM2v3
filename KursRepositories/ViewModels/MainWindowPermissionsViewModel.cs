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
            LoadView();
        }

        #region Fields

                private UsersViewModel myUserListCurrentItem;
                private DataSourcesViewModel myEditValueComboboxCompany;
                

        #endregion

        # region Properties

        public ObservableCollection<UsersViewModel> UserList { set; get; } =
            new ObservableCollection<UsersViewModel>();

        
        public ObservableCollection<KursMenuItemViewModel> PermissionsList { set; get; } =
            new ObservableCollection<KursMenuItemViewModel>();

        public ObservableCollection<DataSourcesViewModel> CompaniesList { set; get; } =
            new ObservableCollection<DataSourcesViewModel>();

        public DataSourcesViewModel EditValueComboboxCompany
        {
            get => myEditValueComboboxCompany;
            set
            {
                if(myEditValueComboboxCompany == value)
                    return;

                myEditValueComboboxCompany = value;
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
        
        #endregion

        #region Methods

        private void LoadView()
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
                        IsSelectedItem = true
                    });
                }

                foreach (var company in ctx.DataSources.ToList())
                {
                    CompaniesList.Add(new DataSourcesViewModel(company));
                }

            }
        }

        private void RefreshDataPermissionList()
        {
            if (UserListCurrentItem == null)
                return;
            if(EditValueComboboxCompany == null)
                return;
            
            using (var ctx = new KursSystemEntities())
            {
                var permissions = ctx.UserMenuRight.Include(_ => _.DataSources).Where(_ =>(_.LoginName == UserListCurrentItem.Name))
                        .Where(_=>(_.DBId == EditValueComboboxCompany.Id));

                foreach (var p in PermissionsList)
                {
                    var pp = permissions.FirstOrDefault(_ => _.MenuId == p.Id);
                    p.IsSelectedItem = pp != null;
                }
            }
        }
        #endregion

        #region Commands

        public ICommand OpenRoleCreationWindowCommand
        {
            get { return new Command(openRoleCreationWindow, _ => true); }
        }

        private void openRoleCreationWindow(object obj)
        {
            var ctx = new RoleCreationWindowViewModel();
            var form = new RoleCreationWindow{ DataContext = ctx };
            form.Show();
        }

        public ICommand OpenWindowCreationUserCommand
        {
            get { return new Command(operWindowCreationUser, _ => true); }
        }

        private void operWindowCreationUser(object obj)
        {
            var ctx = new UserCreationWindowViewModel();
            var form = new UserCreationWindow {DataContext = ctx};
            ctx.Form = form;
            form.ShowDialog();
            if (ctx.NewUser != null)
            {
                using (var context = new KursSystemEntities())
                {
                    var newUser = new Users //Добавляю пользователя в список UserList в главной моделе
                    {
                        Id = Guid.NewGuid(),
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

        public ICommand SaveChangesInDatagridCommand
        {
            get { return new Command(saveChangesInDatagrid, _ => true); }
        }

        private void saveChangesInDatagrid(object obj)
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

        #endregion
    }
}
