﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Spreadsheet;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.XtraSpreadsheet.Export;
using KursRepositories.ViewModels;

namespace KursAM2.ViewModel.Repozit
{
    public sealed class UserOptionsWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Constructor

        public UserOptionsWindowViewModel()
        {
            LoadRegisteredUsers();
            LoadDataSourceAndRoleList();
            IsNewUser = true;
        }

        public UserOptionsWindowViewModel(string loginName) : this()
        {
            if (!string.IsNullOrWhiteSpace(loginName)) 
                IsNewUser = false;
            LoadExistingUser(loginName);
        }

        public UserOptionsWindowViewModel(string loginName, bool isCopy = false) : this(loginName)
        {
            if (!string.IsNullOrWhiteSpace(loginName)) 
                IsNewUser = true;
            LoadExistingUser(loginName);
            LoginName = null;
            FullName = null;
        }

        #endregion


        #region Fields

        private Guid userId = Guid.Empty;
        private string myFirstName;
        private string myMiddleName;
        private string myLastName;
        private string myFullName;
        private string myLoginName;
        private bool myAdmin;
        private bool myTester;
        private byte[] myAvatar;
        private string myThemeName = "MetropolisLight";
        private new string myNote;
        private Users myNewUser;
        private List<string> _myRegisteredUsersNames = new List<string>();

        private ObservableCollection<DataSourcesViewModel> myCompanies =
            new ObservableCollection<DataSourcesViewModel>();

        private ObservableCollection<UserRolesViewModel> myRoles = new ObservableCollection<UserRolesViewModel>();

        private ObservableCollection<DataSourcesViewModel> myIsSelectedCompanies =
            new ObservableCollection<DataSourcesViewModel>();

        private ObservableCollection<UserRolesViewModel> myIsSelectedRoles =
            new ObservableCollection<UserRolesViewModel>();

        private DataSourcesViewModel myCurrentCompany;
        private UserRolesViewModel myCurrentRole;
        private string myPassword;
        private string myPasswordConfirm;
        private bool myIsLoginEnable;
        private bool myIsNewUser;

        #endregion

        #region Properties

        public override string WindowName => IsNewUser
            ? "Создание нового пользователя"
            : $"Изменение пользователя {LoginName} ({FullName})";

        public bool IsNewUser
        {
            get => myIsNewUser;
            set
            {
                if (myIsNewUser == value)
                    return;
                myIsNewUser = value;
                IsLoginEnable = myIsNewUser;
                RaisePropertyChanged();
            }
        }

        public bool IsLoginEnable
        {
            get => myIsLoginEnable;
            set
            {
                if (myIsLoginEnable == value)
                    return;
                myIsLoginEnable = value;
                RaisePropertyChanged();
            }
        }

