using System;
using System.Runtime.Serialization;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using Core.ViewModel.Common;
using DevExpress.Data;

namespace KursAM2.ViewModel.Management.BreakEven
{
    public class BreakEvenRow : RSViewModelBase
    {
        private readonly decimal myKontrOperSummaCrs = 0;
        private readonly decimal myNOMENKLOperSumWoRevalField = 0;
        private readonly decimal mySummaOperNomenklField = 0;
        [OptionalField] private CentrOfResponsibility myCentrOfResponsibilityField;
        [OptionalField] private DateTime myDateField;
        [OptionalField] private string myDilerField;
        [OptionalField] private decimal myDilerSummaField;
        [OptionalField] private decimal myDocDC;
        [OptionalField] private DocumentType myDocType;
        [OptionalField] private bool myIsUslugaField;
        [OptionalField] private string myKontragentField;
        [OptionalField] private decimal myKontrSummaCrsField;
        [OptionalField] private decimal myKontrSummaField;
        [OptionalField] private string myManagerField;
        [OptionalField] private string myNakladField;
        [OptionalField] private Nomenkl myNomenklField;

        // ReSharper disable once InconsistentNaming
        [OptionalField] private decimal myNOMENKLSumWoRevalField;
        [OptionalField] private string myOperCrsNameField;
        [OptionalField] private decimal myPriceField;
        [OptionalField] private decimal myQuantityField;
        [OptionalField] private string mySchetField;
        [OptionalField] private decimal mySummaNomenklCrsField;
        [OptionalField] private decimal mySummaNomenklField;

