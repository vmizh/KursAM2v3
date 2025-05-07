using System.ComponentModel.DataAnnotations;
using Core;

namespace KursAM2.ViewModel.Period
{
    [MetadataType(typeof(DataAnnotationsMultiCurrenciesDataPeriod))]
    public class MultyCurrenciesDatePeriod : DatePeriod, IMultyWithDilerCurrency
    {
        private decimal myDilerCHF;
        private decimal myDilerEUR;
        private decimal myDilerGBP;
        private decimal myDilerRUB;
        private decimal myDilerSEK;
        private decimal myDilerUSD;
        private decimal myLossCHF;
        private decimal myLossEUR;
        private decimal myLossGBP;
        private decimal myLossRUB;
        private decimal myLossSEK;
        private decimal myLossUSD;
        private string myName;
        private decimal myProfitCHF;
        private decimal myProfitEUR;
        private decimal myProfitGBP;
        private decimal myProfitRUB;
        private decimal myProfitSEK;
        private decimal myProfitUSD;
        private decimal myResultCHF;
        private decimal myResultEUR;
        private decimal myResultGBP;
        private decimal myResultRUB;
        private decimal myResultSEK;
        private decimal myResultUSD;

        public MultyCurrenciesDatePeriod()
        {
        }

        public MultyCurrenciesDatePeriod(DatePeriod d)
        {
            UpdateFromDatePeriod(d);
        }

        public override string Name
        {
            get => myName;
            set
            {
                if (myName == value) return;
                myName = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Шведская Крона
        /// </summary>
        public decimal LossSEK
        {
            get => myLossSEK;
            set
            {
                if (myLossSEK == value) return;
                myLossSEK = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Шведская Крона
        /// </summary>
        public decimal ProfitSEK
        {
            get => myProfitSEK;
            set
            {
                if (myProfitSEK == value) return;
                myProfitSEK = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Шведская Крона
        /// </summary>
        public decimal ResultSEK
        {
            get => myResultSEK;
            set
            {
                if (myResultSEK == value) return;
                myResultSEK = value;
                RaisePropertyChanged();
            }
        }

        public decimal ProfitCNY { get; set; }

        /// <summary>
        ///     Швейцарский франк
        /// </summary>
        public decimal LossCHF
        {
            get => myLossCHF;
            set
            {
                if (myLossCHF == value) return;
                myLossCHF = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Швейцарский франк
        /// </summary>
        public decimal ProfitCHF
        {
            get => myProfitCHF;
            set
            {
                if (myProfitCHF == value) return;
                myProfitCHF = value;
                RaisePropertyChanged();
            }
        }

        public decimal LossCNY { get; set; }

        /// <summary>
        ///     Швейцарский франк
        /// </summary>
        public decimal ResultCHF
        {
            get => myResultCHF;
            set
            {
                if (myResultCHF == value) return;
                myResultCHF = value;
                RaisePropertyChanged();
            }
        }

        public decimal LossGBP
        {
            get => myLossGBP;
            set
            {
                if (myLossGBP == value) return;
                myLossGBP = value;
                RaisePropertyChanged();
            }
        }

        public decimal ProfitGBP
        {
            get => myProfitGBP;
            set
            {
                if (myProfitGBP == value) return;
                myProfitGBP = value;
                RaisePropertyChanged();
            }
        }

        public decimal ResultGBP
        {
            get => myResultGBP;
            set
            {
                if (myResultGBP == value) return;
                myResultGBP = value;
                RaisePropertyChanged();
            }
        }

        public decimal ProfitRUB
        {
            set
            {
                if (Equals(value, myProfitRUB)) return;
                myProfitRUB = value;
                RaisePropertyChanged();
            }
            get => myProfitRUB;
        }

        public decimal LossRUB
        {
            set
            {
                if (Equals(value, myLossRUB)) return;
                myLossRUB = value;
                RaisePropertyChanged();
            }
            get => myLossRUB;
        }

        public decimal ResultRUB
        {
            set
            {
                if (Equals(value, myResultRUB)) return;
                myResultRUB = value;
                RaisePropertyChanged();
            }
            get => myResultRUB;
        }

        public decimal ProfitUSD
        {
            set
            {
                if (Equals(value, myProfitUSD)) return;
                myProfitUSD = value;
                RaisePropertyChanged();
            }
            get => myProfitUSD;
        }

        public decimal LossUSD
        {
            set
            {
                if (Equals(value, myLossUSD)) return;
                myLossUSD = value;
                RaisePropertyChanged();
            }
            get => myLossUSD;
        }

        public decimal ResultUSD
        {
            set
            {
                if (Equals(value, myResultUSD)) return;
                myResultUSD = value;
                RaisePropertyChanged();
            }
            get => myResultUSD;
        }

        public decimal ResultCNY { get; }

        public decimal ProfitEUR
        {
            set
            {
                if (Equals(value, myProfitEUR)) return;
                myProfitEUR = value;
                RaisePropertyChanged();
            }
            get => myProfitEUR;
        }

        public decimal LossEUR
        {
            set
            {
                if (Equals(value, myLossEUR)) return;
                myLossEUR = value;
                RaisePropertyChanged();
            }
            get => myLossEUR;
        }

        public decimal ResultEUR
        {
            set
            {
                if (Equals(value, myResultEUR)) return;
                myResultEUR = value;
                RaisePropertyChanged();
            }
            get => myResultEUR;
        }

        public decimal DilerRUB
        {
            set
            {
                if (Equals(value, myDilerRUB)) return;
                myDilerRUB = value;
                RaisePropertyChanged();
            }
            get => myDilerRUB;
        }

        public decimal DilerUSD
        {
            set
            {
                if (Equals(value, myDilerUSD)) return;
                myDilerUSD = value;
                RaisePropertyChanged();
            }
            get => myDilerUSD;
        }

        public decimal DilerCNY { get; set; }

        public decimal DilerEUR
        {
            set
            {
                if (Equals(value, myDilerEUR)) return;
                myDilerEUR = value;
                RaisePropertyChanged();
            }
            get => myDilerEUR;
        }

        public decimal DilerGBP
        {
            set
            {
                if (Equals(value, myDilerGBP)) return;
                myDilerGBP = value;
                RaisePropertyChanged();
            }
            get => myDilerGBP;
        }

        public decimal DilerSEK
        {
            set
            {
                if (Equals(value, myDilerSEK)) return;
                myDilerSEK = value;
                RaisePropertyChanged();
            }
            get => myDilerSEK;
        }

        public decimal DilerCHF
        {
            set
            {
                if (Equals(value, myDilerCHF)) return;
                myDilerCHF = value;
                RaisePropertyChanged();
            }
            get => myDilerCHF;
        }

        /// <summary>
        ///     true если все цифры нулевые
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return ProfitRUB == 0 && LossRUB == 0 && DilerRUB == 0
                   && ProfitUSD == 0 && LossUSD == 0 && DilerUSD == 0
                   && ProfitEUR == 0 && LossEUR == 0 && DilerEUR == 0
                   && ProfitCHF == 0 && LossCHF == 0 && DilerCHF == 0
                   && ProfitGBP == 0 && LossGBP == 0 && DilerGBP == 0
                   && ProfitSEK == 0 && LossSEK == 0 && DilerSEK == 0;
        }

        private void UpdateFromDatePeriod(DatePeriod d)
        {
            Name = d.Name;
            Id = d.Id;
            ParentId = d.ParentId;
            PeriodType = d.PeriodType;
            DateStart = d.DateStart;
            DateEnd = d.DateEnd;
        }
    }
}
