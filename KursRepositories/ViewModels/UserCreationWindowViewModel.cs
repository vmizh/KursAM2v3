using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Editors;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;


namespace KursRepositories.ViewModels
{
    public class UserCreationWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        public UserCreationWindowViewModel()
        {


        }

        #region Fields

        private string myFirstName;
        private string myMiddleName;
        private string myLastName;
        private string myFullName;
        private string myLoginName;
        private bool myAdmin = false;
        private bool myTester = false;
        private readonly bool myDeleted = false;
        private byte[] myAvatar;
        private string myThemeName = "MetropolisLight";
        private new string myNote;
        private ObservableCollection<UsersViewModel> myNewUser = new ObservableCollection<UsersViewModel>();


        #endregion

        #region Properties

        public ObservableCollection<UsersViewModel> NewUser
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

        [Required(ErrorMessage = "необходимо ввести имя.")]
        public string FirstName
        {
            get => myFirstName;
            set
            {
                if (myFirstName == value)
                    return;
                myFirstName = value;
                RaiseFullNamePropertyChanged();
                RaisePropertyChanged();
            }
        }

        [Required(ErrorMessage = "необходимо ввести имя.")]
        public string MiddleName
        {
            get => myMiddleName;
            set
            {
                if (myMiddleName == value)
                    return;
                myMiddleName = value;
                RaiseFullNamePropertyChanged();
                RaisePropertyChanged();
            }
        }

        [Required(ErrorMessage = "необходимо ввести имя.")]
        public string LastName
        {
            get => myLastName;
            set
            {
                if (myLastName == value)
                    return;
                myLastName = value;
                RaiseFullNamePropertyChanged();
                RaisePropertyChanged();
            }
        }

        [Required(ErrorMessage = "необходимо ввести имя.")]
        public string FullName
        {
            get => $"{LastName} {FirstName} {MiddleName}";

            set
            {
                if (myFullName == value)
                    return;
                myFullName = value;
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

        public bool Deleted => myDeleted;

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

        [Required(ErrorMessage = "необходимо ввести имя.")]
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
            NewUser.Add(new UsersViewModel(new Users
            {
                Id = new Guid(),
                Name = LoginName,
                FullName = FullName,
                Note = Note,
                ThemeName = ThemeName,
                IsAdmin = Admin,
                IsTester = Tester,
                IsDeleted = Deleted,
                Avatar = Avatar

            }));

            MessageBox.Show("Пользователь успешно зарегистрирован.");
        }

        string IDataErrorInfo.Error => string.Empty;

        public string Error
        {
            get => error;
        }

        private string error;

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
                }
                return string.Empty;
            }
        }
        


        #endregion

        #region Methods

        private void RaiseFullNamePropertyChanged()
        {
            RaisePropertyChanged(nameof(FullName));
        }

        #region ImnputValidation

        void SetError(bool isValid, string errorString)
        {
            error = isValid ? string.Empty : errorString;
        }

        public bool ValidateName(string name)
        {
            bool isValid = !string.IsNullOrEmpty(name);
            SetError(isValid, $"Поле {name} должно быть заполнено.");
            return isValid;
        }

        public bool ValidateLogin(string login)
        {
            bool isValid = login != "Test";
            SetError(isValid, "Такой пользователь уже зарегистрирован.");
            return isValid;
        }

        #endregion

        #endregion

    }


}