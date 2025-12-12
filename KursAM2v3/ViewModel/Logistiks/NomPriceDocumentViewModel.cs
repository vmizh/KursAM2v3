using System;
using Core.ViewModel.Base;
using JetBrains.Annotations;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomPriceDocumentViewModel : RSViewModelBase
    {
        private DateTime myDocumentDate;
        private string myDocumentName;
        private string myDocumentNum;
        private string myFrom;
        private decimal myQuantityDelta;
        private decimal myQuantityIn;
        private decimal myQuantityOut;
        private decimal mySummaDelta;
        private decimal mySummaIn;
        private decimal mySummaOut;
        private string myTo;
        private decimal myNakopit;
        private string mySFDocumentNum;
        private DateTime? mySFDocumentDate;
        private decimal? mySFDocCode;

        public string DocumentName    
        {
            get => myDocumentName;
            set
            {
                if (myDocumentName == value) return;
                myDocumentName = value;
                RaisePropertyChanged();
            }
        }

        public string DocumentNum
        {
            get => myDocumentNum;
            set
            {
                if (myDocumentNum == value) return;
                myDocumentNum = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DocumentDate
        {
            get => myDocumentDate;
            set
            {
                if (myDocumentDate == value) return;
                myDocumentDate = value;
                RaisePropertyChanged();
            }
        }

        [CanBeNull]
        public string SFDocumentNum
        {
            get => mySFDocumentNum;
            set
            {
                if (mySFDocumentNum == value) return;
                mySFDocumentNum = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFDocCode
        {
            get => mySFDocCode;
            set
            {
                if (mySFDocCode == value) return;
                mySFDocCode = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? SFDocumentDate
        {
            get => mySFDocumentDate;
            set
            {
                if (mySFDocumentDate == value) return;
                mySFDocumentDate = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaIn
        {
            get => mySummaIn;
            set
            {
                if (mySummaIn == value) return;
                mySummaIn = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaOut
        {
            get => mySummaOut;
            set
            {
                if (mySummaOut == value) return;
                mySummaOut = value;
                RaisePropertyChanged();
            }
        }

        public decimal QuantityIn
        {
            get => myQuantityIn;
            set
            {
                if (myQuantityIn == value) return;
                myQuantityIn = value;
                RaisePropertyChanged();
            }
        }

        public decimal QuantityOut
        {
            get => myQuantityOut;
            set
            {
                if (myQuantityOut == value) return;
                myQuantityOut = value;
                RaisePropertyChanged();
            }
        }

        public decimal QuantityDelta
        {
            get => myQuantityDelta;
            set
            {
                if (myQuantityDelta == value) return;
                myQuantityDelta = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaDelta
        {
            get => mySummaDelta;
            set
            {
                if (mySummaDelta == value) return;
                mySummaDelta = value;
                RaisePropertyChanged();
            }
        }

        public decimal Nakopit
        {
            get => myNakopit;
            set
            {
                if (myNakopit == value) return;
                myNakopit = value;
                RaisePropertyChanged();
            }
        }

        public string From
        {
            get => myFrom;
            set
            {
                if (myFrom == value) return;
                myFrom = value;
                RaisePropertyChanged();
            }
        }

        public string To
        {
            get => myTo;
            set
            {
                if (myTo == value) return;
                myTo = value;
                RaisePropertyChanged();
            }
        }
    }
}
