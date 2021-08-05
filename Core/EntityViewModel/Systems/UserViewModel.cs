using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;

namespace Core.EntityViewModel.Systems
{
    public class UserViewModel : RSViewModelBase
    {
        private string myFullName;
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

        [Display(Name = "Полное имя")]
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

        [Display(Name = "Табельный номер", AutoGenerateField = false)]
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