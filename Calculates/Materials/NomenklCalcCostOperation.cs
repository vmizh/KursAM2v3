using System;
using Core.ViewModel.Base;
using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.Documents.NomenklManagement;

namespace Calculates.Materials
{
    /// <summary>
    ///      ласс описывающий операцию движени€ товара
    /// </summary>
    public class NomenklCalcCostOperation : RSViewModelData
    {
        private decimal myCalcPrice;
        private decimal myCalcPriceNaklad;
        private DateTime myDocDate;
        private decimal myDocPrice;
        private string myFinDocument;
        private decimal? myFinDocumentDC;
        private Kontragent myKontragentIn;
        private Kontragent myKontragentOut;
        private decimal myNaklad;
        private decimal myNomenklDC;
        private string myOperationName;
        private int myOperCode;
        private decimal myQuantityIn;
        private decimal myQuantityNakopit;
        private decimal myQuantityOut;
        private Warehouse mySkladIn;
        private Warehouse mySkladOut;
        private decimal mySummaIn;
        private decimal mySummaInWithNaklad;
        private decimal mySummaOut;
        private decimal mySummaOutWithNaklad;
        private decimal? myTovarDocDC;
        private string myTovarDocument;
        private int myTovarRowCode;
        public int RowNumber { set; get; }

        public int OperCode
        {
            get => myOperCode;
            set
            {
                if (myOperCode == value) return;
                myOperCode = value;
                RaisePropertyChanged();
            }
        }

        public decimal NomenklDC
        {
            get => myNomenklDC;
            set
            {
                if (myNomenklDC == value) return;
                myNomenklDC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? TovarDocDC
        {
            get => myTovarDocDC;
            set
            {
                if (myTovarDocDC == value) return;
                myTovarDocDC = value;
                RaisePropertyChanged();
            }
        }

        public int TovarRowCode
        {
            get => myTovarRowCode;
            set
            {
                if (myTovarRowCode == value) return;
                myTovarRowCode = value;
                RaisePropertyChanged();
            }
        }

        public decimal? FinDocumentDC
        {
            get => myFinDocumentDC;
            set
            {
                if (myFinDocumentDC == value) return;
                myFinDocumentDC = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DocDate
        {
            get => myDocDate;
            set
            {
                if (myDocDate == value) return;
                myDocDate = value;
                RaisePropertyChanged();
            }
        }

        public string TovarDocument
        {
            get => myTovarDocument;
            set
            {
                if (myTovarDocument == value) return;
                myTovarDocument = value;
                RaisePropertyChanged();
            }
        }

        public string FinDocument
        {
            get => myFinDocument;
            set
            {
                if (myFinDocument == value) return;
                myFinDocument = value;
                RaisePropertyChanged();
            }
        }

        public string OperationName
        {
            get => myOperationName;
            set
            {
                if (myOperationName == value) return;
                myOperationName = value;
                RaisePropertyChanged();
            }
        }

        public Warehouse SkladOut
        {
            get => mySkladOut;
            set
            {
                if (mySkladOut != null && mySkladOut.Equals(value)) return;
                mySkladOut = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SkladOutName));
            }
        }

        public string SkladOutName => SkladOut?.Name;

        public Warehouse SkladIn
        {
            get => mySkladIn;
            set
            {
                if (mySkladIn != null && mySkladIn.Equals(value)) return;
                mySkladIn = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SkladInName));
            }
        }

        public string SkladInName => SkladIn?.Name;

        public Kontragent KontragentIn
        {
            get => myKontragentIn;
            set
            {
                if (myKontragentIn != null && myKontragentIn.Equals(value)) return;
                myKontragentIn = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(KontrInName));
            }
        }

        public string KontrInName => KontragentIn?.Name;

        public Kontragent KontragentOut
        {
            get => myKontragentOut;
            set
            {
                if (myKontragentOut != null && myKontragentOut.Equals(value)) return;
                myKontragentOut = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(KontrOutName));
            }
        }

        public string KontrOutName => KontragentOut?.Name;

        public decimal DocPrice
        {
            get => myDocPrice;
            set
            {
                if (myDocPrice == value) return;
                myDocPrice = value;
                RaisePropertyChanged();
            }
        }

        public decimal Naklad
        {
            get => myNaklad;
            set
            {
                if (myNaklad == value) return;
                myNaklad = value;
                RaisePropertyChanged();
            }
        }

        public decimal QuantityIn
        {
            get => myQuantityIn;
            set
            {
                if (myQuantityIn == value) return;
                myQuantityIn = value;
                RaisePropertyChanged();
            }
        }

        public decimal QuantityOut
        {
            get => myQuantityOut;
            set
            {
                if (myQuantityOut == value) return;
                myQuantityOut = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaIn
        {
            get => mySummaIn;
            set
            {
                if (mySummaIn == value) return;
                mySummaIn = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaOut
        {
            get => mySummaOut;
            set
            {
                if (mySummaOut == value) return;
                mySummaOut = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaInWithNaklad
        {
            get => mySummaInWithNaklad;
            set
            {
                if (mySummaInWithNaklad == value) return;
                mySummaInWithNaklad = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaOutWithNaklad
        {
            get => mySummaOutWithNaklad;
            set
            {
                if (mySummaOutWithNaklad == value) return;
                mySummaOutWithNaklad = value;
                RaisePropertyChanged();
            }
        }

        public decimal QuantityNakopit
        {
            get => myQuantityNakopit;
            set
            {
                if (myQuantityNakopit == value) return;
                myQuantityNakopit = value;
                RaisePropertyChanged();
            }
        }

        public decimal CalcPrice
        {
            get => myCalcPrice;
            set
            {
                if (myCalcPrice == value) return;
                myCalcPrice = value;
                RaisePropertyChanged();
            }
        }

        public decimal CalcPriceNaklad
        {
            get => myCalcPriceNaklad;
            set
            {
                if (myCalcPriceNaklad == value) return;
                myCalcPriceNaklad = value;
                RaisePropertyChanged();
            }
        }
    }
}