        public List<string> RegisteredUsersNames
        {
            get => _myRegisteredUsersNames;
            set
            {
                if (_myRegisteredUsersNames == value)
                    return;
                _myRegisteredUsersNames = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<DataSourcesViewModel> IsSelectedCompanies
        {
            get => myIsSelectedCompanies;
            set
            {
                if (myIsSelectedCompanies == value)
                    return;
                myIsSelectedCompanies = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<DataSourcesViewModel> Companies
        {
            get => myCompanies;
            set
            {
                if (myCompanies == value)
                    return;
                myCompanies = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<UserRolesViewModel> IsSelectedRoles
        {
            get => myIsSelectedRoles;
            set
            {
                if (myIsSelectedRoles == value)
                    return;
                myIsSelectedRoles = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<UserRolesViewModel> Roles
        {
            get => myRoles;
            set
            {
                if (myRoles == value)
                    return;
                myRoles = value;
                RaisePropertyChanged();
            }
        }

        public DataSourcesViewModel CurrentCompany
        {
            get => myCurrentCompany;
            set
            {
                if (myCurrentCompany == value)
                    return;
                myCurrentCompany = value;
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
                RaisePropertyChanged();
            }
        }

        public string Password
        {
            get => myPassword;
            set
            {
                if (myPassword == value) return;
                myPassword = value;
                RaisePropertyChanged();
            }
        }

        public string PasswordConfirm
        {
            get => myPasswordConfirm;
            set
            {
                if (myPasswordConfirm == value) return;
                myPasswordConfirm = value;
                RaisePropertyChanged();
            }
        }

        public string Error { get; private set; }

        public Users NewUser
        {
            get => myNewUser;
            set
            {
                if (myNewUser == value)
                    return;
                myNewUser = value;
                RaisePropertyChanged();
            }
        }

        public string FirstName
        {
            get => myFirstName;
            set
            {
                if (myFirstName == value)
                    return;
                myFirstName = value;
                myFullName = $"{LastName} {FirstName} {MiddleName}";
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FullName));
            }
        }

        public string MiddleName
        {
            get => myMiddleName;
            set
            {
                if (myMiddleName == value)
                    return;
                myMiddleName = value;
                myFullName = $"{LastName} {FirstName} {MiddleName}";
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FullName));
            }
        }

        public string LastName
        {
            get => myLastName;
            set
            {
                if (myLastName == value)
                    return;
                myLastName = value;
                myFullName = $"{LastName} {FirstName} {MiddleName}";
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FullName));
            }
        }

        public string FullName
        {
            get => $"{myLastName} {myFirstName} {myMiddleName}";
            set
            {
                if (myFullName == value)
                    return;
                myFullName = value;
                setNamesFromFullName(value);
                RaisePropertyChanged();
            }
        }

        public bool Admin
        {
            get => myAdmin;
            set
            {
                if (myAdmin == value)
                    return;
                myAdmin = value;
                RaisePropertyChanged();
            }
        }

        public string LoginName
        {
            get => myLoginName;
            set
            {
                if (myLoginName == value)
                    return;
                myLoginName = value;
                RaisePropertyChanged();
            }
        }

        public bool Tester
        {
            get => myTester;
            set
            {
                if (myTester == value)
                    return;
                myTester = value;
                RaisePropertyChanged();
            }
        }

        public bool Deleted { get; set; }

        public byte[] Avatar
        {
            get => myAvatar;
            set
            {
                if (myAvatar == value)
                    return;
                myAvatar = value;
                RaisePropertyChanged();
            }
        }

        public string ThemeName
        {
            get => myThemeName;
            set
            {
                if (myThemeName == value)
                    return;
                myThemeName = value;
                RaisePropertyChanged();
            }
        }

        public override string Note
        {
            get => myNote;
            set
            {
                if (myNote == value)
                    return;
                myNote = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Command

        public ICommand AdminStatusChangeCommand
        {
            get { return new Command(AdminStatusChange, _ => true); }
        }

        private void AdminStatusChange(object obj)
        {
            if (string.IsNullOrWhiteSpace(LoginName)) return;
            if (obj is EditValueChangingEventArgs e)
            {
                if ((bool) e.NewValue)
                {
                    using (var ctx = GlobalOptions.KursSystem())
                    {
                        var sql = $"ALTER SERVER ROLE [sysadmin] ADD MEMBER [{LoginName}]; "
                                  + $"ALTER SERVER ROLE [securityadmin] ADD MEMBER [{LoginName}]; ";
                        ctx.Database.ExecuteSqlCommand(sql);

                    }
                }
                else
                {
                    using (var ctx = GlobalOptions.KursSystem())
                    {
                        var sql = $"ALTER SERVER ROLE [sysadmin] DROP MEMBER [{LoginName}]; "
                                  + $"ALTER SERVER ROLE [securityadmin] DROP MEMBER [{LoginName}]; ";
                        ctx.Database.ExecuteSqlCommand(sql);
                    }
                }
            }
        }

        public ICommand UpdateLinkDataSourceCommand
        {
            get
            {
                return new Command(UpdateLinkDataSource, _ => CurrentCompany != null);
            }
        }

        private void UpdateLinkDataSource(object obj)
        {
            if(!(obj is CellValueChangedEventArgs e)) return;
            using (var ctx = GlobalOptions.KursSystem())
            {
                var usr = ctx.Users.Include(_ => _.DataSources).FirstOrDefault(_ => _.Id == userId);
                var ds = ctx.DataSources.FirstOrDefault(_ => _.Id == CurrentCompany.Id);
                if (usr != null && ds != null)
                {
                    if ((bool) e.Value)
                    {
                        usr.DataSources.Add(ds);
                        List<int> dbcount = ctx.Database.SqlQuery<int>($"SELECT count(*) FROM sys.databases d WHERE d.name = '{ds.DBName}'").ToList();
                        if (dbcount[0] > 0)
                        {
                            var usrsql = $"USE [{ds.DBName}] " +
                                         $"if  (select  count(*) from sys.database_principals where name = '{LoginName}') = 0 " +
                                         "begin " +
                                         $"CREATE USER {LoginName} FOR LOGIN {LoginName} WITH DEFAULT_SCHEMA = [{LoginName}] " +
                                         "end " +
                                         $"if (SELECT COUNT(*) FROM EXT_USERS WHERE USR_NICKNAME ='{LoginName}' ) = 0 " +
                                         "BEGIN " +
                                         "DECLARE @maxId int; " +
                                         "SELECT @maxId = isnull(MAX(USR_ID),1)+1 FROM EXT_USERS; " +
                                         "INSERT INTO dbo.EXT_USERS (USR_ID,USR_NICKNAME,USR_FULLNAME,TABELNUMBER, " +
                                         "USR_PHONE  ,USR_NOTES  ,USR_PASSWORD  ,USR_PROVODKY  ,USR_ABORT_CONNECT) " +
                                         $"VALUES ( @maxId, '{LoginName}','{FullName}',null,null,null,null,null,null ); " +
                                         "END;";
                            ctx.Database.ExecuteSqlCommand(usrsql);
                        }
                    }
                    else
                    {
                        usr.DataSources.Remove(ds);
                    }
                }
                ctx.SaveChanges();
            }
        }

        public override void SaveData(object data)
        {
            if (IsNewUser)
            {
                if (!string.IsNullOrWhiteSpace(LoginName) && !string.IsNullOrWhiteSpace(Password) &&
                    !string.IsNullOrWhiteSpace(PasswordConfirm)
                    && Password == PasswordConfirm)
                    try
                    {
                        using (var ctx = GlobalOptions.KursSystem())
                        {
                            var sql = "USE MASTER " +
                                      $"CREATE LOGIN {LoginName} WITH PASSWORD=N'{Password}', " +
                                      "DEFAULT_DATABASE = MASTER, " +
                                      "DEFAULT_LANGUAGE = US_ENGLISH " +
                                      $"ALTER LOGIN {LoginName} ENABLE ";
                            ctx.Database.ExecuteSqlCommand(sql);
                            foreach (var c in Companies.Where(_ => _.IsSelectedItem))
                            {
                                var usrsql = $"USE [{c.DBName}] " +
                                             $"CREATE USER {LoginName} FOR LOGIN {LoginName} WITH DEFAULT_SCHEMA = [{LoginName}] " +
                                             $"if (SELECT COUNT(*) FROM EXT_USERS WHERE USR_NICKNAME ='{LoginName}' ) = 0" +
                                             "BEGIN " +
                                             "DECLARE @maxId int; " +
                                             "SELECT @maxId =isnull(MAX(USR_ID),1) + 1 FROM EXT_USERS; " +
                                             "INSERT INTO dbo.EXT_USERS (USR_ID,USR_NICKNAME,USR_FULLNAME,TABELNUMBER, " +
                                             "USR_PHONE  ,USR_NOTES  ,USR_PASSWORD  ,USR_PROVODKY  ,USR_ABORT_CONNECT) " +
                                             $"VALUES ( @maxId, '{LoginName}','{FullName}',null,null,null,null,null,null );" +
                                             "END;";
                                ctx.Database.ExecuteSqlCommand(usrsql);
                            }

                            var newUser = new Users
                            {
                                Id = Guid.NewGuid(),
                                Name = LoginName.Trim(),
                                FullName = FullName.Trim(),
                                Note = Note,
                                ThemeName = ThemeName,
                                IsAdmin = Admin,
                                IsTester = Tester,
                                IsDeleted = Deleted,
                                Avatar = Avatar
                            };
                            ctx.Users.Add(newUser);

                            foreach (var c in Companies)
                                if (c.IsSelectedItem)
                                {
                                    var old = ctx.DataSources.Include(_ => _.Users).FirstOrDefault(_ => _.Id == c.Id);
                                    old?.Users.Add(newUser);
                                }

                            ctx.SaveChanges();
                        }
                        IsNewUser = false;
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                    }
            }
            else
            {
                using (var ctx = GlobalOptions.KursSystem())
                {
                    var olduser = ctx.Users.FirstOrDefault(_ => _.Id == userId);
                    if (olduser != null)
                    {
                        olduser.FullName = FullName.Trim();
                        olduser.Note = Note;
                        olduser.ThemeName = ThemeName;
                        olduser.IsAdmin = Admin;
                        olduser.IsTester = Tester;
                        olduser.IsDeleted = Deleted;
                        olduser.Avatar = Avatar;
                    }
                    ctx.SaveChanges();
                    if (Admin)
                    {
                        if (!string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(PasswordConfirm)
                                                                 && Password == PasswordConfirm)
                        {
                            var sql = $"ALTER LOGIN {LoginName} WITH PASSWORD = '{Password}'";
                            ctx.Database.ExecuteSqlCommand(sql);
                        }
                    }
                }
            }
        }

        public ICommand CreateNewUserCommand
        {
            get { return new Command(createNewUser, _ => true); }
        }

        private void createNewUser(object obj)
        {
            if (string.IsNullOrWhiteSpace(FirstName) | string.IsNullOrWhiteSpace(LastName) |
                string.IsNullOrWhiteSpace(MiddleName) |
                string.IsNullOrWhiteSpace(LoginName) | string.IsNullOrWhiteSpace(Password) |
                string.IsNullOrWhiteSpace(PasswordConfirm))
            {
                MessageBox.Show(
                    "Поля не могут состоять из пробелов и должны быть обязательно заполнены.");
                return;
            }

            userId = Guid.NewGuid();
            NewUser = new Users
            {
                Id = userId,
                Name = LoginName.Trim(),
                FullName = FullName.Trim(),
                Note = Note,
                ThemeName = ThemeName,
                IsAdmin = Admin,
                IsTester = Tester,
                IsDeleted = Deleted,
                Avatar = Avatar
            };
            MessageBox.Show("Пользователь создан.");
            CloseWindow(Form);
        }

        public override bool IsCanSaveData => isCanSave();

        private bool isCanSave()
        {
            if (IsNewUser)
            {
                return !string.IsNullOrWhiteSpace(LoginName) && !string.IsNullOrWhiteSpace(Password)
                                                             && !string.IsNullOrWhiteSpace(PasswordConfirm)
                                                             && Password == PasswordConfirm;
            }

            return true;
        }

        public ICommand CancelCreateNewUserCommand
        {
            get { return new Command(cancelCreateNewUser, _ => true); }
        }

        private void cancelCreateNewUser(object obj)
        {
            NewUser = null;
            CloseWindow(Form);
        }

        #endregion

        #region Methods

        private void LoadExistingUser(string loginName)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var usr = ctx.Users.FirstOrDefault(_ => _.Name == loginName);
                if (usr == null)
                {
                    IsNewUser = true;
                    return;
                }

                

                //{LastName} {FirstName} {MiddleName}"
                Id = usr.Id;
                userId = usr.Id;
                LoginName = usr.Name;
                setNamesFromFullName(usr.FullName);
                Note = usr.Note;
                ThemeName = usr.ThemeName;
                Admin = usr.IsAdmin;
                Tester = usr.IsTester;
                Deleted = usr.IsDeleted;
                Avatar = usr.Avatar;

                foreach (var c in Companies) c.IsSelectedItem = false;
                var data = ctx.Users.Include(_ => _.DataSources).FirstOrDefault(_ => _.Name == usr.Name)?.DataSources
                    .ToList();
                if (data != null && data.Count > 0)
                    foreach (var d in data)
                    foreach (var c in Companies)
                        if (c.Id == d.Id)
                            c.IsSelectedItem = true;
            }
        }

        private void setNamesFromFullName(string fname)
        {
            string[] fnames = { };
            if (!string.IsNullOrWhiteSpace(fname))
            {
                var s = Regex.Replace(fname, @"\s+", " ");
                fnames = s.Split(' ');
            }
            if (fnames.Length > 0)
            {
                LastName = fnames[0];
                if (fnames.Length >= 2) FirstName = fnames[1];
                if (fnames.Length >= 3) MiddleName = fnames[2];
            }
        }

        private void LoadDataSourceAndRoleList()
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                foreach (var company in ctx.DataSources.ToList())
                    Companies.Add(new DataSourcesViewModel(company)
                    {
                        IsSelectedItem = false
                    });

                foreach (var role in ctx.UserRoles.ToList())
                    Roles.Add(new UserRolesViewModel(role)
                    {
                        IsSelectedItem = false
                    });
            }
        }

