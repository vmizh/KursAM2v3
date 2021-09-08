using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.CommonReferences.Kontragent
{
    public class KontragentBanks : RSViewModelBase, IEntity<TD_43>
    {
        #region Fields

        #endregion

        #region Constructors

        public KontragentBanks()
        {
            Entity = DefaultValue(-1);
        }

        public KontragentBanks(TD_43 entity, decimal parentDC)
        {
            Entity = entity ?? DefaultValue(parentDC);
        }

        private TD_43 DefaultValue(decimal dc)
        {
            return new TD_43
            {
                DOC_CODE = dc,
                CODE = -1
            };
        }

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)] public TD_43 Entity { get; set; }

        [Display(AutoGenerateField = false)]
        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public override int Code
        {
            get => Entity.CODE;
            set
            {
                if (Entity.CODE == value) return;
                Entity.CODE = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Счет №")]
        public string AccountNumber
        {
            get => Entity.RASCH_ACC;
            set
            {
                if (Entity.RASCH_ACC == value) return;
                Entity.RASCH_ACC = value;
                RaisePropertyChanged();
            }
        }


        [Display(AutoGenerateField = true, Name = "Удален")]
        public bool IsDeleted
        {
            get => (Entity.DELETED ?? 0) == 1;
            set
            {
                if ((Entity.DELETED ?? 0) == (short)(value ? 1 : 0)) return;
                Entity.DELETED = (short)(value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)] 
        public Bank.Bank BankAccount { get; set; }

        public string BankName => BankAccount?.Name;
        public string BankKorrAccount => BankAccount?.CORRESP_ACC;
        public string BankKPP => BankAccount?.POST_CODE;

        [Display(AutoGenerateField = true, Name = "Для счетов")]
        public bool IsForSchet
        {
            get => (Entity.USE_FOR_TLAT_TREB ?? 0) == 1;
            set
            {
                if ((Entity.USE_FOR_TLAT_TREB ?? 0) == (short)(value ? 1 : 0)) return;
                Entity.USE_FOR_TLAT_TREB = (short)(value ? 1 : 0);
                RaisePropertyChanged();
            }
        }



        #endregion

        #region Methods

        #endregion
    }
}