using System;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class ProviderRowWithNakladViewModel : RSWindowViewModelBase
    {
        private string myCurrency;
        private DateTime myDate;
        private string myInvoiceInfo;
        private string myOrderInfo;
        private decimal myPrice;
        private decimal myPriceWithNaklad;
        private decimal myQuantity;
        private decimal mySummaIn;
        private decimal mySummaNaklad;
        private decimal mySummaWithNaklad;
        private string myTypePrihodDocument;
        private decimal myUnitNaklad;

        public string InvoiceInfo
        {
            get => myInvoiceInfo;
            set
            {
                if (myInvoiceInfo == value) return;
                myInvoiceInfo = value;
                RaisePropertyChanged();
            }
        }

        public string Currency
        {
            get => myCurrency;
            set
            {
                if (myCurrency == value) return;
                myCurrency = value;
                RaisePropertyChanged();
            }
        }

        public string TypePrihodDocument
        {
            get => myTypePrihodDocument;
            set
            {
                if (myTypePrihodDocument == value) return;
                myTypePrihodDocument = value;
                RaisePropertyChanged();
            }
        }

        public string OrderInfo
        {
            get => myOrderInfo;
            set
            {
                if (myOrderInfo == value) return;
                myOrderInfo = value;
                RaisePropertyChanged();
            }
        }

        public decimal Price
        {
            get => myPrice;
            set
            {
                if (myPrice == value) return;
                myPrice = value;
                RaisePropertyChanged();
            }
        }

        public decimal UnitNaklad
        {
            get => myUnitNaklad;
            set
            {
                if (myUnitNaklad == value) return;
                myUnitNaklad = value;
                RaisePropertyChanged();
            }
        }

        public decimal Quantity
        {
            get => myQuantity;
            set
            {
                if (myQuantity == value) return;
                myQuantity = value;
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

        public decimal SummaNaklad
        {
            get => mySummaNaklad;
            set
            {
                if (mySummaNaklad == value) return;
                mySummaNaklad = value;
                RaisePropertyChanged();
            }
        }

        public decimal PriceWithNaklad
        {
            get => myPriceWithNaklad;
            set
            {
                if (myPriceWithNaklad == value) return;
                myPriceWithNaklad = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaWithNaklad
        {
            get => mySummaWithNaklad;
            set
            {
                if (mySummaWithNaklad == value) return;
                mySummaWithNaklad = value;
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
    }
}