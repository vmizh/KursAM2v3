using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.XtraEditors.DXErrorProvider;
using KursDomain;
using KursRepositories.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace KursAM2.ViewModel.Repozit
{
    public sealed class UserProfileWindowViewModel : RSWindowViewModelBase, IDataErrorInfo

    {
        public UserProfileWindowViewModel(string login)
        {
            WindowName = "Профиль пользователя";

            if (!string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Данные пользователя не доступны, обратитесь к администратору.");
                return;
            }

            LoadUserProfile(login);
            LoadRegisteredUsers();

        }

        #region Fields

        
        private string myFirstName;
        private string myMiddleName;
        private string myLastName;
        private string myFullName;
        private string myLoginName;
        private bool myAdmin;
        private bool myTester;
        private byte[] myAvatar;
        private string myThemeName;
        private new string myNote;
        private string myPassword;
        private string myPasswordConfirm;
        private Guid myId;
        private List<string> myRegisteredUsersNames;

        #endregion

        #region Properties

        public List<string> RegisteredUsersNames
        {
            get => myRegisteredUsersNames;
            set
            {
                if (myRegisteredUsersNames == value)
                    return;
                myRegisteredUsersNames = value;
                RaisePropertyChanged();
            }
        }

        public override Guid Id
        {
            get => myId;
            set
            {
                if (myId == value)
                    return;
                myId = value;
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
                RaisePasswordConfirm();
                RaisePropertyChanged();
            }
        }

        public string Error { get; private set; }

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

        #region Methods

        private void LoadUserProfile(string login)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var usr = ctx.Users.FirstOrDefault(_ => _.Name == login);
                if (usr == null)
                {
                    MessageBox.Show("Данные пользователя не доступны, обратитесь к администратору.");
                    return;
                }

                Id = usr.Id;
                LoginName = usr.Name;
                setNamesFromFullName(usr.FullName);
                Note = usr.Note;
                ThemeName = usr.ThemeName;
                Admin = usr.IsAdmin;
                Tester = usr.IsTester;
                Deleted = usr.IsDeleted;
                Avatar = usr.Avatar;

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

        #region Command

        public override void SaveData(object data)
        {

            if (!string.IsNullOrWhiteSpace(LoginName) && !string.IsNullOrWhiteSpace(Password) &&
                !string.IsNullOrWhiteSpace(PasswordConfirm)
                && Password == PasswordConfirm)
                
                using (var ctx = GlobalOptions.KursSystem())
                {
                    var olduser = ctx.Users.FirstOrDefault(_ => _.Id == Id);
                    if (olduser != null)
                    {
                        olduser.Id = Id;
                        olduser.FullName = FullName.Trim();
                        olduser.Note = Note;
                        olduser.ThemeName = ThemeName;
                        olduser.IsAdmin = Admin;
                        olduser.IsTester = Tester;
                        olduser.IsDeleted = Deleted;
                        olduser.Avatar = Avatar;
                    }
                    ctx.SaveChanges();
                }
        }
    

        // public override bool IsCanSaveData => isCanSave();
        //
        // private bool isCanSave()
        // {
        //     if
        //     {
        //         return !string.IsNullOrWhiteSpace(LoginName) && !string.IsNullOrWhiteSpace(Password)
        //                                                      && !string.IsNullOrWhiteSpace(PasswordConfirm)
        //                                                      && Password == PasswordConfirm;
        //     }
        //
        //     return true;
        // }

        public ICommand CancelChangesCommand
        {
            get { return new Command(CancelChanges, _ => true); }
        }

        private void CancelChanges(object obj)
        {
            CloseWindow(Form);
        }

        #endregion
    }
}
