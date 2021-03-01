using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

        public ObservableCollection<UsersViewModel> DeletedUsers { get; set; } =
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

        public ICommand AddUserCommand
        {
            get { return new Command(addUser, _ => true); }
        }

        private void addUser(object obj)
        {
            var ctx = new UserCreationWindowViewModel();
            var form = new UserCreationWindow {DataContext = ctx};
            form.Show();
        }

        public ICommand DeleteUserCommand
        {
            get { return new Command(deleteUser, _ => UserList.Count > 0); }
        }

        private void deleteUser(object p)
        {
            if (UserListCurrentItem == null)
                return;
            DeletedUsers.Add(UserListCurrentItem);

            using (var ctx = new KursSystemEntities())
            {
                foreach (var u in UserList)
                {
                    var delUser = DeletedUsers.FirstOrDefault(_ => _.Id == u.Id);
                    u.IsDeleted = delUser != null;
                }

                ctx.SaveChanges();
                MessageBox.Show("Пользователю присвоен статус \"Удалён.\"");
            }

        }

        #endregion
    }
}
