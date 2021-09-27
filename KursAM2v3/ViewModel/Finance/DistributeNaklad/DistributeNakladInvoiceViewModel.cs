using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Data;
using KursAM2.Repositories;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    public class DistributeNakladInvoiceViewModel : KursBaseViewModel,
        IViewModelToEntity<DistributeNakladInvoices>
    {
        #region Constructors

        public DistributeNakladInvoiceViewModel(DistributeNakladInvoices entity)
        {
            if (entity != null)
            {
                Entity = entity;
                State = RowStatus.NotEdited;
            }
            else
            {
                Entity = new DistributeNakladInvoices
                {
                    Id = Guid.NewGuid()
                };
                State = RowStatus.NewRow;
            }
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)] public DistributeNakladInvoices Entity { get; set; }

        [Display(AutoGenerateField = false)]
        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertiesChanged();
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
                RaisePropertiesChanged();
            }
        }

        [Display(Name = "Тип распределения")]
        public DistributeNakladRepository.DistributeNakladTypeEnum DistributeType
        {
            get => (DistributeNakladRepository.DistributeNakladTypeEnum)Entity.DistributeType;
            set
            {
                if (Entity.DistributeType == (short)value) return;
                Entity.DistributeType = (short)value;
                RaisePropertiesChanged();
            }
        }

        [Display(AutoGenerateField = false)] public SD_26 Invoice { set; get; }

        [DisplayName("Номер")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public string DocNum => $"{Invoice?.SF_IN_NUM} / {Invoice?.SF_POSTAV_NUM} ";

        [DisplayName("Дата сф")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public string DocDate => Invoice?.SF_POSTAV_DATE.ToShortDateString();


        [DisplayName("Поставщик")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public string ProviderName => Invoice != null
            ? MainReferences.GetKontragent(Invoice.SF_POST_DC).Name
            : null;


        [DisplayName("Сумма накладных")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public decimal? Summa
        {
            get => Invoice.SF_KONTR_CRS_SUMMA;
            set
            {
                if (Invoice.SF_KONTR_CRS_SUMMA == value) return;
                Invoice.SF_KONTR_CRS_SUMMA = value;
                RaisePropertiesChanged();
                RaisePropertiesChanged(nameof(SummaRemain));
            }
        }

        [DisplayName("Распределено")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public decimal? SummaDistribute
        {
            get => Invoice.NakladDistributedSumma;
            set
            {
                if (Invoice.NakladDistributedSumma == value) return;
                Invoice.NakladDistributedSumma = value;
                RaisePropertiesChanged();
                RaisePropertiesChanged(nameof(SummaRemain));
            }
        }

        [DisplayName("Остаток")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public decimal SummaRemain => (Summa ?? 0) - (SummaDistribute ?? 0);

        [DisplayName("Валюта")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        // ReSharper disable once PossibleInvalidOperationException
        public Currency Currency
        {
            get => MainReferences.GetCurrency(Entity.CrsDC);
            set
            {
                if (value == null) return;
                if (Entity.CrsDC == value.DocCode) return;
                Entity.CrsDC = value.DocCode;
                RaisePropertiesChanged();
            }
        }

        [DisplayName("Курс")]
        [Display(AutoGenerateField = true)]
        // ReSharper disable once PossibleInvalidOperationException
        public decimal Rate
        {
            get => Entity.Rate;
            set
            {
                if (Entity.Rate == value) return;
                Entity.Rate = value;
                RaisePropertiesChanged();
            }
        }

        #endregion
    }
}