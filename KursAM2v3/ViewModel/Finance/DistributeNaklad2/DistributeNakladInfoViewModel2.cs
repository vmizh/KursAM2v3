using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel.AccruedAmount;
using Core.EntityViewModel.CommonReferences;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Data;

namespace KursAM2.ViewModel.Finance.DistributeNaklad2
{
    public class DistributeNakladInfoViewModel2 : RSWindowViewModelBase, IDataErrorInfo, IViewModelToEntity<DistributeNakladInfo>
    {
        #region Fields

        private InvoiceProvider myInvoiceProvider;
        private AccruedAmountOfSupplierViewModel myAccruedAmount;

        #endregion

        #region Constructors

        public DistributeNakladInfoViewModel2(DistributeNakladInfo entity)
        {
            Entity = entity ?? DefaultValue();
            LoadReference();
        }

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
        public override Guid RowId
        {
            get => Entity.RowId;
            set
            {
                if (Entity.RowId == value) return;
                Entity.RowId = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceProvider InvoiceProvider
        {
            get => myInvoiceProvider;
            set
            {
                if (Entity.InvoiceNakladId == value?.Id) return;
                myInvoiceProvider = value;
                Entity.InvoiceNakladId = value?.Id;
                RaisePropertyChanged();
            }
        }

        public Currency Currency 
            { get => MainReferences.GetCurrency(Entity.InvoiceCrsDC);
                set
                {
                    if (Entity.InvoiceCrsDC == value.DocCode) return;
                    Entity.InvoiceCrsDC = value.DocCode;
                    RaisePropertyChanged();
                } }

        public decimal Rate
        {
            get => Entity.Rate;
            set
            {
                if(Entity.Rate == value) return;
                Entity.Rate = value;
                RaisePropertyChanged();
            }
        }

        public decimal DistributeSumma
        {
            get => Entity.DistributeSumma;
            set
            {
                if(Entity.DistributeSumma == value) return;
                Entity.DistributeSumma = value;
                RaisePropertyChanged();
            }
        }

        public decimal NakladSumma
        {
            get => Entity.NakladSumma ?? 0;
            set
            {
                if(Entity.NakladSumma == value) return;
                Entity.NakladSumma = value;
                RaisePropertyChanged();
            }
        }

        public AccruedAmountOfSupplierViewModel AccruedAmount
        {
            get => myAccruedAmount;
            set
            {
                if (Entity.AccrualAmountId == value?.Id) return;
                myAccruedAmount = value;
                Entity.AccrualAmountId = value?.Id;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public Guid FinanceDocId
        {
            get => Entity.FinanceDocId ?? Guid.Empty;
            set
            {
                if (Entity.FinanceDocId == value) return;
                Entity.FinanceDocId = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        #endregion

        #region Methods

        private DistributeNakladInfo DefaultValue()
        {
            return new DistributeNakladInfo()
            {
                Id = Guid.Empty
            };
        }
        private void LoadReference()
        {
            if (Entity.AccuredAmountOfSupplierRow != null)
                AccruedAmount = new AccruedAmountOfSupplierViewModel(Entity.AccuredAmountOfSupplierRow.AccruedAmountOfSupplier, null);
        }

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

        [Display(AutoGenerateField = false)]
        public string Error => "";
        #endregion

        public DistributeNakladInfo Entity { get; set; }
    }
}