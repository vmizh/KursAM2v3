using Core.ViewModel.Base;

namespace PeriodManager.ViewModel
{
    public class PeriodClosedTypeViewModel : KursViewModelBase
    {
        private bool myIsSelected;

        public bool IsSelected
        {
            get { return myIsSelected; }
            set
            {
                if (myIsSelected == value) return;
                myIsSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }
}