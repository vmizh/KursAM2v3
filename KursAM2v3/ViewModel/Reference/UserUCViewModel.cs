using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.Systems;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.KursReferences.UC;

namespace KursAM2.ViewModel.Reference
{
    internal class UserUCViewModel : RSWindowViewModelBase
    {
        public UserUCViewModel()
        {
            myDataUserControl = new UsersUC();
            RefreshData(null);
            WindowName = "Пользователи";
        }

        #region properties

        public ObservableCollection<EXT_USERSViewModel> Users { set; get; }
        public ObservableCollection<EXT_USERSViewModel> SelectedUsers { set; get; }
        public List<EXT_USERSViewModel> ListSelectedUsers { set; get; } = new List<EXT_USERSViewModel>();
        private UsersUC myDataUserControl;

        public UsersUC DataUserControl
        {
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertiesChanged();
            }
            get => myDataUserControl;
        }

        private EXT_USERSViewModel myCurrentSelectedUser;

        public EXT_USERSViewModel CurrentSelectedUser
        {
            set
            {
                if (myCurrentSelectedUser != null && myCurrentSelectedUser.Equals(value)) return;
                myCurrentSelectedUser = value;
                RaisePropertyChanged();
            }
            get => myCurrentSelectedUser;
        }

        private EXT_USERSViewModel myCurrentUser;

        public EXT_USERSViewModel CurrentUser
        {
            set
            {
                if (myCurrentUser != null && myCurrentUser.Equals(value)) return;
                myCurrentUser = value;
                RaisePropertyChanged();
            }
            get => myCurrentUser;
        }

        #endregion

        #region command

        public override void RefreshData(object o)
        {
            Users = new ObservableCollection<EXT_USERSViewModel>();
            SelectedUsers = new ObservableCollection<EXT_USERSViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.EXT_USERS.ToList();
                    foreach (var i in data)
                        Users.Add(new EXT_USERSViewModel(i));
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public ICommand AddUserCommand
        {
            get { return new Command(AddUser, _ => CurrentUser != null); }
        }

        public ICommand DeleteSelectedUser
        {
            get { return new Command(DeleteSelecte, _ => CurrentSelectedUser != null); }
        }

        public void AddUser(object obj)
        {
            if (SelectedUsers.Contains(CurrentUser)) return;
            SelectedUsers.Add(CurrentUser);
            ListSelectedUsers.Add(CurrentUser);
        }

        public void DeleteSelecte(object obj)
        {
            SelectedUsers.Remove(CurrentSelectedUser);
            ListSelectedUsers.Remove(CurrentSelectedUser);
        }

        #endregion
    }
}