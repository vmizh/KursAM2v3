using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Data;
using KursAM2.Repositories;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.References;

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
        [Display(AutoGenerateField = false)] public AccuredAmountOfSupplierRow AccruedAmountRow { set; get; }

        [Display(AutoGenerateField = true, Name = "Тип документа")]
        public string DocumentName
        {
            get
            {
                if (Invoice != null) return "Счет-фактура поставщика";
                if (AccruedAmountRow != null) return "Прямой расход";
                return null;
            }
        }

        [DisplayName("Номер")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public string DocNum
        {
            get
            {
                if (Invoice != null) return $"{Invoice?.SF_IN_NUM} / {Invoice?.SF_POSTAV_NUM}";
                if (AccruedAmountRow?.AccruedAmountOfSupplier != null)
                    return string.IsNullOrWhiteSpace(AccruedAmountRow?.AccruedAmountOfSupplier.DocExtNum)
                        ? $"{AccruedAmountRow?.AccruedAmountOfSupplier.DocInNum}"
                        : $"{AccruedAmountRow?.AccruedAmountOfSupplier.DocInNum} / {AccruedAmountRow?.AccruedAmountOfSupplier.DocExtNum} ";
                return null;
            }
        }

        [DisplayName("Дата сф")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public string DocDate
        {
            get
            {
                if (Invoice != null) return Invoice.SF_POSTAV_DATE.ToShortDateString();
                if (AccruedAmountRow?.AccruedAmountOfSupplier != null)
                    return AccruedAmountRow.AccruedAmountOfSupplier.DocDate.ToShortDateString();
                return null;
            }
        }


        [DisplayName("Поставщик")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public string ProviderName
        {
            get
            {
                if (Invoice != null) return MainReferences.GetKontragent(Invoice.SF_POST_DC).Name;
                if (AccruedAmountRow?.AccruedAmountOfSupplier != null)
                    return MainReferences.GetKontragent(AccruedAmountRow.AccruedAmountOfSupplier.KontrDC).Name;
                return null;
            }
        }

        [DisplayName("Сумма накладных")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public decimal? Summa
        {
            get
            {
                if (Invoice != null)
                    return Invoice.SF_KONTR_CRS_SUMMA;
                if (AccruedAmountRow != null)
                    return AccruedAmountRow.Summa;
                return 0;
            }
        }

        [DisplayName("Распределено")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public decimal SummaDistribute
        {
            get => Entity.SummaDistribute;
            set
            {
                Entity.SummaDistribute = Math.Round(value, 2);
                if (Invoice != null)
                    Invoice.NakladDistributedSumma = Math.Round(Entity.SummaDistribute, 2);
                RaisePropertiesChanged();
                RaisePropertiesChanged(nameof(SummaRemain));
            }
        }

        [DisplayName("Остаток")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public decimal SummaRemain => (Summa ?? 0) - SummaDistribute;

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

        [Display(Name = "Сумма(курс)")]
        [ReadOnly(true)]
        public decimal SummaRate => (Summa ?? 0) * Rate;

        [Display(Name = "Остаток(курс)")]
        [ReadOnly(true)]
        public decimal RemainRate => SummaRemain * Rate;

        #endregion
    }
}
