using System.Collections.Generic;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Data;

namespace Core.ViewModel.Common
{
    public class BankAccount : RSViewModelBase, IEntity<SD_114>
    {
        private string myAccount;
        private Bank myBank;
        private decimal myBankDC;
        private string myBankName;
        private string myBIK;
        private string myCorrAccount;
        private int myKontrBankCode;

        public BankAccount()
        {
        }

        public BankAccount(SD_114 entity)
        {
            if (entity.SD_44 != null)
                Bank = new Bank(entity.SD_44);
            Name = $"{Bank?.Name} Сч.№ {entity.BA_RASH_ACC} " +
                   $"{MainReferences.Currencies[(decimal) entity.CurrencyDC]}";
        }

        public string FullName => $"{Bank?.Name} Сч.№ {Account} {Currency} ";
        public decimal BankDC
        {
            get => myBankDC;
            set
            {
                if (myBankDC == value) return;
                myBankDC = value;
                RaisePropertyChanged();
            }
        }
        public Bank Bank
        {
            get => myBank;
            set
            {
                if (myBank == value) return;
                myBank = value;
                RaisePropertyChanged();
            }
        }
        public int KontrBankCode
        {
            get => myKontrBankCode;
            set
            {
                if (myKontrBankCode == value) return;
                myKontrBankCode = value;
                RaisePropertyChanged();
            }
        }
        public string BankName
        {
            get => myBankName;
            set
            {
                if (myBankName == value) return;
                myBankName = value;
                RaisePropertyChanged();
            }
        }
        public string CorrAccount
        {
            get => myCorrAccount;
            set
            {
                if (myCorrAccount == value) return;
                myCorrAccount = value;
                RaisePropertyChanged();
            }
        }
        public string BIK
        {
            get => myBIK;
            set
            {
                if (myBIK == value) return;
                myBIK = value;
                RaisePropertyChanged();
            }
        }
        public string Account
        {
            get => myAccount;
            set
            {
                if (myAccount == value) return;
                myAccount = value;
                RaisePropertyChanged();
            }
        }
        public Currency Currency { get; set; }
        public bool IsAccessRight { get; set; }

        public List<SD_114> LoadList()
        {
            return null;
        }

        public SD_114 DefaultValue()
        {
            return new SD_114
            {
                DOC_CODE = -1
            };
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}