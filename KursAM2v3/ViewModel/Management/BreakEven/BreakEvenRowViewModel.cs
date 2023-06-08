using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using DevExpress.Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.Documents.CommonReferences;
using KursDomain.References;

namespace KursAM2.ViewModel.Management.BreakEven
{
    public class BreakEvenRow : RSViewModelBase
    {
        private readonly decimal myKontrOperSummaCrs = 0;
        private readonly decimal myNOMENKLOperSumWoRevalField;
        private readonly decimal mySummaOperNomenklField;
        [OptionalField] private CentrResponsibility myCentrOfResponsibilityField;
        [OptionalField] private string myCurrency;
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
        public CentrResponsibility CentrOfResponsibility
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
                if (Equals(myDateField, value)) return;
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
                if (Equals(myDilerSummaField, value)) return;
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
                if (Equals(myIsUslugaField, value)) return;
                myIsUslugaField = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public string Currency
        {
            get => myCurrency;
            set
            {
                if (myCurrency == value) return;
                myCurrency = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public decimal KontrSumma
        {
            get => myKontrSummaField;
            set
            {
                if (Equals(myKontrSummaField, value)) return;
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
                if (Equals(myKontrSummaCrsField, value)) return;
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
                if (Equals(myKontrSummaCrsField, value)) return;
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
                if (Equals(myPriceField, value)) return;
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
                if (Equals(myQuantityField, value)) return;
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
                if (Equals(mySummaNomenklField, value)) return;
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
                if (Equals(mySummaNomenklCrsField, value)) return;
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
                if (Equals(myNOMENKLSumWoRevalField, value)) return;
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
                if (Equals(mySummaOperNomenklField, value)) return;
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
                if (Equals(mySummaNomenklCrsField, value)) return;
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
                if (Equals(myNOMENKLOperSumWoRevalField, value)) return;
                myNOMENKLSumWoRevalField = value;
                RaisePropertyChanged();
            }
        }
    }

    public class DataAnnotationsCommonRow : DataAnnotationForFluentApiBase,
        IMetadataProvider<CommonRow>
    {
        void IMetadataProvider<CommonRow>.BuildMetadata(
            MetadataBuilder<CommonRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование").LocatedAt(1).ReadOnly();
            builder.Property(_ => _.CurrencyName).AutoGenerated().DisplayName("Валюта").LocatedAt(2).ReadOnly();
            builder.Property(_ => _.Quantity).NotAutoGenerated().DisplayName("Кол-во").LocatedAt(3).DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").LocatedAt(5).DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.Price).NotAutoGenerated().DisplayName("Цена").LocatedAt(4).DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.Cost).AutoGenerated().DisplayName("Себестоимость").LocatedAt(8).DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.Naklad).AutoGenerated().DisplayName("Себе-сть накл.").LocatedAt(7).DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.CostOne).AutoGenerated().DisplayName("Себе-сть ед.").LocatedAt(14).DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.NakladOne).AutoGenerated().DisplayName("Себе-сть ед. накл.").LocatedAt(13)
                .DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.DilerSumma).AutoGenerated().DisplayName("Дилерские").LocatedAt(10).DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.Result).AutoGenerated().LocatedAt(11).DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.CostWOReval).AutoGenerated().DisplayName("Себе-сть (факт)")
                .DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.CostWORevalOne).AutoGenerated().DisplayName("Себе-сть (факт) ед.").LocatedAt(6)
                .DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultWOReval).AutoGenerated().DisplayName("Результат (факт)").LocatedAt(12)
                .DisplayFormatString("n2").ReadOnly();
        }
    }

    #region Nested type: CommonRow

    [MetadataType(typeof(DataAnnotationsCommonRow))]
    public class CommonRow : RSViewModelBase
    {
        private decimal myResult;
        public decimal Quantity { set; get; }
        public decimal Summa { set; get; }
        public decimal Price { set; get; }
        public decimal Cost { set; get; }
        public decimal Naklad { set; get; }
        public decimal CostOne { set; get; }
        public decimal NakladOne { set; get; }
        public decimal DilerSumma { set; get; }
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

        // ReSharper disable once InconsistentNaming
        public decimal CostWOReval { set; get; }

        // ReSharper disable once InconsistentNaming
        public decimal CostWORevalOne { set; get; }

        // ReSharper disable once InconsistentNaming
        public decimal ResultWOReval { set; get; }
        public decimal Currency { set; get; }
        public string CurrencyName { set; get; }
        
    }

    #endregion

    public class DataAnnotationsDocumentRow : DataAnnotationForFluentApiBase,
        IMetadataProvider<DocumentRow>
    {
        void IMetadataProvider<DocumentRow>.BuildMetadata(
            MetadataBuilder<DocumentRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.DocType).AutoGenerated().LocatedAt(1).DisplayName("Тип документа").ReadOnly();
            builder.Property(_ => _.Currency).NotAutoGenerated();
            builder.Property(_ => _.Date).AutoGenerated().DisplayName("Дата").LocatedAt(0).ReadOnly();
            builder.Property(_ => _.NomenklNumber).AutoGenerated().DisplayName("Ном.№").LocatedAt(3).ReadOnly();
            builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Товар").LocatedAt(4).ReadOnly();
            builder.Property(_ => _.COName).AutoGenerated().DisplayName("Центр. отв.").LocatedAt(12).ReadOnly();
            builder.Property(_ => _.KontragentName).AutoGenerated().DisplayName("Контрагент").LocatedAt(2).ReadOnly();
            builder.Property(_ => _.OperCrsName).NotAutoGenerated().DisplayName("Валюта (опер)").ReadOnly();
            builder.Property(_ => _.ManagerName).AutoGenerated().DisplayName("Менеджер").LocatedAt(15).ReadOnly();
            builder.Property(_ => _.Quantity).AutoGenerated().DisplayName("Кол-во").LocatedAt(6).DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.Price).AutoGenerated().DisplayName("Цена").LocatedAt(5).DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.KontrSumma).AutoGenerated().DisplayName("Сумма счета").LocatedAt(7).DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.SummaNomenkl).AutoGenerated().DisplayName("Всего (товар)").LocatedAt(8).DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.KontrSummaCrs).AutoGenerated().DisplayName("Зачислено контрагенту").LocatedAt(10).DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.Schet).AutoGenerated().DisplayName("Счет").LocatedAt(13).ReadOnly();
            builder.Property(_ => _.DilerName).AutoGenerated().DisplayName("Дилер").LocatedAt(14).ReadOnly();
            builder.Property(_ => _.DilerSumma).AutoGenerated().DisplayName("Дилер сумма").LocatedAt(9).DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.NomenklSumWOReva).AutoGenerated().DisplayName("Результат").LocatedAt(11).DisplayFormatString("n2").ReadOnly();
        }
    }

    public class DataAnnotationsDocumentCrsRow : DataAnnotationForFluentApiBase,
        IMetadataProvider<DocumentCrsRow>
    {
        void IMetadataProvider<DocumentCrsRow>.BuildMetadata(
            MetadataBuilder<DocumentCrsRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.DocType).AutoGenerated().DisplayName("Тип документа").ReadOnly();
            builder.Property(_ => _.Currency).NotAutoGenerated();
            builder.Property(_ => _.Date).AutoGenerated().DisplayName("Дата").ReadOnly();
            builder.Property(_ => _.NomenklNumber).AutoGenerated().DisplayName("Ном.№").ReadOnly();
            builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Товар").ReadOnly();
            builder.Property(_ => _.COName).AutoGenerated().DisplayName("Центр. отв.").ReadOnly();
            builder.Property(_ => _.KontragentName).AutoGenerated().DisplayName("Контрагент").ReadOnly();
            builder.Property(_ => _.OperCrsName).NotAutoGenerated().DisplayName("Валюта (опер)").ReadOnly();
            builder.Property(_ => _.ManagerName).AutoGenerated().DisplayName("Менеджер").ReadOnly();
            builder.Property(_ => _.Quantity).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.Price).AutoGenerated().DisplayName("Цена").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.KontrSumma).AutoGenerated().DisplayName("Сумма счета").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.SummaNomenkl).AutoGenerated().DisplayName("Всего (товар)").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.KontrSummaCrs).AutoGenerated().DisplayName("Зачислено контрагенту").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.Schet).AutoGenerated().DisplayName("Счет").ReadOnly();
            builder.Property(_ => _.DilerName).AutoGenerated().DisplayName("Дилер").ReadOnly();
            builder.Property(_ => _.DilerSumma).AutoGenerated().DisplayName("Дилер сумма").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.NomenklSumWOReva).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultEUR).AutoGenerated().DisplayName("EUR").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultUSD).AutoGenerated().DisplayName("USD").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultRUB).AutoGenerated().DisplayName("RUB").DisplayFormatString("n2").ReadOnly();
        }
    }

    #region Nested type: DocumentRow

    [MetadataType(typeof(DataAnnotationsDocumentRow))]
    [DataContract]
    public class DocumentRow
    {
        public DocumentType DocType { set; get; }
        public decimal DocCode { set; get; }

        [DataMember]
        public string Currency { set; get; }

        [DataMember]
        public bool IsUsluga { set; get; }

        [DataMember]
        public DateTime Date { set; get; }

        [DataMember]
        public Nomenkl Nomenkl { set; get; }

        [DataMember]
        public string NomenklNumber => Nomenkl?.NomenklNumber;

        [DataMember]
        public string COName { set; get; }

        [DataMember]
        public string KontragentName { set; get; }

        [DataMember]
        public string OperCrsName { set; get; }

        [DataMember]
        public string ManagerName { set; get; }

        [DataMember]
        public decimal Quantity { set; get; }

        [DataMember]
        public decimal Price { set; get; }

       [DataMember]
        public decimal KontrSumma { set; get; }

        [DataMember]
        public decimal SummaNomenkl { set; get; }

        [DataMember]
        public decimal SummaOperNomenkl { set; get; }

         [DataMember]
        public decimal KontrSummaCrs { set; get; }

        [DataMember]
        public decimal SummaNomenklCrs { set; get; }

         [DataMember]
        public decimal SummaOperNomenklCrs { set; get; }

        [DataMember]
        public string Schet { set; get; }

        [DataMember]
        public string Naklad { set; get; }

        [DataMember]
        public string DilerName { set; get; }

         [DataMember]
        public decimal DilerSumma { set; get; }

        [DataMember]
        // ReSharper disable once InconsistentNaming
        public decimal NomenklSumWOReva { set; get; }

        [DataMember]
        public decimal KontrOperSummaCrs { set; get; }
    }

    [MetadataType(typeof(DataAnnotationsDocumentCrsRow))]
    [DataContract]
    public class DocumentCrsRow : DocumentRow
    {
        [DataMember]
        public decimal ResultRUB { set; get; }

        [DataMember]
        public decimal ResultUSD { set; get; }

        [DataMember]
        public decimal ResultEUR { set; get; }
    }

    #endregion

   
}
