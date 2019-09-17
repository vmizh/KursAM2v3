using Core.EntityViewModel;
using Data;

namespace Core.ViewModel.Common
{
    public class KontragentBank : TD_43ViewModel
    {
        private EntityViewModel.Bank myBank;

        public KontragentBank()
        {
            Entity = new TD_43 {DOC_CODE = -1, CODE = -1};
        }

        public KontragentBank(TD_43 entity) : base(entity)
        {
            if (entity == null)
                Entity = new TD_43 {DOC_CODE = -1, CODE = -1};
            else if (entity.SD_44 != null)
                Bank = new EntityViewModel.Bank(entity.SD_44);
        }

        public EntityViewModel.Bank Bank
        {
            get => myBank;
            set
            {
                if (myBank != null && myBank.Equals(value)) return;
                myBank = value;
                BANK_DC = value?.DocCode;
                RaisePropertyChanged();
            }
        }
    }
}