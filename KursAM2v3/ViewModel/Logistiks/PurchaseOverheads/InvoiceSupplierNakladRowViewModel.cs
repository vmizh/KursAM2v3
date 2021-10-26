using System;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Logistiks.PurchaseOverheads
{
    /// <summary>
    ///     Тип распределения накладных расходов
    /// </summary>
    public enum TypeDistribNakladOnNomenkl
    {
        /// <summary>
        ///     Цена*Кол-во
        /// </summary>
        PriceAndQuantity,

        /// <summary>
        ///     Только по цене
        /// </summary>
        PriceOnly,

        /// <summary>
        ///     Только по количеству
        /// </summary>
        QuantityOnly,

        /// <summary>
        ///     Вручную
        /// </summary>
        Custom
    }

    public class InvoiceSupplierNakladRowViewModel : RSViewModelBase
    {
        private Kontragent myKontragent;
        private Nomenkl myNomenkl;
        private decimal myRate;
        private Guid? mySchetRowId;
        private decimal mySumma;
        private decimal mySummaNotDistribute;
        private TypeDistribNakladOnNomenkl myTypeDistribute;

        // ReSharper disable once UnusedParameter.Local
        public InvoiceSupplierNakladRowViewModel(InvoiceProvider invoice)
        {
        }

        public Kontragent Kontragent
        {
            get => myKontragent;
            set
            {
                if (myKontragent != null && myKontragent.Equals(value)) return;
                myKontragent = value;
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

        public Guid? SchetRowId
        {
            get => mySchetRowId;
            set
            {
                if (mySchetRowId == value) return;
                mySchetRowId = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Сумма не распределенная из суммы счета (максимум для текущей суммы
        /// </summary>
        public decimal SummaNotDistribute
        {
            get => mySummaNotDistribute;
            set
            {
                if (mySummaNotDistribute == value) return;
                mySummaNotDistribute = value;
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

        public decimal Rate
        {
            get => myRate;
            set
            {
                if (myRate == value) return;
                myRate = value;
                RaisePropertyChanged();
            }
        }

        public TypeDistribNakladOnNomenkl TypeDistribute
        {
            get => myTypeDistribute;
            set
            {
                if (myTypeDistribute == value) return;
                myTypeDistribute = value;
                RaisePropertyChanged();
            }
        }
    }
}