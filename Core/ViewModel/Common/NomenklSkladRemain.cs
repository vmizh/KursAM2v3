using System;
using Core.EntityViewModel;
using Core.ViewModel.Base;

namespace Core.ViewModel.Common
{
    /// <summary>
    ///     Класс остатка товара на складе
    /// </summary>
    public class NomenklSkladRemain : RSViewModelData
    {
        private decimal myPrice;
        private decimal myPriceNaklad;
        private decimal myQuantity;
        private DateTime myRemainDate;
        private Warehouse mySklad;
        private decimal mySumma;
        private decimal mySummaWithNaklad;

        public DateTime RemainDate
        {
            get => myRemainDate;
            set
            {
                if (myRemainDate == value) return;
                myRemainDate = value;
                RaisePropertyChanged();
            }
        }

        public Warehouse Sklad
        {
            get => mySklad;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (mySklad == value) return;
                mySklad = value;
                RaisePropertyChanged();
            }
        }

        public string SkladName => mySklad?.Name;

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

        public decimal PriceNaklad
        {
            get => myPriceNaklad;
            set
            {
                if (myPriceNaklad == value) return;
                myPriceNaklad = value;
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
    }
}