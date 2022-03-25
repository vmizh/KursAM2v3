using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel.Invoices;
using Core.ViewModel.Base;
using Data;

namespace KursAM2.ViewModel.Finance.DistributeNaklad2
{
    public class DistributeNakladRowViewModel2 : RSWindowViewModelBase, IDataErrorInfo,
        IViewModelToEntity<DistributeNakladRow>
    {
        #region Constructors

        public DistributeNakladRowViewModel2(DistributeNakladRow entity)
        {
            Entity = entity ?? DefaultValue();
            LoadReference();
        }

        #endregion

        public DistributeNakladRow Entity { get; set; }

        #region Methods

        private DistributeNakladRow DefaultValue()
        {
            return new DistributeNakladRow
            {
                Id = Guid.Empty
            };
        }

        private void LoadReference()
        {
            throw new NotImplementedException();
        }

       #endregion

        #region Fields

        private InvoiceProviderRow myTovarInvoiceProviderRow;
        private InvoiceProviderRowCurrencyConvertViewModel myTransferInvoiceProviderRow;

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)]
        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public Guid DocId
        {
            get => Entity.DocId;
            set
            {
                if (Entity.DocId == value) return;
                Entity.DocId = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceProviderRow TovarInvoiceProviderRow
        {
            get => myTovarInvoiceProviderRow;
            set
            {
                if (Entity.TovarInvoiceRowId == value?.Id) return;
                myTovarInvoiceProviderRow = value;
                Entity.TovarInvoiceRowId = value?.Id;
                RaisePropertyChanged();
            }
        }

        public override string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Note = value;
                RaisePropertyChanged();
            }
        }

        public decimal Quantity
        {
            get => Entity.Quantity;
            set
            {
                if (Entity.Quantity == value) return;
                Entity.Quantity = value;
                RaisePropertyChanged();
            }
        }

        public decimal Price
        {
            get => Entity.Price;
            set
            {
                if (Entity.Price == value) return;
                Entity.Price = value;
                RaisePropertyChanged();
            }
        }

        public decimal Summa
        {
            get => Entity.Summa;
            set
            {
                if (Entity.Summa == value) return;
                Entity.Summa = value;
                RaisePropertyChanged();
            }
        }

        public decimal DistributeSumma
        {
            get => Entity.DistributeSumma;
            set
            {
                if (Entity.DistributeSumma == value) return;
                Entity.DistributeSumma = value;
                RaisePropertyChanged();
            }
        }

        public decimal DistributePrice
        {
            get => Entity.DistributePrice;
            set
            {
                if (Entity.DistributePrice == value) return;
                Entity.DistributePrice = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceProviderRowCurrencyConvertViewModel TransferInvoiceProviderRow
        {
            get => myTransferInvoiceProviderRow;
            set
            {
                if (Entity.TransferRowId == value?.Id) return;
                myTransferInvoiceProviderRow = value;
                Entity.TransferRowId = value?.Id;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        #endregion

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    //case "Warehouse":
                    //    return Warehouse == null ? "Склад должен быть обязательно выбран" : null;
                    default:
                        return null;
                }
            }
        }

        [Display(AutoGenerateField = false)] public string Error => "";

        #endregion
    }
}