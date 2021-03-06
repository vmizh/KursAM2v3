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

        private UserRolesViewModel myCurrentRole;
        private KursMenuItemViewModel myCurrentPermission;
        private string myNameRole;
        private string myNoteRole;

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

        public ObservableCollection<KursMenuItemViewModel> PermissionsList { get; set; } = new ObservableCollection<KursMenuItemViewModel>();

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
            get { return new Command(createRoleCommand, _ => NameRole != null); }
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
                    Name = NameRole.Trim(),
                    Note = NoteRole.Trim(),
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