        [DataMember]
        public decimal DocDC
        {
            get => myDocDC;
            set
            {
                if (myDocDC == value) return;
                myDocDC = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public DocumentType DocType
        {
            get => myDocType;
            set
            {
                if (myDocType == value) return;
                myDocType = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public CentrOfResponsibility CentrOfResponsibility
        {
            get => myCentrOfResponsibilityField;
            set
            {
                if (ReferenceEquals(myCentrOfResponsibilityField, value) != true)
                {
                    myCentrOfResponsibilityField = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Kontragent Kontr { set; get; }

        [DataMember]
        public DateTime Date
        {
            get => myDateField;
            set
            {
                if (myDateField.Equals(value)) return;
                myDateField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public string Diler
        {
            get => myDilerField;
            set
            {
                if (ReferenceEquals(myDilerField, value)) return;
                myDilerField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal DilerSumma
        {
            get => myDilerSummaField;
            set
            {
                if (myDilerSummaField.Equals(value)) return;
                myDilerSummaField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public bool IsUsluga
        {
            get => myIsUslugaField;
            set
            {
                if (myIsUslugaField.Equals(value)) return;
                myIsUslugaField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal KontrSumma
        {
            get => myKontrSummaField;
            set
            {
                if (myKontrSummaField.Equals(value)) return;
                myKontrSummaField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal KontrSummaCrs
        {
            get => myKontrSummaCrsField;
            set
            {
                if (myKontrSummaCrsField.Equals(value)) return;
                myKontrSummaCrsField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal KontrOperSummaCrs
        {
            get => myKontrOperSummaCrs;
            set
            {
                if (myKontrSummaCrsField.Equals(value)) return;
                myKontrSummaCrsField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public string Kontragent
        {
            get => myKontragentField;
            set
            {
                if (ReferenceEquals(myKontragentField, value)) return;
                myKontragentField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public string Manager
        {
            get => myManagerField;
            set
            {
                if (ReferenceEquals(myManagerField, value)) return;
                myManagerField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public string Naklad
        {
            get => myNakladField;
            set
            {
                if (ReferenceEquals(myNakladField, value)) return;
                myNakladField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public Nomenkl Nomenkl
        {
            get => myNomenklField;
            set
            {
                if (ReferenceEquals(myNomenklField, value)) return;
                myNomenklField = value;
                RaisePropertyChanged();
            }
        }

        public Currency OperCurrency { set; get; }

        [DataMember]
        public string OperCrsName
        {
            get => myOperCrsNameField;
            set
            {
                if (ReferenceEquals(myOperCrsNameField, value)) return;
                myOperCrsNameField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal Price
        {
            get => myPriceField;
            set
            {
                if (myPriceField.Equals(value)) return;
                myPriceField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal Quantity
        {
            get => myQuantityField;
            set
            {
                if (myQuantityField.Equals(value)) return;
                myQuantityField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public string Schet
        {
            get => mySchetField;
            set
            {
                if (ReferenceEquals(mySchetField, value)) return;
                mySchetField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal SummaNomenkl
        {
            get => mySummaNomenklField;
            set
            {
                if (mySummaNomenklField.Equals(value)) return;
                mySummaNomenklField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal SummaNomenklCrs
        {
            get => mySummaNomenklCrsField;
            set
            {
                if (mySummaNomenklCrsField.Equals(value)) return;
                mySummaNomenklCrsField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        // ReSharper disable once InconsistentNaming
        public decimal NomenklSumWOReval
        {
            get => myNOMENKLSumWoRevalField;
            set
            {
                if (myNOMENKLSumWoRevalField.Equals(value)) return;
                myNOMENKLSumWoRevalField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal SummaOperNomenkl
        {
            get => mySummaOperNomenklField;
            set
            {
                if (mySummaOperNomenklField.Equals(value)) return;
                mySummaNomenklField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal SummaOperNomenklCrs
        {
            get => mySummaNomenklCrsField;
            set
            {
                if (mySummaNomenklCrsField.Equals(value)) return;
                mySummaNomenklCrsField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        // ReSharper disable once InconsistentNaming
        public decimal NomenklOperSumWOReval
        {
            get => myNOMENKLOperSumWoRevalField;
            set
            {
                if (myNOMENKLOperSumWoRevalField.Equals(value)) return;
                myNOMENKLSumWoRevalField = value;
                RaisePropertyChanged();
            }
        }
    }

    #region Nested type: CommonRow

    public class CommonRow : RSViewModelBase
    {
        private decimal myResult;

        [GridColumnSummary(SummaryItemType.Count, "n0")]
        [GridColumnView("Кол-во", SettingsType.Decimal, ReadOnly = true)]
        public decimal Quantity { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Сумма", SettingsType.Decimal, ReadOnly = true)]
        public decimal Summa { set; get; }

        [GridColumnView("Цена", SettingsType.Decimal, ReadOnly = true)]
        public decimal Price { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Себестоимость", SettingsType.Decimal, ReadOnly = true)]
        public decimal Cost { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Себестоимость с накладными", SettingsType.Decimal, ReadOnly = true)]
        public decimal Naklad { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Дилерские", SettingsType.Decimal, ReadOnly = true)]
        public decimal DilerSumma { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Результат", SettingsType.Decimal, ReadOnly = true)]
        public decimal Result
        {
            set
            {
                if (myResult == value) return;
                myResult = value;
                RaisePropertyChanged();
            }
            get => myResult;
        }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Себестоимость(факт)", SettingsType.Decimal, ReadOnly = true)]
        // ReSharper disable once InconsistentNaming
        public decimal CostWOReval { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Результат (факт)", SettingsType.Decimal, ReadOnly = true)]
        // ReSharper disable once InconsistentNaming
        public decimal ResultWOReval { set; get; }
    }

    #endregion

    #region Nested type: DocumentRow

    [DataContract]
    public class DocumentRow
    {
        public DocumentType DocType { set; get; }
        public decimal DocCode { set; get; }

        [GridColumnView("Услуга", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public bool IsUsluga { set; get; }

        [GridColumnView("Дата", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public DateTime Date { set; get; }

        [GridColumnView("Товар", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public Nomenkl Nomenkl { set; get; }

        [GridColumnView("Nom.№", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string NomenklNumber => Nomenkl?.NomenklNumber;

        [GridColumnView("Центр. отв.", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string COName { set; get; }

        [GridColumnView("Контрагент", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string KontragentName { set; get; }

        [GridColumnView("Валюта (опер)", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string OperCrsName { set; get; }

        [GridColumnView("Менеджер", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string ManagerName { set; get; }

        [GridColumnSummary(SummaryItemType.Count, "n0")]
        [GridColumnView("Кол-во", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal Quantity { set; get; }

        [GridColumnView("Цена", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal Price { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Сумма счета", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal KontrSumma { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Всего (товар)", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal SummaNomenkl { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Всего (товар)", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal SummaOperNomenkl { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Зачислено контрагенту", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal KontrSummaCrs { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Себестоимость товара", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal SummaNomenklCrs { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Себестоимость товара", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal SummaOperNomenklCrs { set; get; }

        [GridColumnView("Счет", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string Schet { set; get; }

        [GridColumnView("Накладные", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string Naklad { set; get; }

        [GridColumnView("Дилер", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string DilerName { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Сумма (дилер)", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal DilerSumma { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Результат", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        // ReSharper disable once InconsistentNaming
        public decimal NomenklSumWOReva { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Результат", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal ResultRUB { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Результат", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal ResultUSD { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Результат", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal ResultEUR { set; get; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Зачислено контрагенту", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal KontrOperSummaCrs { set; get; }
    }

    #endregion

    #region Nested type: NomenklRow

    public class NomenklRow : CommonRow
    {
        [GridColumnView("Номенкл.№", SettingsType.Default, ReadOnly = true)]
        public string NomenklNumber { set; get; }
    }

    #endregion
}