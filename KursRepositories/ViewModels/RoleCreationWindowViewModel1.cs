using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Data;
using KursDomain.Documents.Systems;


namespace KursRepositories.ViewModels
{
    public class RoleCreationWindowViewModel1 : RSWindowViewModelBase
    {
        #region Constructors
        

        public RoleCreationWindowViewModel1()
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

        public List<KursMenuItem> SelectedMenuIdItems { get; set; } = new List<KursMenuItem>();

        #endregion
        
        #region Method

        private void LoadDataRoleCreationWindow()
        {
            using (var ctx =  GlobalOptions.KursSystem())
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
            get { return new Command(CreateRole, _ => NameRole != null & NoteRole != null); }
        }

        private void CreateRole(object obj)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                SelectedMenuIdItems.Clear();
                foreach (var item in from permission in PermissionsList
                    where permission.IsSelectedItem
                    select permission.Entity)
                    SelectedMenuIdItems.Add(item);

                var newRole = new UserRoles()
                {
                    id = Guid.NewGuid(),
                    Name = NameRole.Trim(),
                    Note = NoteRole.Trim(),
                    KursMenuItem = new List<KursMenuItem>()
                };
                
                
                newRole.KursMenuItem.Clear();

                foreach (var item in SelectedMenuIdItems)
                {
                    newRole.KursMenuItem.Add(item);
                }

                ctx.UserRoles.Add(newRole);
                ctx.SaveChanges();

            }
            
           

            MessageBox.Show("Роль успешно создана.");
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
