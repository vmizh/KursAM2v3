using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;

namespace KursAM2.ViewModel.Logistiks
{
    public class InventorySheetRowViewModel : RSViewModelData
    {
        private IncomeExpenseSchet myExpenseSchet;
        private bool myIsTaxExecuted;
        private Nomenkl myNomenkl;
        private decimal myPrice;
        private decimal myQuantityCalc;
        private decimal myQuantityFact;

        public Nomenkl Nomenkl
        {
            get => myNomenkl;
            set
            {
                if (myNomenkl != null && myNomenkl.Equals(value)) return;
                myNomenkl = value;
                RaisePropertyChanged();
            }
        }

        public string NomenklNumber => Nomenkl?.NomenklNumber;
        public string NomenklName => Nomenkl?.Name;
        public string NomenklUnit => Nomenkl?.Unit?.Name;
        public string NomenklCrsName => Nomenkl?.Currency?.Name;

        public decimal QuantityCalc
        {
            get => myQuantityCalc;
            set
            {
                if (myQuantityCalc == value) return;
                myQuantityCalc = value;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Summa));
            }
        }

        public decimal QuantityFact
        {
            get => myQuantityFact;
            set
            {
                if (value < 0)
                {
                    //WindowManager.ShowMessage(Application.Current.MainWindow,"Остаток не может быть меньше 0.","Ошибка",MessageBoxImage.Error);
                    RaisePropertyChanged();
                    return;
                }

                if (myQuantityFact == value) return;
                myQuantityFact = value;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Difference));
                RaisePropertiesChanged(nameof(Summa));
            }
        }

        public decimal Difference => QuantityFact - QuantityCalc;

        public bool IsTaxExecuted
        {
            get => myIsTaxExecuted;
            set
            {
                if (myIsTaxExecuted == value) return;
                myIsTaxExecuted = value;
                RaisePropertyChanged();
            }
        }

        public decimal Summa => Price * Difference;

        public decimal Price
        {
            get => myPrice;
            set
            {
                if (myPrice == value) return;
                myPrice = value;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Summa));
            }
        }

        public IncomeExpenseSchet ExpenseSchet
        {
            get => myExpenseSchet;
            set
            {
                if (myExpenseSchet != null && myExpenseSchet.Equals(value)) return;
                myExpenseSchet = value;
                RaisePropertyChanged();
            }
        }
    }
}