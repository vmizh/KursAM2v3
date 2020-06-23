using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Data;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    public class DistributeNakladInfoViewModel : KursBaseViewModel, IViewModelToEntity<DistributeNakladInfo>
    {
        #region Fields 
        #endregion
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
                // ReSharper disable once VirtualMemberCallInConstructor
                Id = state == RowStatus.NewRow ? Guid.NewGuid() : Entity.Id;
                State = state;
            }
        }
        #endregion
        #region Properties
        [DisplayName("Entity")]
        [Display(AutoGenerateField = false)]
        public DistributeNakladInfo Entity
        {
            get => GetValue<DistributeNakladInfo>();
            set => SetValue(value, () =>
            {
                SetChangeStatus();
            });
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
        [DisplayName("InvoiceNakladId")]
        [Display(AutoGenerateField = false)]
        public Guid InvoiceNakladId
        {
            get => Entity.InvoiceNakladId;
            set
            {
                if (Entity.InvoiceNakladId == value) return;
                Entity.InvoiceNakladId = value;
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
            get => GetValue<decimal>();
            set => SetValue(value);
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
            get => Entity.Rate;
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