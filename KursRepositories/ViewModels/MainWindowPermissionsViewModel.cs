using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Core.ViewModel.Base;
using Data;
using DevExpress.ExpressApp.Validation.AllContextsView;
using DevExpress.Xpf.Editors;

namespace KursRepositories.ViewModels
{


    public class MainWindowPermissionsViewModel : RSViewModelBase
    {
        #region Fields

        private UsersViewModel myUserListCurrentItem;
        private DataSourcesViewModel myEditValueComboboxCompany;

        #endregion


        public MainWindowPermissionsViewModel()
        {
            LoadView();
        }

        # region Properties

        public ObservableCollection<UsersViewModel> UserList { set; get; } =
            new ObservableCollection<UsersViewModel>();

        public ObservableCollection<WrapKursMenuItemViewModel> PermissionsList { set; get; } =
            new ObservableCollection<WrapKursMenuItemViewModel>();

        public ObservableCollection<DataSourcesViewModel> CompaniesList { set; get; } =
            new ObservableCollection<DataSourcesViewModel>();

        public string EditValueComboboxCompany
        {
            get => myEditValueComboboxCompany.ShowName;
            set
            {
                if(myEditValueComboboxCompany.ShowName == value)
                    return;
                myEditValueComboboxCompany.ShowName = value;
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

        #endregion

        #region Methods

        public void LoadView()
        {
            using (var ctx = new KursSystemEntities())
            {
                foreach (var user in ctx.Users.ToList())
                {
                    UserList.Add(new UsersViewModel(user));
                }

                foreach (var permission in ctx.KursMenuItem.ToList())
                {
                    PermissionsList.Add(new WrapKursMenuItemViewModel(permission));
                }

                foreach (var company in ctx.DataSources.ToList())
                {
                    CompaniesList.Add(new DataSourcesViewModel(company));
                }

            }
        }

        public void RefreshDataPermissionList()
        {
            if (UserListCurrentItem == null && EditValueComboboxCompany == null)
                return;
            
            using (var ctx = new KursSystemEntities())
            {
                var permissions = ctx.UserMenuRight.Include(_ => _.DataSources).Where(_ => _.LoginName == UserListCurrentItem.Name)
                    .ToList();

                foreach (var p in PermissionsList)
                {
                    var pp = permissions.FirstOrDefault(_ => _.MenuId == p.Id);
                    p.IsSelectedItem = pp != null;
                }

            }

            #endregion
        }
    }
}
