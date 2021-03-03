using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.ViewModel.Base;
using Data;
using static KursRepositories.ViewModels.MainWindowPermissionsViewModel;


namespace KursRepositories.ViewModels
{
    public class UserCreationWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Constructor
        public UserCreationWindowViewModel()
        {
            LoadRegisteredUsers();
        }
        
        #endregion


        #region Fields

        private string error;
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
        public List<string> RegisteredUserNames = new List<string>();
        


        #endregion

        #region Properties

        public string Error => error;
        
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
            if (string.IsNullOrWhiteSpace(FirstName) | string.IsNullOrWhiteSpace(LastName) | string.IsNullOrWhiteSpace(MiddleName) |
                string.IsNullOrWhiteSpace(LoginName))
            {
                MessageBox.Show("Поля Ф.И.О. и Login не могут состоять из пробелов и должны быть обязательно заполнены.");
                return;
            }

            UserList.Add(new UsersViewModel(new Users //Добавляю пользователя в список UserList в главной моделе
            {
                Id = new Guid(),
                Name = LoginName.Trim(),
                FullName = FullName.Trim(),
                Note = Note,
                ThemeName = ThemeName,
                IsAdmin = Admin,
                IsTester = Tester,
                IsDeleted = Deleted,
                Avatar = Avatar

            }));

            using (var context = new KursSystemEntities())
            {
                var oldUsers = context.Users.ToList();
                foreach (var u in UserList)
                {
                    var propertyUser = oldUsers.FirstOrDefault(_ => _.Id == u.Id);
                    if (propertyUser != null)
                    {
                        propertyUser.Id = u.Id;
                        propertyUser.Name = u.Name;
                        propertyUser.FullName = u.FullName;
                        propertyUser.Note = u.Note;
                        propertyUser.IsAdmin = u.IsAdmin;
                        propertyUser.IsTester = u.IsTester;
                        propertyUser.IsDeleted = u.IsDeleted;
                        propertyUser.ThemeName = u.ThemeName;
                        propertyUser.Avatar = u.Avatar;
                    }
                    else
                    {
                        context.Users.Add(new Users()
                        {
                            Id = u.Id,
                            Name = u.Name,
                            FullName = u.FullName,
                            Note = u.Note,
                            IsAdmin = u.IsAdmin,
                            IsTester = u.IsTester,
                            IsDeleted = u.IsDeleted,
                            ThemeName = u.ThemeName,
                            Avatar = u.Avatar
                            
                        });
                    }
                }
                // TODO: Добавить SaveChanges
                MessageBox.Show("Пользователь успешно зарегистрирован.");
            }
        }

            #endregion

            #region Methods

            private void LoadRegisteredUsers()
            {
                using (var ctx = new KursSystemEntities())
                {
                    foreach (var user in ctx.Users.ToList())
                    {
                        RegisteredUserNames.Add(user.Name);
                    }

                }
            }

            private void RaiseFullNamePropertyChanged()
            {
                RaisePropertyChanged(nameof(FullName));
            }

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
                    }
                    return string.Empty;
                }
            }

            private void SetError(bool isValid, string errorString)
            {
                error = isValid ? string.Empty : errorString;
            }
        
            public bool ValidateName(string name)
            {
                bool isValid = !string.IsNullOrEmpty(name);
                SetError(isValid, $"Поле должно быть заполнено.");
                return isValid;
            }

            public bool ValidateLogin(string login)
            {
                bool isValid = login != RegisteredUserNames.FirstOrDefault(_ => _ == login);
                SetError(isValid, "Пользователь с таким именем уже зарегистрирован или вы не заполнили поле."); 
                return isValid;
            }

            #endregion

            #endregion

    }


  


}