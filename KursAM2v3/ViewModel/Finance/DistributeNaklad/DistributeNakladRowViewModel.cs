using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Data;
using JetBrains.Annotations;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    public class DistributeNakladRowViewModel : KursBaseViewModel, IViewModelToEntity<DistributeNakladRow>
    {
        #region Fields

        #endregion

        #region Constructors

        public DistributeNakladRowViewModel()
        {
            Entity = new DistributeNakladRow
            {
                Id = Guid.NewGuid()
            };
            State = RowStatus.NewRow;
        }

        public DistributeNakladRowViewModel([NotNull] DistributeNakladRow rent)
        {
            Entity = rent;
            State = RowStatus.NotEdited;
        }

        #endregion

        #region Properties

        [DisplayName("Convert")]
        [Display(AutoGenerateField = false)]
        public InvoiceProviderRowCurrencyConvertViewModel Convert { set; get; }
        
        [DisplayName("ConvertNomenkl")]
        [Display(AutoGenerateField = false)]
        public Nomenkl ConvertNomenkl => Convert != null ? MainReferences.GetNomenkl(Convert.NomenklId) : null;

        [DisplayName("Entity")]
        [Display(AutoGenerateField = false)]
        public DistributeNakladRow Entity
        {
            get => GetValue<DistributeNakladRow>();
            set => SetValue(value, () => { SetChangeStatus(); });
        }

        [DisplayName("Id")]
        [Display(AutoGenerateField = false)]
        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                SetChangeStatus();
            }
        }

        [DisplayName("DocId")]
        [Display(AutoGenerateField = false)]
        public Guid DocId
        {
            get => Entity.DocId;
            set
            {
                if (Entity.DocId == value) return;
                Entity.DocId = value;
                SetChangeStatus();
            }
        }

        [DisplayName("TovarInvoiceRowId")]
        [Display(AutoGenerateField = false)]
        public Guid? TovarInvoiceRowId
        {
            get => Entity.TovarInvoiceRowId;
            set
            {
                if (Entity.TovarInvoiceRowId == value) return;
                Entity.TovarInvoiceRowId = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Счет-фактура поставщика")]
        [Display(AutoGenerateField = false)]
        public InvoiceProvider Invoice
        {
            get => GetValue<InvoiceProvider>();
            set => SetValue(value);
        }

        [DisplayName("Документ")]
        [Display(AutoGenerateField = true)]
        [ReadOnly(true)]
        public string DocName => Invoice != null ? "С/фактура поставщика" : null;

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

        [Display(AutoGenerateField = false)]
        public InvoiceProviderRow InvoiceRow
        {
            get => GetValue<InvoiceProviderRow>();
            set => SetValue(value);
        }

        [DisplayName("Ном.№")]
        [Display(AutoGenerateField = true)]
        public string NomNumber => Convert == null ? InvoiceRow?.Nomenkl?.NomenklNumber : ConvertNomenkl.NomenklNumber;

        [DisplayName("Номенклатура")]
        [Display(AutoGenerateField = true)]
        public Nomenkl Nomenkl => Convert == null ? InvoiceRow?.Nomenkl : ConvertNomenkl;

        [DisplayName("Кол-во")]
        [Display(AutoGenerateField = true)]
        public decimal Quantity => Convert?.Quantity ?? (InvoiceRow?.SFT_KOL ?? 0);

        [DisplayName("Цена")]
        [Display(AutoGenerateField = true)]
        public decimal Price => Convert?.Price ?? (Quantity != 0 ? (InvoiceRow?.SFT_SUMMA_K_OPLATE ?? 0) / Quantity : 0);

        [DisplayName("Сумма")]
        [Display(AutoGenerateField = true),ReadOnly(true)]
        public decimal Summa => Convert?.Summa ?? InvoiceRow?.SFT_SUMMA_K_OPLATE ?? 0;

        [DisplayName("Сумма накладных")]
        [Display(AutoGenerateField = true),ReadOnly(true)]
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

        [DisplayName("Цена с накладными"),ReadOnly(true)]
        [Display(AutoGenerateField = true)]
        public decimal DistributePrice
        {
            get => Entity.DistributePrice;
            set
            {
                if (Entity.DistributePrice == value) return;
                Entity.DistributePrice = value;
                SetChangeStatus();
            }
        }


        [DisplayName("Примечания")]
        [Display(AutoGenerateField = true)]
        public string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
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