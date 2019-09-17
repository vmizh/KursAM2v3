using System;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Data;

namespace KursAM2.ViewModel.Personal
{
    /// <summary>
    ///     Строки ведомости начисления заработно платы
    /// </summary>
    public class PayRollVedomostEmployeeRowViewModel : RSViewModelDataEntity<EMP_PR_ROWS>
    {
        private Currency myCrs;
        private Employee myEmployee;
        private EMP_PR_ROWS myEntity;
        private decimal myNachEmpRate;
        private decimal myOplataEmpRate;
        private EMP_PAYROLL_TYPEViewModel myPrType;
        private decimal myRate;
        private Guid myRowId;
        private decimal mySumma;
        private DateTime myNachDate;

        public PayRollVedomostEmployeeRowViewModel()
        {
            myNachEmpRate = 0;
            myOplataEmpRate = 0;
        }

        public PayRollVedomostEmployeeRowViewModel(PayRollVedomostEmployeeViewModel emp)
        {
            myNachEmpRate = 0;
            myOplataEmpRate = 0;
            Parent = emp;
        }

        public override Guid RowId
        {
            get => myRowId;
            set
            {
                if (myRowId == value) return;
                myRowId = value;
                RaisePropertyChanged();
            }
        }

        public Employee Employee
        {
            get => myEmployee;
            set
            {
                if (myEmployee != null && myEmployee.Equals(value)) return;
                myEmployee = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Name));
            }
        }

        public Currency Crs
        {
            get => myCrs;
            set
            {
                if (Equals(myCrs, value)) return;
                myCrs = value;
                RaisePropertyChanged(nameof(Employee));
            }
        }

        public EMP_PAYROLL_TYPEViewModel PRType
        {
            get => myPrType;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myPrType == value) return;
                myPrType = value;
                RaisePropertyChanged();
            }
        }

        public decimal Summa
        {
            get => mySumma;
            set
            {
                if (mySumma == value) return;
                mySumma = value;
                var p = Parent as PayRollVedomostEmployeeViewModel;
                p?.CalcSumma();
                RaisePropertyChanged();
            }
        }

        public decimal Rate
        {
            get => myRate;
            set
            {
                if (myRate == value) return;
                myRate = value;
                var p = Parent as PayRollVedomostEmployeeViewModel;
                p?.CalcSumma();
                RaisePropertyChanged();
            }
        }

        public decimal NachEmpRate
        {
            get => myNachEmpRate;
            set
            {
                if (myNachEmpRate == value) return;
                myNachEmpRate = value;
                RaisePropertyChanged();
            }
        }

        public decimal OplataEmpRate
        {
            get => myOplataEmpRate;
            set
            {
                if (myOplataEmpRate == value) return;
                myOplataEmpRate = value;
                RaisePropertyChanged();
            }
        }

        public DateTime NachDate
        {
            get => myNachDate;
            set
            {
                if (myNachDate == value) return;
                myNachDate = value;
                RaisePropertyChanged();
            }
        }

        public override EMP_PR_ROWS Entity
        {
            get => myEntity;
            set
            {
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public override void SetEntity(Guid entityId)
        {
        }

        public override void SetEntity(decimal entityDocCode)
        {
            throw new NotImplementedException();
        }
    }
}