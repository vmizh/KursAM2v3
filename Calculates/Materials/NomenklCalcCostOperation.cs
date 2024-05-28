using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using Core.ViewModel.Base;
using KursDomain.References;

namespace Calculates.Materials
{
    /// <summary>
    ///      Класс описывающий операцию движение товара
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
        private string mySenderReceiverName;

        [Display(AutoGenerateField = false)]
        public int RowNumber { set; get; }

        [Display(AutoGenerateField = false)]
        public ImageSource SenderReceiverIcon {
            get;
            set;
        }

        [Display(AutoGenerateField = false, Name = "Отправитель/получатель",Order = 4)]
        public string SenderReceiverName
        {
            get => mySenderReceiverName;
            set
            {
                if (mySenderReceiverName == value) return;
                mySenderReceiverName = value;
                RaisePropertyChanged();
            }
        }
        [Display(AutoGenerateField = false)]
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
        [Display(AutoGenerateField = false)]
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
        [Display(AutoGenerateField = false)]
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
        [Display(AutoGenerateField = false)]
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
        [Display(AutoGenerateField = false)]
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
        [Display(AutoGenerateField = true, Name = "Дата", Order = 0)]
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
        [Display(AutoGenerateField = true,Name = "Товарный документ",Order = 3)]
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

        [Display(AutoGenerateField = true, Name = "Финансовый документ",Order = 2)]
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
        [Display(AutoGenerateField = true, Name = "Операция",Order = 1)]
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
        [Display(AutoGenerateField = false)]
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
        [Display(AutoGenerateField = false)]
        public string SkladOutName => SkladOut?.Name;
        [Display(AutoGenerateField = false)]
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
        [Display(AutoGenerateField = false)]
        public string SkladInName => SkladIn?.Name;
        [Display(AutoGenerateField = false)]
        public Kontragent KontragentIn
        {
            get => myKontragentIn;
            set
            {
                if (Equals(myKontragentIn,value)) return;
                myKontragentIn = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(KontrInName));
            }
        }
        [Display(AutoGenerateField = false)]
        public string KontrInName => KontragentIn?.Name;
        [Display(AutoGenerateField = false)]
        public Kontragent KontragentOut
        {
            get => myKontragentOut;
            set
            {
                if (Equals(myKontragentOut,value)) return;
                myKontragentOut = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(KontrOutName));
            }
        }
        [Display(AutoGenerateField = false)]
        public string KontrOutName => myKontragentOut?.Name;

        [Display(AutoGenerateField = true, Name = "Цена", Order = 8)]
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

        [Display(AutoGenerateField = true, Name = "Накладные", Order = 14)]
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

        [Display(AutoGenerateField = true, Name = "Кол-во (приход)",Order = 5)]
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
        [Display(AutoGenerateField = true, Name = "Кол-во (расход)",Order = 6)]
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

        [Display(AutoGenerateField = true, Name = "Сумма прихода", Order = 9)]
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
        [Display(AutoGenerateField = true, Name = "Сумма расхода", Order = 10)]
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
        [Display(AutoGenerateField = true, Name = "Сумма прихода (с накл)",Order = 11)]
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
        [Display(AutoGenerateField = true, Name = "Сумма расхода (с накл)", Order = 12)]
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

        [Display(AutoGenerateField = true, Name = "Накопительно",Order = 7)]
        [DisplayFormat(DataFormatString = "n2")]
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
        [Display(AutoGenerateField = true, Name = "Себестоимость", Order = 13)]
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
        [Display(AutoGenerateField = true, Name = "Себестоимость с наклад.", Order = 15)]
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

        [Display(AutoGenerateField = true, Name = "Примечание", Order = 16)]
        public override string Note
        {
            get => myNote;
            set
            {
                if (myNote == value) return;
                myNote = value;
                RaisePropertyChanged();
            }
        }
    }
}
