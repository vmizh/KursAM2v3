using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

        private KursMenuItemViewModel myCurrentPermission;
        private string myNameRole;
        private string myNoteRole;
        private UserRolesViewModel myNewRole;

        #endregion

        #region Properties

        public string NameRole
        {
            get => myNameRole;
            set
            {
                if(myNameRole == value)
                    return;
                myNameRole = value;
                RaisePropertyChanged();
            }

        }

        public string NoteRole
        {
            get => myNoteRole;
            set
            {
                if(myNoteRole == value)
                    return;
                myNoteRole = value;
                RaisePropertyChanged();
            }
        }

        public UserRolesViewModel NewRole
        {
            get => myNewRole;
            set
            {
                if (myNewRole == value)
                    return;
                myNewRole = value;
                RaisePropertyChanged();
            }

        }

        public KursMenuItemViewModel CurrentPermission
        {
            get => myCurrentPermission;
            set
            {
                if(myCurrentPermission == value)
                    return;
                myCurrentPermission = value;
                RaisePropertyChanged(nameof(myCurrentPermission));
            }
        }

        public ObservableCollection<KursMenuItemViewModel> PermissionsList { get; set; } = new ObservableCollection<KursMenuItemViewModel>();

        public ObservableCollection<KursMenuItem> SelectedMenuIdItems { get; set; } = new ObservableCollection<KursMenuItem>();

        #endregion
        
        #region Method

        private void LoadDataRoleCreationWindow()
        {
            using (var ctx = new KursSystemEntities())
            {
                foreach (var item in ctx.KursMenuItem.ToList())
                {
                    PermissionsList.Add(new KursMenuItemViewModel(item)
                    {
                        IsSelectedItem = false
                    });
                }
            }
        }

        #endregion

        #region Commands

        public ICommand CreateRoleCommand
        {
            get { return new Command(createRoleCommand, _ => NameRole != null & CurrentPermission != null); }
        }

        private void createRoleCommand(object obj)
        {
            var SelectedMenuIdItems = (from permission in PermissionsList
                                        where permission.IsSelectedItem
                                        select permission.Entity).ToList();

            NewRole = new UserRolesViewModel(new UserRoles()
            {
                    id = Guid.NewGuid(),
                    Name = NameRole.Trim(),
                    Note = NoteRole.Trim(),
            });
            MessageBox.Show("Роль создана.");
            CloseWindow(Form);
        }

        public ICommand CancelCreateRoleCommand
        {
            get { return new Command(cancelCreateRole, _ => true); }
        }

        private void cancelCreateRole(object obj)
        {
            NewRole = null;
            CloseWindow(Form);
        }

    }
        #endregion
}
