using Core.ViewModel.Base;

namespace Core.ViewModel.Common
{
    public class IncomeExpenseSchet : RSViewModelData
    {
        private IncomeExpenseStatia myStatia;
        public IncomeExpenseStatia Statia
        {
            get => myStatia;
            set
            {
                if (myStatia != null && myStatia.Equals(value)) return;
                myStatia = value;
                RaisePropertyChanged();
            }
        }
    }
}