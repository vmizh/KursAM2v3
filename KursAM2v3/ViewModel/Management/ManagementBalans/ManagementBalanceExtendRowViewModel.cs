using System;
using Core;
using Core.ViewModel.Base;
using KursDomain.Documents.CommonReferences;
using KursDomain.References;

namespace KursAM2.ViewModel.Management.ManagementBalans
{
    public class ManagementBalanceExtendRowViewModel : RSViewModelBase, ISummaMultiWithPriceCurrency
    {
        private Currency myCurrency;
        private string myCurrencyName;
        private DateTime myDate;
        private string myDocNum;
        private DocumentType myDocumentType;
        private Guid myGroupId;
        private Kontragent myKontragent;
        private string myKontragentName;
        private Nomenkl myNom;
        private string myNomenkl;
        private Employee myPersona;
        private decimal myPrice;
        private decimal myPriceCalc;
        private decimal myPriceCHF;
        private decimal myPriceCNY;
        private decimal myPriceEUR;
        private decimal myPriceGBP;
        private decimal myPriceRUB;
        private decimal myPriceSEK;
        private decimal myPriceUSD;
        private decimal myQuantity;
        private decimal mySumma;
        private decimal mySummaCalc;
        private decimal mySummaCHF;
        private decimal mySummaCNY;
        private decimal mySummaEUR;
        private decimal mySummaGBP;
        private decimal mySummaRUB;
        private decimal mySummaSEK;
        private decimal mySummaUSD;

        public DocumentType DocumentType
        {
            get => myDocumentType;
            set
            {
                if (myDocumentType == value) return;
                myDocumentType = value;
                RaisePropertyChanged();
            }
        }

        public string CurrencyName
        {
            get => myCurrencyName;
            set
            {
                if (myCurrencyName == value) return;
                myCurrencyName = value;
                RaisePropertyChanged();
            }
        }

        public Currency Currency
        {
            get => myCurrency;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrency == value) return;
                myCurrency = value;
                RaisePropertyChanged();
            }
        }

        public string KontragentName
        {
            get => myKontragentName;
            set
            {
                if (myKontragentName == value) return;
                myKontragentName = value;
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

        public string DocNum
        {
            get => myDocNum;
            set
            {
                if (myDocNum == value) return;
                myDocNum = value;
                RaisePropertyChanged();
            }
        }

        public Kontragent Kontragent
        {
            get => myKontragent;
            set
            {
                if (Equals(myKontragent,value)) return;
                myKontragent = value;
                RaisePropertyChanged();
            }
        }

        public Nomenkl Nom
        {
            get => myNom;
            set
            {
                if (Equals(myNom,value)) return;
                myNom = value;
                RaisePropertyChanged();
            }
        }

        public Employee Persona
        {
            get => myPersona;
            set
            {
                if (Equals(myPersona,value)) return;
                myPersona = value;
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

        public decimal Price
        {
            set
            {
                if (Equals(value, myPrice)) return;
                myPrice = value;
                RaisePropertyChanged();
            }
            get => myPrice;
        }

        public decimal Summa
        {
            set
            {
                if (Equals(value, mySumma)) return;
                mySumma = value;
                RaisePropertyChanged();
            }
            get => mySumma;
        }

        public decimal PriceCalc
        {
            set
            {
                if (Equals(value, myPriceCalc)) return;
                myPriceCalc = value;
                RaisePropertyChanged();
            }
            get => myPriceCalc;
        }

        public decimal SummaCalc
        {
            set
            {
                if (Equals(value, mySummaCalc)) return;
                mySummaCalc = value;
                RaisePropertyChanged();
            }
            get => mySummaCalc;
        }

        public decimal Quantity
        {
            set
            {
                if (Equals(value, myQuantity)) return;
                myQuantity = value;
                RaisePropertyChanged();
            }
            get => myQuantity;
        }

        public Guid GroupId
        {
            set
            {
                if (Equals(value, myGroupId)) return;
                myGroupId = value;
                RaisePropertyChanged();
            }
            get => myGroupId;
        }

        public decimal PriceGBP
        {
            get => myPriceGBP;
            set
            {
                if (myPriceGBP == value) return;
                myPriceGBP = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaGBP
        {
            get => mySummaGBP;
            set
            {
                if (mySummaGBP == value) return;
                mySummaGBP = value;
                RaisePropertyChanged();
            }
        }

        public decimal PriceCHF
        {
            get => myPriceCHF;
            set
            {
                if (myPriceCHF == value) return;
                myPriceCHF = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaCHF
        {
            get => mySummaCHF;
            set
            {
                if (mySummaCHF == value) return;
                mySummaCHF = value;
                RaisePropertyChanged();
            }
        }

        public decimal PriceSEK
        {
            get => myPriceSEK;
            set
            {
                if (myPriceSEK == value) return;
                myPriceSEK = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaSEK
        {
            get => mySummaSEK;
            set
            {
                if (mySummaSEK == value) return;
                mySummaSEK = value;
                RaisePropertyChanged();
            }
        }

        public decimal PriceRUB
        {
            set
            {
                if (Equals(value, myPriceRUB)) return;
                myPriceRUB = value;
                RaisePropertyChanged();
            }
            get => myPriceRUB;
        }

        public decimal SummaRUB
        {
            set
            {
                if (Equals(value, mySummaRUB)) return;
                mySummaRUB = value;
                RaisePropertyChanged();
            }
            get => mySummaRUB;
        }

        public decimal PriceUSD
        {
            set
            {
                if (Equals(value, myPriceUSD)) return;
                myPriceUSD = value;
                RaisePropertyChanged();
            }
            get => myPriceUSD;
        }

        public decimal SummaEUR
        {
            set
            {
                if (Equals(value, mySummaEUR)) return;
                mySummaEUR = value;
                RaisePropertyChanged();
            }
            get => mySummaEUR;
        }

        public decimal PriceEUR
        {
            set
            {
                if (Equals(value, myPriceEUR)) return;
                myPriceEUR = value;
                RaisePropertyChanged();
            }
            get => myPriceEUR;
        }

        public decimal SummaUSD
        {
            set
            {
                if (Equals(value, mySummaUSD)) return;
                mySummaUSD = value;
                RaisePropertyChanged();
            }
            get => mySummaUSD;
        }

        public decimal PriceCNY
        {
            set
            {
                if (Equals(value, myPriceCNY)) return;
                myPriceCNY = value;
                RaisePropertyChanged();
            }
            get => myPriceCNY;
        }

        public decimal SummaCNY
        {
            set
            {
                if (Equals(value, mySummaCNY)) return;
                mySummaCNY = value;
                RaisePropertyChanged();
            }

            get => mySummaCNY;
        }
    }
}
