using System.Collections.ObjectModel;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Personal
{
    public class EmployeePayPeriodViewModel : RSViewModelBase
    {
        public EmployeePayPeriodViewModel()
        {
            Periods = new ObservableCollection<NachEmployeeForPeriod>();
        }

        public ObservableCollection<NachEmployeeForPeriod> Periods { set; get; }
    }
}