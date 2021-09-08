using Core.EntityViewModel.Bank;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class GruzoInfoViewModel : RSViewModelBase
    {
        private string myAccGruzoOtprav;
        private string myAccGruzopol;

        private BankAccount myBankAccount;
        //= ent.TD_43 == null ? null : TD_43_ToBankAccount.ENTITY_To_ViewModel(ent.TD_43),

        private string myNaklGruzoOtprav;
        private string myNaklGruzoOtpravOKPO;
        private string myNaklGruzopol;
        private string myNaklGruzopolOKPO;
        private string myPlatDocText;
        private string myPlatelOKPO;
        private string myPlatelText;
        private string myPostavOKPO;
        private string myPostavText;

        public BankAccount BankAccount
        {
            get => myBankAccount;
            set
            {
                if (myBankAccount != null && myBankAccount.Equals(value)) return;
                myBankAccount = value;
                RaisePropertyChanged();
            }
        }

        public string AccGruzoOtprav
        {
            get => myAccGruzoOtprav;
            set
            {
                if (myAccGruzoOtprav == value) return;
                myAccGruzoOtprav = value;
                RaisePropertyChanged();
            }
        }

        public string PostavText
        {
            get => myPostavText;
            set
            {
                if (myPostavText == value) return;
                myPostavText = value;
                RaisePropertyChanged();
            }
        }

        public string AccGruzopol
        {
            get => myAccGruzopol;
            set
            {
                if (myAccGruzopol == value) return;
                myAccGruzopol = value;
                RaisePropertyChanged();
            }
        }

        public string NaklGruzoOtprav
        {
            get => myNaklGruzoOtprav;
            set
            {
                if (myNaklGruzoOtprav == value) return;
                myNaklGruzoOtprav = value;
                RaisePropertyChanged();
            }
        }

        public string NaklGruzoOtpravOKPO
        {
            get => myNaklGruzoOtpravOKPO;
            set
            {
                if (myNaklGruzoOtpravOKPO == value) return;
                myNaklGruzoOtpravOKPO = value;
                RaisePropertyChanged();
            }
        }

        public string NaklGruzopol
        {
            get => myNaklGruzopol;
            set
            {
                if (myNaklGruzopol == value) return;
                myNaklGruzopol = value;
                RaisePropertyChanged();
            }
        }

        public string NaklGruzopolOKPO
        {
            get => myNaklGruzopolOKPO;
            set
            {
                if (myNaklGruzopolOKPO == value) return;
                myNaklGruzopolOKPO = value;
                RaisePropertyChanged();
            }
        }

        public string PlatDocText
        {
            get => myPlatDocText;
            set
            {
                if (myPlatDocText == value) return;
                myPlatDocText = value;
                RaisePropertyChanged();
            }
        }

        public string PlatelOKPO
        {
            get => myPlatelOKPO;
            set
            {
                if (myPlatelOKPO == value) return;
                myPlatelOKPO = value;
                RaisePropertyChanged();
            }
        }

        public string PlatelText
        {
            get => myPlatelText;
            set
            {
                if (myPlatelText == value) return;
                myPlatelText = value;
                RaisePropertyChanged();
            }
        }

        public string PostavOKPO
        {
            get => myPostavOKPO;
            set
            {
                if (myPostavOKPO == value) return;
                myPostavOKPO = value;
                RaisePropertyChanged();
            }
        }
    }
}