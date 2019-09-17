using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;

namespace PeriodManager.ViewModel
{
    public class PeriodClosedViewModel : KursViewModelBase
    {
        private DateTime myDateClosed;
        private ObservableCollection<global::PeriodManager.ViewModel.PeriodClosedExcludeViewModel> myExcludeUserGroups;
        private PeriodClosedTypeViewModel myTypeClosed;

        [Display(Name = "Тип операций"), ReadOnly(true)]
        public PeriodClosedTypeViewModel TypeClosed
        {
            get { return myTypeClosed; }
            set
            {
                if (value == myTypeClosed) return;
                myTypeClosed = value;
                OnPropertyChanged(nameof(TypeClosed));
            }
        }

        [Display(Name = "Дата закрытия")]
        public DateTime DateClosed
        {
            get { return myDateClosed; }
            set
            {
                if (value == myDateClosed) return;
                myDateClosed = value;
                OnPropertyChanged(nameof(DateClosed));
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

        [Display(AutoGenerateField = false)]
        public ObservableCollection<global::PeriodManager.ViewModel.PeriodClosedExcludeViewModel> ExcludeUserGroups
        {
            get { return myExcludeUserGroups; }
            set
            {
                if (value == myExcludeUserGroups) return;
                myExcludeUserGroups = value;
                OnPropertyChanged(nameof(ExcludeUserGroups));
            }
        }
    }
}