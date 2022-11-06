using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Data;
using KursDomain;
using KursDomain.Documents.Systems;
using KursRepositories.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace KursAM2.ViewModel.Repozit
{
    class RoleCreationWindowViewModel : RSWindowViewModelBase
    {
        #region Constructors


        public RoleCreationWindowViewModel()
        {
            LoadDataRoleCreationWindow();
            WindowName = "Создание новой роли";

        }

        #endregion

        #region Fields

        private KursMenuItemViewModel myCurrentPermission;
        private string myNameRole;
        private string myNoteRole;
        private UserRolesViewModel myNewRole;
        
        #endregion

        #region Properties

        public bool RoleIsCreate { get; set; }

        public string NameRole
        {
            get => myNameRole;
            set
            {
                if (myNameRole == value)
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
                if (myNoteRole == value)
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
                if (myCurrentPermission == value)
                    return;
                myCurrentPermission = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<KursMenuItemViewModel> PermissionsList { get; set; } = new ObservableCollection<KursMenuItemViewModel>();


        #endregion

        #region Method

        private void LoadDataRoleCreationWindow()
        {
            using (var ctx = GlobalOptions.KursSystem())
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
                
                var newRole = new UserRoles()
                {
                    id = Guid.NewGuid(),
                    Name = NameRole.Trim(),
                    Note = NoteRole.Trim(),
                };
                ctx.UserRoles.Add(newRole);
                
                foreach (var item in PermissionsList) 
                    if (item.IsSelectedItem)
                    {
                        var selectedPermissions = ctx.KursMenuItem.Include(_ => _.UserRoles)
                            .FirstOrDefault(_ => _.Id == item.Id);
                        selectedPermissions?.UserRoles.Add(newRole);
                    }
                
                ctx.SaveChanges();
                RoleIsCreate = true;
                MessageBox.Show("Роль успешно создана.");
                CloseWindow(Form);
            }

        }

        public ICommand CancelCreateRoleCommand
        {
            get { return new Command(cancelCreateRole, _ => true); }
        }

        private void cancelCreateRole(object obj)
        {
            RoleIsCreate = false;
            CloseWindow(Form);
        }

    }
    #endregion
}

