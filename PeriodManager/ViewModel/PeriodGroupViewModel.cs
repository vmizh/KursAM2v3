using System.Collections.ObjectModel;
using Core.ViewModel.Base;

namespace PeriodManager.ViewModel
{
    public class PeriodGroupViewModel : KursViewModelBase
    {
        public PeriodGroupViewModel()
        {
            Users = new ObservableCollection<PeriodGroupUserViewModel>();
        }

        public ObservableCollection<PeriodGroupUserViewModel> Users { set; get; }
    }
}