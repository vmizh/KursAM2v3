using System;
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
using Data;
using KursDomain;
using KursDomain.Documents.Systems;

namespace KursRepositories.ViewModels
{
    public class UserCreationWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Constructor

        public UserCreationWindowViewModel()
        {
            LoadRegisteredUsers();
            LoadDataSourceAndRoleList();
        }

        public UserCreationWindowViewModel(string loginName) : this()
        {
            if (!string.IsNullOrWhiteSpace(loginName)) IsNewUser = false;

            LoadExistingUser(loginName);
        }

        #endregion


        #region Fields

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
        private List<string> _myRegisteredUsersNames = new();

        private ObservableCollection<DataSourcesViewModel> myCompanies =
            new();

        private ObservableCollection<UserRolesViewModel> myRoles = new();

        private ObservableCollection<DataSourcesViewModel> myIsSelectedCompanies =
            new();

        private ObservableCollection<UserRolesViewModel> myIsSelectedRoles =
            new();

        private DataSourcesViewModel myCurrentCompany;
        private UserRolesViewModel myCurrentRole;
        private string myPassword;
        private string myPasswordConfirm;

        #endregion

        #region Properties

        public bool IsNewUser { set; get; } = true;

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
                RaisePassword();
                RaisePropertiesChanged();
            }
        }

        public string PasswordConfirm
        {
            get => myPasswordConfirm;
            set
            {
                if (myPasswordConfirm == value) return;
                myPasswordConfirm = value;
                RaisePasswordConfirm();
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
                if (myLastName == value)
                    return;
                myFullName = value;
                var fnames = myFullName.Split(' ');
                if (fnames.Length > 0)
                {
                    LastName = fnames[0];
                    if (fnames.Length >= 2) MiddleName = fnames[1];
                    if (fnames.Length >= 3) FirstName = fnames[2];
                }

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

            NewUser = new Users
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
            MessageBox.Show("Пользователь создан.");
            CloseWindow(Form);
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

                string[] fnames = { };
                if (!string.IsNullOrWhiteSpace(usr.FullName))
                {
                    var s = Regex.Replace(usr.FullName, @"\s+", " ");
                    fnames = s.Split(' ');
                }

                //{LastName} {FirstName} {MiddleName}"
                Id = usr.Id;
                LoginName = usr.Name;
                if (fnames.Length > 0)
                {
                    LastName = fnames[0];
                    if (fnames.Length >= 2) MiddleName = fnames[1];
                    if (fnames.Length >= 3) FirstName = fnames[2];
                }

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

        private void RaisePassword()
        {
            RaisePropertyChanged(nameof(PasswordConfirm));
        }

        private void RaisePasswordConfirm()
        {
            RaisePropertyChanged(nameof(Password));
        }

        #endregion

        #region InputValidation

        string IDataErrorInfo.Error => Error;

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
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
                    case "Password":
                        return ValidatePassword(Password, PasswordConfirm) ? string.Empty : Error;
                    case "PasswordConfirm":
                        return ValidatePassword(Password, PasswordConfirm) ? string.Empty : Error;
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

        public override void RefreshData(object obj)
        {
        }
    }
}
