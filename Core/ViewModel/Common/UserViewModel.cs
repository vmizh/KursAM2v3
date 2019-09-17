using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;

namespace Core.ViewModel.Common
{
    public class UserViewModel : KursViewModelBase
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
                OnPropertyChanged(nameof(UserId));
            }
        }
        public string FullName
        {
            get => myFullName;
            set
            {
                if (value == myFullName) return;
                myFullName = value;
                OnPropertyChanged(nameof(FullName));
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
                OnPropertyChanged(nameof(TabelNumber));
            }
        }

        public override string ToString()
        {
            return $"{Name} ({FullName}, {TabelNumber})";
        }
    }
}