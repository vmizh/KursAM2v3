using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;

namespace PeriodManager.ViewModel
{
    public class PeriodClosedExcludeViewModel : KursViewModelBase
    {
        private DateTime myDateClosed;
        private DateTime myDateFrom;
        private PeriodGroupViewModel myUserGroup;

        [Display(Name = "Группа пользователей"), ReadOnly(true)]
        public PeriodGroupViewModel UserGroup
        {
            get { return myUserGroup; }
            set
            {
                if (value == myUserGroup) return;
                myUserGroup = value;
                OnPropertyChanged(nameof(UserGroup));
            }
        }

        [Display(Name = "Действует до")]
        public DateTime DateOut
        {
            get { return myDateClosed; }
            set
            {
                if (value == myDateClosed) return;
                myDateClosed = value;
                OnPropertyChanged(nameof(DateOut));
            }
        }

        [Display(Name = "Разрешено от")]
        //[Display(AutoGenerateField = false)]
        public DateTime DateFrom
        {
            get { return myDateFrom; }
            set
            {
                if (value == myDateFrom) return;
                myDateFrom = value;
                OnPropertyChanged(nameof(DateFrom));
            }
        }
    }
}