using System;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Management.Projects
{
    public class ProjectPrihodDistributeRow : ProjectPrihodDocRow
    {
        private decimal myDistributeSumma;
        public Project myProject;
        private Guid myRowId;
        public decimal mySumma;

        public Project Project
        {
            get => myProject;
            set
            {
                if (myProject != null && myProject.Equals(value)) return;
                myProject = value;
                RaisePropertyChanged();
            }
        }

        public override Guid RowId
        {
            get => myRowId;
            set
            {
                if (myRowId == value) return;
                myRowId = value;
                RaisePropertyChanged();
            }
        }

        public new decimal Summa
        {
            get => mySumma;
            set
            {
                if (value < 0 || mySumma == value) return;
                if (!IsUsluga)
                {
                    mySumma = Price * Quantity;
                }
                else
                {
                    Quantity = 1;
                    mySumma = value;
                }

                mySumma = value;
                RaisePropertyChanged();
            }
        }

        public new decimal DistributeSumma
        {
            get => !IsUsluga ? Price * DistributeQuantity : myDistributeSumma;
            set
            {
                if (myDistributeSumma == value) return;
                myDistributeSumma = value;
                RaisePropertyChanged();
            }
        }
    }

    public class ProjectPrihodDocRow : RSViewModelBase
    {
        private decimal myAccountDC;
        private string myCreator;
        private DateTime myDate;
        private decimal myDistributeQuantity;
        private decimal myDistributeSumma;
        private bool myIsUsluga;
        private string myKontragent;
        private decimal? myNakladDC;
        private string myNomenkl;
        private string myNomenklNumber;
        private string myNumAccount;
        private string myNumNaklad;
        private decimal myPrice;
        private decimal myQuantity;
        private string myUnit;

        public decimal AccountDC
        {
            get => myAccountDC;
            set
            {
                if (myAccountDC == value) return;
                myAccountDC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? NakladDC
        {
            get => myNakladDC;
            set
            {
                if (myNakladDC == value) return;
                myNakladDC = value;
                RaisePropertyChanged();
            }
        }

        public string Kontragent
        {
            get => myKontragent;
            set
            {
                if (myKontragent == value) return;
                myKontragent = value;
                RaisePropertyChanged();
            }
        }

        public string Nomenkl
        {
            get => myNomenkl;
            set
            {
                if (myNomenkl == value) return;
                myNomenkl = value;
                RaisePropertyChanged();
            }
        }

        public string NomenklNumber
        {
            get => myNomenklNumber;
            set
            {
                if (myNomenklNumber == value) return;
                myNomenklNumber = value;
                RaisePropertyChanged();
            }
        }

        public string NumAccount
        {
            get => myNumAccount;
            set
            {
                if (myNumAccount == value) return;
                myNumAccount = value;
                RaisePropertyChanged();
            }
        }

        public string NumNaklad
        {
            get => myNumNaklad;
            set
            {
                if (myNumNaklad == value) return;
                myNumNaklad = value;
                RaisePropertyChanged();
            }
        }

        public DateTime Date
        {
            get => myDate;
            set
            {
                if (myDate == value) return;
                myDate = value;
                RaisePropertyChanged();
            }
        }

        public string Unit
        {
            get => myUnit;
            set
            {
                if (myUnit == value) return;
                myUnit = value;
                RaisePropertyChanged();
            }
        }

        public decimal Price
        {
            get => myPrice;
            set
            {
                if (myPrice == value) return;
                myPrice = value;
                RaisePropertyChanged();
            }
        }

        public decimal Quantity
        {
            get => myQuantity;
            set
            {
                if (myQuantity == value) return;
                myQuantity = value;
                RaisePropertyChanged();
            }
        }

        public decimal DistributeQuantity
        {
            get => myDistributeQuantity;
            set
            {
                if (myDistributeQuantity == value) return;
                myDistributeQuantity = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(DistributeSumma));
            }
        }

        public decimal Summa => Price * Quantity;

        public decimal DistributeSumma
        {
            get
            {
                if (!IsUsluga) return Price * DistributeQuantity;
                return myDistributeSumma;
            }
            set
            {
                if (myDistributeSumma == value) return;
                myDistributeSumma = value;
                RaisePropertyChanged();
            }
        }

        public string Creator
        {
            get => myCreator;
            set
            {
                if (myCreator == value) return;
                myCreator = value;
                RaisePropertyChanged();
            }
        }

        public bool IsUsluga
        {
            get => myIsUsluga;
            set
            {
                if (myIsUsluga == value) return;
                myIsUsluga = value;
                RaisePropertyChanged();
            }
        }
    }
}