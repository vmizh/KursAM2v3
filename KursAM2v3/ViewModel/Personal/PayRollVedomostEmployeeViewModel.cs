using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Core.ViewModel.Base;
using KursDomain.ICommon;
using KursDomain.References;
using Employee = KursDomain.Documents.Employee.Employee;

namespace KursAM2.ViewModel.Personal
{
    /// <summary>
    ///     Промежуточные вычисленные значения для сотрудников в ведомости
    /// </summary>
    public class PayRollVedomostEmployeeViewModel : RSViewModelData
    {
        private Employee myEemployee;
        private decimal myEurSumma;

        private ObservableCollection<PayRollVedomostEmployeeRowViewModel> myRemoveRows =
            new ObservableCollection<PayRollVedomostEmployeeRowViewModel>();

        private ObservableCollection<PayRollVedomostEmployeeRowViewModel> myRows =
            new ObservableCollection<PayRollVedomostEmployeeRowViewModel>();

        private decimal myRubSumma;
        private decimal myUsdSumma;

        public PayRollVedomostEmployeeViewModel()
        {
            Rows.CollectionChanged += Rows_CollectionChanged;
        }

        public ObservableCollection<PayRollVedomostEmployeeRowViewModel> RemoveRows
        {
            set
            {
                myRemoveRows = value;
                RaisePropertyChanged();
            }
            get => myRemoveRows;
        }

        public ObservableCollection<PayRollVedomostEmployeeRowViewModel> Rows
        {
            set
            {
                myRows = value;
                RaisePropertyChanged();
            }
            get => myRows;
        }

        public Employee Employee
        {
            set
            {
                if (myEemployee == value) return;
                myEemployee = value;
                RaisePropertyChanged();
            }
            get => myEemployee;
        }

        public int TabelNumber => Employee?.TabelNumber ?? -1;
        public Currency EmployeeCrs => Employee.Currency;

        public decimal USDSumma
        {
            set
            {
                myUsdSumma = value;
                RaisePropertyChanged();
            }
            get => myUsdSumma;
        }

        public decimal RUBSumma
        {
            set
            {
                myRubSumma = value;
                RaisePropertyChanged();
            }
            get => myRubSumma;
        }

        public decimal EURSumma
        {
            set
            {
                myEurSumma = value;
                RaisePropertyChanged();
            }
            get => myEurSumma;
        }

        public override decimal DocCode => Employee?.DocCode ?? 0;

        public void CalcSumma()
        {
            EURSumma = Rows.Where(_ => _.Crs.Name == "EUR").Sum(_ => _.Summa);
            USDSumma = Rows.Where(_ => _.Crs.Name == "USD").Sum(_ => _.Summa);
            RUBSumma = Rows.Where(_ => _.Crs.Name == "RUB" || _.Crs.Name == "RUR").Sum(_ => _.Summa);
            State = RowStatus.NotEdited;
        }

        private void Rows_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnStateChanged();
            RaisePropertyChanged(nameof(Rows));
        }

        public void RemoveRow(PayRollVedomostEmployeeRowViewModel row)
        {
            if (!Rows.Contains(row)) return;
            var id = Rows.IndexOf(row);
            if (row.State != RowStatus.NewRow)
                RemoveRows.Add(row);
            Rows.RemoveAt(id);
            RaisePropertyChanged(nameof(Rows));
        }
    }
}
