using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Data;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    public class DistributeNakladInfoViewModel : KursBaseViewModel, IViewModelToEntity<DistributeNakladInfo>
    {
        #region Constructors

        public DistributeNakladInfoViewModel(DistributeNakladInfo entity, RowStatus state = RowStatus.NotEdited)
        {
            if (entity == null)
            {
                Entity = new DistributeNakladInfo
                {
                    Id = Guid.NewGuid()
                };
                State = RowStatus.NewRow;
            }
            else
            {
                Entity = entity;
                // ReSharper disable once VirtualMemberCallInConstructor
                Id = state == RowStatus.NewRow ? Guid.NewGuid() : Entity.Id;
                State = state;
            }
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

        [DisplayName("Entity")]
        [Display(AutoGenerateField = false)]
        public DistributeNakladInfo Entity
        {
            get => GetValue<DistributeNakladInfo>();
            set => SetValue(value, () => { SetChangeStatus(); });
        }

        [DisplayName("RowId")]
        [Display(AutoGenerateField = false)]
        public Guid RowId
        {
            get => Entity.RowId;
            set
            {
                if (Entity.RowId == value) return;
                Entity.RowId = value;
                SetChangeStatus();
            }
        }

        [DisplayName("FinanceDocId")]
        [Display(AutoGenerateField = false)]
        public Guid? FinanceDocId
        {
            get => Entity.FinanceDocId;
            set
            {
                if (Entity.FinanceDocId == value) return;
                Entity.FinanceDocId = value;
                SetChangeStatus();
            }
        }

        [DisplayName("InvoiceNakladId")]
        [Display(AutoGenerateField = false)]
        public Guid? InvoiceNakladId
        {
            get => Entity.InvoiceNakladId;
            set
            {
                if (Entity.InvoiceNakladId == value) return;
                Entity.InvoiceNakladId = value;
                SetChangeStatus();
            }
        }

        [DisplayName("InvoiceNakladId")]
        [Display(AutoGenerateField = false)]
        public Guid? AccrualAmountId
        {
            get => Entity.AccrualAmountId;
            set
            {
                if (Entity.AccrualAmountId == value) return;
                Entity.AccrualAmountId = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Счет-фактура накладных")]
        [Display(AutoGenerateField = true)]
        public string InvoiceInfo
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [DisplayName("Сумма накладных")]
        [Display(AutoGenerateField = true)]
        public decimal NakladSumma
        {
            get => Entity.NakladSumma ?? 0;
            set
            {
                if (Entity.NakladSumma == value) return;
                Entity.NakladSumma = value;
                RaisePropertiesChanged();
            }
        }

        [DisplayName("InvoiceCrsDC")]
        [Display(AutoGenerateField = false)]
        public decimal InvoiceCrsDC
        {
            get => Entity.InvoiceCrsDC;
            set
            {
                if (Entity.InvoiceCrsDC == value) return;
                Entity.InvoiceCrsDC = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Валюта")]
        [Display(AutoGenerateField = true)]
        // ReSharper disable once PossibleInvalidOperationException
        public Currency Currency => MainReferences.Currencies[Entity.InvoiceCrsDC];

        [DisplayName("Курс")]
        [Display(AutoGenerateField = true)]
        public decimal Rate
        {
            get => Entity.Rate == 0 ? 1 : Entity.Rate;
            set
            {
                if (Entity.Rate == value) return;
                Entity.Rate = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Распределенная сумма")]
        [Display(AutoGenerateField = true)]
        public decimal DistributeSumma
        {
            get => Entity.DistributeSumma;
            set
            {
                if (Entity.DistributeSumma == value) return;
                Entity.DistributeSumma = value;
                SetChangeStatus();
            }
        }

        #endregion

        #region Methods

        #endregion

        #region Commands

        #endregion
    }
}