using System.Collections.ObjectModel;
using Data;
using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.References;
using BankAccount = KursDomain.Documents.Bank.BankAccount;

namespace KursAM2.ViewModel.Reference.Kontragent
{
    public sealed class KontragentViewModelReferenceViewModel : KontragentViewModel
    {
        private ObservableCollection<BankAccount> myBankCollection;
        private ObservableCollection<KontragentGruzoRequisite> myGruzoRequisities;

        public KontragentViewModelReferenceViewModel()
        {
            GruzoRequisities = new ObservableCollection<KontragentGruzoRequisite>();
            BankCollection = new ObservableCollection<BankAccount>();
        }

        // ReSharper disable once UnusedParameter.Local
        public KontragentViewModelReferenceViewModel(decimal docCode) : this()
        {
        }

        public KontragentViewModelReferenceViewModel(SD_43 entity) : base(entity)
        {
            GruzoRequisities = new ObservableCollection<KontragentGruzoRequisite>();
            BankCollection = new ObservableCollection<BankAccount>();
            if (Entity.SD_43_GRUZO != null)
                foreach (var g in Entity.SD_43_GRUZO)
                    GruzoRequisities.Add(new KontragentGruzoRequisite(g));
        }

        // ReSharper disable once ConvertToAutoProperty
        public new ObservableCollection<KontragentGruzoRequisite> GruzoRequisities
        {
            set
            {
                myGruzoRequisities = value;
                RaisePropertyChanged();
            }
            get => myGruzoRequisities;
        }

        public ObservableCollection<BankAccount> BankCollection
        {
            set
            {
                myBankCollection = value;
                RaisePropertyChanged();
            }
            get => myBankCollection;
        }

        public override string Name
        {
            get => Entity.NAME;
            set
            {
                if (Entity.NAME == value) return;
                Entity.NAME = value;
                RaisePropertyChanged();
            }
        }

        public bool IsOutBalans
        {
            get => Entity.FLAG_BALANS == 0;
            set
            {
                if (Entity.FLAG_BALANS == 0 == value) return;
                Entity.FLAG_BALANS = (short?)(value ? 0 : 1);
                RaisePropertyChanged();
            }
        }

        public override string Note
        {
            get => Entity.NOTES;
            set
            {
                if (Entity.NOTES == value) return;
                Entity.NOTES = value;
                RaisePropertyChanged();
            }
        }
    }
}
