using Core.ViewModel.Base;

namespace PeriodManager.ViewModel
{
    public class PeriodClosedTypeViewModel : RSViewModelBase
    {
        private bool myIsSelected;

        public bool IsSelected
        {
            get => myIsSelected;
            set
            {
                if (myIsSelected == value) return;
                myIsSelected = value;
                RaisePropertyChanged();
            }
        }
    }
}