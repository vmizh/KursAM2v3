using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ViewModel.Base;
using Data;

namespace KursRepositories.ViewModels
{
    public class RoleCreationWindowViewModel : RSWindowViewModelBase
    {
        #region Constructors
        
        public RoleCreationWindowViewModel()
        {
            LoadDataRoleCreationWindow();
        }

        #endregion

        #region Fields

        private UserRolesViewModel myCurrentRole;

        #endregion

        #region Properties
        
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

       
        #region Method

        private void LoadDataRoleCreationWindow()
        {
            using (var ctx = new KursSystemEntities())
            {
                foreach (var role in ctx.UserRoles.ToList())
                {
                    RoleList.Add(new UserRolesViewModel(role));
                }
            }
        }

        private void RefreshRoleItemsList()
        {

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
    }
}
