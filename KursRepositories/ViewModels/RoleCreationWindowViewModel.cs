using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Core.ViewModel.Base;
using Data;
using DevExpress.Xpf.Editors;
using DevExpress.Mvvm;
using DevExpress.Office.Utils;

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
            var menuIdList = (from permission in PermissionsList
                    where permission.IsSelectedItem
                    select permission).ToList();
            MessageBox.Show(string.Join(Environment.NewLine, menuIdList));

                RoleList.Add(new UserRolesViewModel(new UserRoles
                {
                    id = Guid.NewGuid(),
                    Name = NameRoleTextEdit.Trim(),
                    Note = NoteRoleTextEdit.Trim(),
                    
                    KursMenuItem = new List<KursMenuItem>()
                }));

                /*
                using (var context = new KursSystemEntities())
                { 
                    var oldRolesList = context.UserRoles.ToList();

                    foreach (var role in RoleList)
                    {
                        var propertyRoles = oldRolesList.FirstOrDefault(_ => _.id == role.Entity.id);
                        if (propertyRoles != null)
                            continue;
                        else
                        {
                            context.UserRoles.Add(new UserRoles()
                            {
                                id = role.Entity.id,
                                Name = role.Name,
                                Note = role.Note,
                                KursMenuItem = role.Entity.KursMenuItem
                            });
                        }
                    }

                    context.SaveChanges();
                    MessageBox.Show("Новая роль создана. Проверьте список ролей.");
                    */

                }
        }
        
        #endregion
    }

}
