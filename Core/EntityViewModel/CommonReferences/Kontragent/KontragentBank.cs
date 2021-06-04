using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.CommonReferences.Kontragent
{
    public class KontragentBank : RSViewModelBase, IEntity<TD_43>
    {
        private Bank.Bank myBank;
        private TD_43 myEntity;

        public KontragentBank()
        {
            Entity = new TD_43 {DOC_CODE = -1, CODE = -1};
        }

        public KontragentBank(TD_43 entity) 
        {
            if (entity == null)
                Entity = new TD_43
                {
                    DOC_CODE = -1, CODE = -1
                };
            else if (entity.SD_44 != null)
                Bank = new Bank.Bank(entity.SD_44);
        }

        public TD_43 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public Bank.Bank Bank
        {
            get => myBank;
            set
            {
                if (myBank != null && myBank.Equals(value)) return;
                myBank = value;
                Entity.BANK_DC = value?.DocCode;
                RaisePropertyChanged();
            }
        }

        public bool IsAccessRight { get; set; }
        public List<TD_43> LoadList()
        {
            throw new System.NotImplementedException();
        }
    }
}