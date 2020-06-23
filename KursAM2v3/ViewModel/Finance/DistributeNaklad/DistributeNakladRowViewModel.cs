using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Data;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    public class DistributeNakladRowViewModel : KursBaseViewModel, IViewModelToEntity<DistributeNakladRow>
    {
        #region Fields 
        #endregion
        #region Constructors 
        public DistributeNakladRowViewModel(DistributeNakladRow entity, RowStatus state = RowStatus.NotEdited)
        {
            Entity = entity ?? new DistributeNakladRow
            {
                Id = Guid.NewGuid()
            };
            // ReSharper disable once VirtualMemberCallInConstructor
            Id = state == RowStatus.NewRow ? Guid.NewGuid() : Entity.Id;
            State = state;
        }
        #endregion
        #region Properties
        [DisplayName("Entity")]
        [Display(AutoGenerateField = false)]
        public DistributeNakladRow Entity
        {
            get => GetValue<DistributeNakladRow>();
            set => SetValue(value, () =>
            {
                SetChangeStatus();
            });
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
        public Guid TovarInvoiceRowId
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
        [Display(AutoGenerateField = true)]
        public InvoiceProvider Invoice
        {
            get => GetValue<InvoiceProvider>();
            set => SetValue(value);
        }


        [Display(AutoGenerateField = false)]
        public InvoiceProviderRow InvoiceRow
        {
            get => GetValue<InvoiceProviderRow>();
            set => SetValue(value);
        }

        [DisplayName("Ном.№")]
        [Display(AutoGenerateField = true)]
        public string NomNumber => InvoiceRow?.Nomenkl?.NomenklNumber;

        [DisplayName("Номенклатура")]
        [Display(AutoGenerateField = true)]
        public Nomenkl Nomenkl => InvoiceRow?.Nomenkl;

        [DisplayName("Кол-во")]
        [Display(AutoGenerateField = true)]
        public decimal Quantity => InvoiceRow?.SFT_KOL ?? 0;

        [DisplayName("Цена")]
        [Display(AutoGenerateField = true)]
        public decimal Price => Quantity != 0 ? (InvoiceRow?.SFT_SUMMA_K_OPLATE ?? 0)/Quantity : 0;

        [DisplayName("Сумма")]
        [Display(AutoGenerateField = true)]
        public decimal Summa => InvoiceRow?.SFT_SUMMA_K_OPLATE ?? 0;

        [DisplayName("Сумма накладных")]
        [Display(AutoGenerateField = true)]
        public decimal SummaNaklad
        {
            get => GetValue<decimal>();
            set => SetValue(value, () => SetChangeStatus());
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