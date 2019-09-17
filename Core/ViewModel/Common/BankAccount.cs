using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.ViewModel.Common
{
    public class BankAccount : RSViewModelData, IEntity<SD_114>
    {
        private string myAccount;
        private decimal myBankDC;
        private string myBankName;
        private string myBIK;
        private string myCorrAccount;
        private int myKontrBankCode;
        public string FullName => $"Сч.№ {Account} в {BankName} К/сч. {CorrAccount} ";
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
        public bool IsAccessRight { get; set; }

        public List<SD_114> LoadList()
        {
            return null;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}