using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ViewModel.Base;

namespace KursRepositories.ViewModels
{
    class UserCreationWindowViewModel : RSViewModelBase
    {
        public UserCreationWindowViewModel ()
        {
           
        }

        #region Fields

        private string myFirstName;
        private string myMiddleName;
        private string myLastName;
        private string myFullName => $"{LastName} {FirstName} {MiddleName}";
        private bool myAdmin;
        private bool myTester;
        private bool myDeleted = false;
        private byte myAvatar;

        #endregion

        #region Properties

        private ObservableCollection<UsersViewModel> NewUser { get; set; } = new ObservableCollection<UsersViewModel>();

        public string FirstName
        {
            get => myFirstName;
            set
            {
                if (myFirstName == null)
                    return;
                myFirstName = value;
                RaisePropertyChanged();
            }
        }

        public string MiddleName
        {
            get => myMiddleName;
            set
            {
                if(myMiddleName == value)
                    return;
                myMiddleName = value;
                RaisePropertyChanged();
            }
        }

        public string LastName
        {
            get => myLastName;
            set
            {
                if(myLastName == value)
                    return;
                myLastName = value;
                RaisePropertyChanged();
            }
        }

        public string FullName => myFullName ;

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

        public bool Deleted => myDeleted;

        public byte Avatar
        {
            get => myAvatar;
            set
            {
                if(myAvatar == value)
                    return;
                myAvatar = value;
                RaisePropertyChanged();
            }
        }
        
        #endregion



    }
    
}
