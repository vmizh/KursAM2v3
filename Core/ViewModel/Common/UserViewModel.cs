using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;

namespace Core.ViewModel.Common
{
    public class UserViewModel : RSViewModelBase
    {
        [Display(Name = "Полное имя")] private string myFullName;
        private int myTabelNumber;
        private int myUserId;

        [Display(AutoGenerateField = false)]
        public int UserId
        {
            get => myUserId;
            set
            {
                if (value == myUserId) return;
                myUserId = value;
                RaisePropertyChanged();
            }
        }

        public string FullName
        {
            get => myFullName;
            set
            {
                if (value == myFullName) return;
                myFullName = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Табельный номер")]
        public int TabelNumber
        {
            get => myTabelNumber;
            set
            {
                if (value == myTabelNumber) return;
                myTabelNumber = value;
                RaisePropertyChanged();
            }
        }

        public override string ToString()
        {
            return $"{Name} ({FullName}, {TabelNumber})";
        }
    }
}