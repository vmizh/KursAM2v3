using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Core.ViewModel.Base;

namespace PeriodManager.ViewModel
{
    [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
    public class PeriodClosedViewModel : RSViewModelBase
    {
        private DateTime myDateClosed;
        private ObservableCollection<PeriodClosedExcludeViewModel> myExcludeUserGroups;
        private PeriodClosedTypeViewModel myTypeClosed;

        [Display(Name = "Тип операций")]
        [ReadOnly(true)]
        public PeriodClosedTypeViewModel TypeClosed
        {
            get => myTypeClosed;
            set
            {
                if (value == myTypeClosed) return;
                myTypeClosed = value;
                RaisePropertyChanged(nameof(TypeClosed));
            }
        }

        [Display(Name = "Дата закрытия")]
        public DateTime DateClosed
        {
            get => myDateClosed;
            set
            {
                if (value == myDateClosed) return;
                myDateClosed = value;
                RaisePropertyChanged(nameof(DateClosed));
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
                RaisePropertyChanged(nameof(Name));
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
                RaisePropertyChanged(nameof(Note));
            }
        }

        [Display(AutoGenerateField = false)]
        public ObservableCollection<PeriodClosedExcludeViewModel> ExcludeUserGroups
        {
            get => myExcludeUserGroups;
            set
            {
                if (value == myExcludeUserGroups) return;
                myExcludeUserGroups = value;
                RaisePropertyChanged(nameof(ExcludeUserGroups));
            }
        }
    }
}