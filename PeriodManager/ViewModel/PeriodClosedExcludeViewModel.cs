using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;

namespace PeriodManager.ViewModel
{
    public class PeriodClosedExcludeViewModel : RSViewModelBase
    {
        private DateTime myDateClosed;
        private DateTime myDateFrom;
        private PeriodGroupViewModel myUserGroup;

        [Display(Name = "Группа пользователей")]
        [ReadOnly(true)]
        public PeriodGroupViewModel UserGroup
        {
            get => myUserGroup;
            set
            {
                if (value == myUserGroup) return;
                myUserGroup = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Действует до")]
        public DateTime DateOut
        {
            get => myDateClosed;
            set
            {
                if (value == myDateClosed) return;
                myDateClosed = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Разрешено от")]
        public DateTime DateFrom
        {
            get => myDateFrom;
            set
            {
                if (value == myDateFrom) return;
                myDateFrom = value;
                RaisePropertyChanged();
            }
        }
    }
}