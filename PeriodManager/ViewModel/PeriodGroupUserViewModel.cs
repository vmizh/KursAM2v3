using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Core.ViewModel.Common;

namespace PeriodManager.ViewModel
{
    public class PeriodGroupUserViewModel : KursViewModelBase
    {
        private UserViewModel myUser;

        [Display(Name = "Пользователь")]
        public UserViewModel User
        {
            get { return myUser; }
            set
            {
                if (value == myUser) return;
                myUser = value;
                OnPropertyChanged(nameof(User));
            }
        }

        [Display(AutoGenerateField = false)]
        public override string Name
        {
            get { return myName; }
            set
            {
                if (value == myName) return;
                myName = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        [Display(AutoGenerateField = false)]
        public override string Note
        {
            get { return myNote; }
            set
            {
                if (value == myNote) return;
                myNote = value;
                OnPropertyChanged(nameof(Note));
            }
        }
    }
}