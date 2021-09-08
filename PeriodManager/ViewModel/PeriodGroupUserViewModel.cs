using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Core.EntityViewModel.Systems;
using Core.ViewModel.Base;

namespace PeriodManager.ViewModel
{
    [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
    public class PeriodGroupUserViewModel : RSViewModelBase
    {
        private UserViewModel myUser;

        [Display(Name = "Пользователь")]
        public UserViewModel User
        {
            get => myUser;
            set
            {
                if (value == myUser) return;
                myUser = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public override string Name
        {
            get => myName;
            set
            {
                if (value == myName) return;
                myName = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public override string Note
        {
            get => myNote;
            set
            {
                if (value == myNote) return;
                myNote = value;
                RaisePropertyChanged();
            }
        }
    }
}