using System.Collections.Generic;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklRemainWithPrice : RSViewModelBase
    {
        private bool myIsAccepted;
        private bool myIsSelected;
        private Nomenkl myNomenkl;
        private decimal myPrice;
        private decimal myPriceWithNaklad;
        private decimal myQuantity;
        public Dictionary<string, decimal> SkladValues = new Dictionary<string, decimal>();

        public bool IsSelected
        {
            get => myIsSelected;
            set
            {
                if (myIsSelected == value) return;
                myIsSelected = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAccepted
        {
            get => myIsAccepted;
            set
            {
                if (myIsAccepted == value) return;
                myIsAccepted = value;
                RaisePropertyChanged();
            }
        }

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
        public Currency Currency => Nomenkl?.Currency;

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
    }
}