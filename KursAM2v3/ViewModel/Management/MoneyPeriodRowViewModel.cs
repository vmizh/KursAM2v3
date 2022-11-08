using System;
using Core.ViewModel.Base;
using KursDomain.Documents.CommonReferences;
using KursDomain.References;

namespace KursAM2.ViewModel.Management
{
   
    public class MoneyRemainsRow : RSViewModelBase
    {
        private Currency myCurrency;
        private decimal myEndSumma;
        private string myRemainsType;
        private decimal myStartSumma;

        public string RemainsType
        {
            get => myRemainsType;
            set
            {
                if (myRemainsType == value) return;
                myRemainsType = value;
                RaisePropertyChanged();
            }
        }

        public decimal StartSumma
        {
            get => myStartSumma;
            set
            {
                if (myStartSumma == value) return;
                myStartSumma = value;
                RaisePropertyChanged();
            }
        }

        public decimal EndSumma
        {
            get => myEndSumma;
            set
            {
                if (myEndSumma == value) return;
                myEndSumma = value;
                RaisePropertyChanged();
            }
        }

        public Currency Currency
        {
            get => myCurrency;
            set
            {
                if (Equals(myCurrency, value)) return;
                myCurrency = value;
                RaisePropertyChanged();
            }
        }
    }

    public class MoneyPeriodRowViewModel : RSViewModelBase
    {
        private Currency myCurrency;
        private DateTime myDate;
        private string myDocName;
        private DocumentType myDocumentType;
        private string myKontragent;
        private SDRSchet mySDRSchet;
        private SDRState mySDRState;

        /// <summary>
        ///     Поступление
        /// </summary>
        private decimal mySummaPrihod;

        /// <summary>
        ///     Выплата
        /// </summary>
        private decimal mySummaRashod;

        public DocumentType DocumentType
        {
            get => myDocumentType;
            set
            {
                if (myDocumentType == value) return;
                myDocumentType = value;
                RaisePropertyChanged();
            }
        }

        public DateTime Date
        {
            get => myDate;
            set
            {
                if (myDate == value) return;
                myDate = value;
                RaisePropertyChanged();
            }
        }

        public SDRState SDRState
        {
            get => mySDRState;
            set
            {
                if (Equals(mySDRState,value)) return;
                mySDRState = value;
                RaisePropertyChanged();
            }
        }

        public SDRSchet SDRSchet
        {
            get => mySDRSchet;
            set
            {
                if (Equals(mySDRSchet,value)) return;
                mySDRSchet = value;
                RaisePropertyChanged();
            }
        }

        public string Kontragent
        {
            get => myKontragent;
            set
            {
                if (myKontragent == value) return;
                myKontragent = value;
                RaisePropertyChanged();
            }
        }

        public string DocName
        {
            get => myDocName;
            set
            {
                if (myDocName == value) return;
                myDocName = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaPrihod
        {
            get => mySummaPrihod;
            set
            {
                if (mySummaPrihod == value) return;
                mySummaPrihod = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaRashod
        {
            get => mySummaRashod;
            set
            {
                if (mySummaRashod == value) return;
                mySummaRashod = value;
                RaisePropertyChanged();
            }
        }

        public Currency Currency
        {
            get => myCurrency;
            set
            {
                if (Equals(myCurrency, value)) return;
                myCurrency = value;
                RaisePropertyChanged();
            }
        }
    }
}
