using System;
using System.Windows;

namespace Core.ViewModel.Base
{
    public abstract class RSWindowSearchViewModelBase : RSWindowViewModelBase
    {
        private DateTime myEndDate;
        private string myFirstSearchName;
        private string mySecondSearchName;
        private DateTime myStartDate;
        
        public RSWindowSearchViewModelBase()
        {
            StartDate = DateTime.Today.AddDays(-14);
            EndDate = DateTime.Today;
        }

        public RSWindowSearchViewModelBase(Window form) : base(form)
        {
            StartDate = DateTime.Today.AddDays(-14);
            EndDate = DateTime.Today;
        }


        public string FirstSearchName
        {
            get => myFirstSearchName;
            set
            {
                if (myFirstSearchName == value) return;
                myFirstSearchName = value;
                RaisePropertyChanged();
            }
        }

        public string SecondSearchName
        {
            get => mySecondSearchName;
            set
            {
                if (mySecondSearchName == value) return;
                mySecondSearchName = value;
                RaisePropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get => myStartDate;
            set
            {
                if (myStartDate == value) return;
                myStartDate = value;
                RaisePropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => myEndDate;
            set
            {
                if (myEndDate == value) return;
                myEndDate = value;
                RaisePropertyChanged();
            }
        }

        public virtual string SplashCaption { set; get; } = "Загрузка...";
    }
}