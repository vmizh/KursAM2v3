using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Data;
using DevExpress.Map.Kml;
using DevExpress.XtraExport.Helpers;

namespace KursRepositories.ViewModels
{
    public class RoleCreationWindowViewModel : RSViewModelBase
    {
        #region Constructors
        

        public RoleCreationWindowViewModel()
        {
            LoadDataRoleCreationWindow();
            
        }

        #endregion

        #region Fields

        private UserRolesViewModel myCurrentRole;
        private KursMenuItemViewModel myCurrentPermission;
        private string myNameRoleTextEdit;
        private string myNoteRoleTextEdit;

        #endregion

        #region Properties

        public string NameRoleTextEdit
        {
            get => myNameRoleTextEdit;
            set
            {
                if(myNameRoleTextEdit == value)
                    return;
                myNameRoleTextEdit = value;
                RaisePropertyChanged();
            }

        }

        public string NoteRoleTextEdit
        {
            get => myNoteRoleTextEdit;
            set
            {
                if(myNoteRoleTextEdit == value)
                    return;
                myNoteRoleTextEdit = value;
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

        public KursMenuItemViewModel CurrentPermission
        {
            get => myCurrentPermission;
            set
            {
                if(myCurrentPermission == value)
                    return;
                myCurrentPermission = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<UserRolesViewModel> RoleList { get; set; } = new ObservableCollection<UserRolesViewModel>();

        public ObservableCollection<KursMenuItemViewModel> RoleItemsList { get; set; } = new ObservableCollection<KursMenuItemViewModel>();

        public ObservableCollection<KursMenuItemViewModel> PermissionsList { get; set; } = new ObservableCollection<KursMenuItemViewModel>();

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

                foreach (var item in ctx.KursMenuItem.ToList())
                {
                    PermissionsList.Add(new KursMenuItemViewModel(item)
                    {
                        IsSelectedItem = false
                    });
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

        #region Commands

        public ICommand CreateRoleCommand
        {
            get { return new Command(createRoleCommand, _ => NameRoleTextEdit != null); }
        }

       
        private void createRoleCommand(object obj)
        {
            try
            {
                var menuIdList = (from permission in PermissionsList
                    where permission.IsSelectedItem
                    select permission.Entity).ToList();


                RoleList.Add(new UserRolesViewModel(new UserRoles
                {
                    id = Guid.NewGuid(),
                    Name = NameRoleTextEdit.Trim(),
                    Note = NoteRoleTextEdit.Trim(),
                    KursMenuItem = menuIdList

                }));

                using (var ctx = new KursSystemEntities())
                {
                    var oldRolesList = ctx.UserRoles.ToList();

                    foreach (var role in RoleList)
                    {
                        var propertyRoles = oldRolesList.FirstOrDefault(_ => _.id == role.Id);
                        if (propertyRoles != null)
                        {
                            continue;
                        }
                        else
                        {
                            ctx.UserRoles.Add(new UserRoles()
                            {
                                id = role.Id,
                                Name = role.Name,
                                Note = role.Note,
                                KursMenuItem = role.Itemset
                            });
                        }
                    }

                    ctx.SaveChanges();
                    MessageBox.Show("Новая роль создана. Проверьте список ролей.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }


        }
        
        #endregion
    }

}
