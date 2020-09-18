using System;
using Core.ViewModel.Base;

namespace Core.ViewModel.Common
{
    public class DatePeriodViewModel : RSViewModelBase
    {
        private DateTime myFirstDate;
        private string myHeader;
        private DateTime mySecondDate;

        public DatePeriodViewModel()
        {
            myFirstDate = DateTime.Today;
            mySecondDate = DateTime.Today;
        }

        public DateTime FirstDate
        {
            get => myFirstDate;
            set
            {
                if (value == myFirstDate) return;
                myFirstDate = value;
                RaisePropertyChanged();
            }
        }

        public string Header
        {
            get => myHeader;
            set
            {
                if (value == myHeader) return;
                myHeader = value;
                RaisePropertyChanged();
            }
        }

        public DateTime SecondDate
        {
            get => mySecondDate;
            set
            {
                if (value == mySecondDate) return;
                mySecondDate = value;
                RaisePropertyChanged();
            }
        }
    }
}