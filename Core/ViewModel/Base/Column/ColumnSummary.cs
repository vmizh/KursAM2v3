using DevExpress.Data;

namespace Core.ViewModel.Base.Column
{
    public class ColumnSummary : RSViewModelBase
    {
        private decimal mySumma;
        public SummaryItemType Type { get; set; }
        public string FieldName { get; set; }
        public string DisplayFormat { set; get; }
        public string Key { set; get; }
        public decimal Summa
        {
            get => mySumma;
            set
            {
                if (mySumma == value) return;
                mySumma = value;
                RaisePropertyChanged();
            }
        }
    }
}