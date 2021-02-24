using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Data;

namespace KursRepositories.ViewModels
{
    class UserCreationWindowViewModel : RSViewModelBase
    {
        public UserCreationWindowViewModel()
        {
        /*public string Error
        {
            get
            {
                string error =
                ["FirstName"] +
                        this["LastName"] +
                    this["Title"] +
                    this["MobilePhone"] +
                    this["Email"];
                if (!string.IsNullOrEmpty(error))
                    return "Please check inputted data.";
                return null;
            }
        }*/

    }

        #region Fields

        private string myFirstName;
        private string myMiddleName;
        private string myLastName;
        private string myFullName;
        private string myLoginName;
        private bool myAdmin = false;
        private bool myTester = false;
        private bool myDeleted = false;
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

        public string FirstName
        {
            get => myFirstName;
            set
            {
                if (myFirstName == null)
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
            get => $"{LastName}  {FirstName} {MiddleName}";
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

        public ICommand CreateNewUserCommand =>
            new Command(createNewUserCommand, _ => LoginName != null && FullName != null);

        private void createNewUserCommand(object obj)
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


        }

        #endregion

        #region Methods

        void RaiseFullNamePropertyChanged()
        {
            RaisePropertyChanged(nameof(FullName));

        }

        

        #endregion
    }
}