        private void LoadRegisteredUsers()
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                foreach (var user in ctx.Users.ToList()) RegisteredUsersNames.Add(user.Name);
            }
        }

        #endregion

        #region InputValidation

        string IDataErrorInfo.Error => Error;

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if(!IsNewUser) return string.Empty;
                switch (columnName)
                {
                    case "FirstName":
                        return ValidateName(FirstName) ? string.Empty : Error;
                    case "LastName":
                        return ValidateName(LastName) ? string.Empty : Error;
                    case "MiddleName":
                        return ValidateName(MiddleName) ? string.Empty : Error;
                    case "LoginName":
                        return ValidateLogin(LoginName) ? string.Empty : Error;
                }

                return string.Empty;
            }
        }

        private void SetError(bool isValid, string errorString)
        {
            Error = isValid ? string.Empty : errorString;
        }

        public bool ValidateName(string name)
        {
            var isValid = !string.IsNullOrEmpty(name);
            SetError(isValid, "Поле должно быть заполнено.");
            return isValid;
        }

        public bool ValidateLogin(string login)
        {
            var isValid = login != RegisteredUsersNames.FirstOrDefault(_ => _ == login);
            SetError(isValid, "Пользователь с таким именем уже зарегистрирован или вы не заполнили поле.");
            return isValid;
        }

        public bool ValidatePassword(string Password, string PasswordConfirm)
        {
            var isValid = Equals(Password, PasswordConfirm);
            SetError(isValid, "Введенные пароли не совпадают, повторите ввод.");
            return isValid;
        }

        #endregion
    }
}