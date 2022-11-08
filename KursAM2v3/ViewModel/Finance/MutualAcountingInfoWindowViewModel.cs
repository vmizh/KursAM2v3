using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.ViewModel.Base;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Grid;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Finance
{
    public class MutualAcountingInfoWindowViewModel : RSWindowViewModelBase
    {
        private DateTime myEndDate;
        private DateTime myStartDate;

        #region Constructors

        public MutualAcountingInfoWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        }

        #endregion

        private void loaddocuments()
        {
            Docs.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.SD_110.Include(_ => _.TD_110)
                    .Include(_ => _.SD_111).Where(_ => _.VZ_DATE >= DateStart && _.VZ_DATE <= DateEnd
                                                                              && _.SD_111.IsCurrencyConvert
                                                                              && _.CurrencyFromDC ==
                                                                              CurrentRate.DebetCurrency.DocCode
                                                                              && _.CurrencyToDC ==
                                                                              CurrentRate.CreditCurrency.DocCode)
                    .ToList();
                foreach (var d in data)
                {
                    var row = Docs.FirstOrDefault(_ => _.DocCode == d.DOC_CODE);
                    if (row != null)
                    {
                        // ReSharper disable PossibleInvalidOperationException
                        row.CreditSumma += (decimal) d.TD_110.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1)
                            .Sum(_ => _.VZT_CRS_SUMMA);
                       
                        row.DebetSumma += (decimal) d.TD_110.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0)
                            .Sum(_ => _.VZT_CRS_SUMMA);
                        row.Rate = calcRate(row.DebetCurrency, row.CreditCurrency, row.DebetSumma, row.CreditSumma);
                     // ReSharper restore PossibleInvalidOperationException
                    }
                    else
                    {
                        // ReSharper disable PossibleInvalidOperationException
                        var newrow = new MutualAcountingInfoDocumentViewModel
                        {
                            DocCode = d.DOC_CODE,
                            DocDate = d.VZ_DATE,
                            DocNum = d.VZ_NUM,
                            Note = d.VZ_NOTES,
                            CreditCurrency = MainReferences.Currencies[d.CurrencyToDC],
                            DebetCurrency = MainReferences.Currencies[d.CurrencyFromDC],
                            CreditSumma =
                                (decimal) d.TD_110.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1).Sum(_ => _.VZT_CRS_SUMMA),
                            DebetSumma =
                                (decimal) d.TD_110.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0).Sum(_ => _.VZT_CRS_SUMMA)
                        };
                        // ReSharper restore PossibleInvalidOperationException
                        foreach (var s in d.TD_110)
                            if (!string.IsNullOrEmpty(s.VZT_DOC_NOTES))
                                newrow.Note += " / " + s.VZT_DOC_NOTES;
                        newrow.Rate = calcRate(newrow.DebetCurrency, newrow.CreditCurrency, newrow.DebetSumma,
                            newrow.CreditSumma);
                        Docs.Add(newrow);
                    }
                }
            }

            RaisePropertyChanged(nameof(Docs));
        }

        #region Fields

        #endregion

        #region Properties

        public ObservableCollection<MutualAcountingInfoCurrenciesViewModel> Rates { set; get; } =
            new ObservableCollection<MutualAcountingInfoCurrenciesViewModel>();

        public ObservableCollection<MutualAcountingInfoDocumentViewModel> Docs { set; get; } =
            new ObservableCollection<MutualAcountingInfoDocumentViewModel>();

        public DateTime DateEnd
        {
            get
            {
                if (myEndDate == DateTime.MinValue)
                    DateEnd = DateTime.Today;
                return myEndDate;
            }
            set
            {
                if (myEndDate == value) return;
                myEndDate = value;
                if (myEndDate < DateStart)
                    DateStart = myEndDate;
                RaisePropertyChanged();
            }
        }

        public DateTime DateStart
        {
            get
            {
                if (myStartDate == DateTime.MinValue)
                    DateStart = DateTime.Today;
                return myStartDate;
            }
            set
            {
                if (myStartDate == value) return;
                myStartDate = value;
                if (myStartDate > DateEnd)
                    DateEnd = myStartDate;
                RaisePropertyChanged();
            }
        }

        private MutualAcountingInfoCurrenciesViewModel myCurrentRate;

        public MutualAcountingInfoCurrenciesViewModel CurrentRate
        {
            get => myCurrentRate;
            set
            {
                if (Equals(myCurrentRate,value)) return;
                myCurrentRate = value;
                if (myCurrentRate != null)
                    loaddocuments();
                RaisePropertyChanged();
            }
        }

        private MutualAcountingInfoDocumentViewModel myCurrentDocument;

        public MutualAcountingInfoDocumentViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (Equals(myCurrentDocument,value)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        private ColumnBase myCurrentColumn2;

        [Display(AutoGenerateField = false)]
        public ColumnBase CurrentColumn2
        {
            get => myCurrentColumn2;
            set
            {
                if (Equals(myCurrentColumn2, value)) return;
                myCurrentColumn2 = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override void DocumentOpen(object obj)
        {
        }

        public ICommand GetColumnSummaCommand2
        {
            get { return new Command(GetColumnSumma2, _ => true); }
        }

        private void GetColumnSumma2(object obj)
        {
            var tbl = obj as TableView;
            var col = tbl?.Grid.CurrentColumn;
            if (col == null) return;
            Clipboard.SetText(col.TotalSummaryText);
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            Rates.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.SD_110.Include(_ => _.TD_110)
                    .Include(_ => _.SD_111).Where(_ => _.VZ_DATE >= DateStart && _.VZ_DATE <= DateEnd).ToList();
                // ReSharper disable PossibleInvalidOperationException                                                              && _.SD_111.IsCurrencyConvert).ToList();
                foreach (var d in data)
                {
                    var row = Rates.FirstOrDefault(_ => _.CreditCurrency?.DocCode == d.CurrencyToDC
                                                        && _.DebetCurrency?.DocCode == d.CurrencyFromDC);
                    if (row != null)
                    {
                        row.CreditSumma += (decimal) d.TD_110.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1)
                            .Sum(_ => _.VZT_CRS_SUMMA);
                        row.DebetSumma += (decimal) d.TD_110.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0)
                            .Sum(_ => _.VZT_CRS_SUMMA);
                        row.Rate = calcRate(row.DebetCurrency, row.CreditCurrency, row.DebetSumma, row.CreditSumma);
                    }
                    else
                    {
                        var newrow = new MutualAcountingInfoCurrenciesViewModel
                        {
                            CreditCurrency = MainReferences.Currencies[d.CurrencyToDC],
                            DebetCurrency = MainReferences.Currencies[d.CurrencyFromDC],
                            CreditSumma =
                                (decimal) d.TD_110.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1).Sum(_ => _.VZT_CRS_SUMMA),
                            DebetSumma =
                                (decimal) d.TD_110.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0).Sum(_ => _.VZT_CRS_SUMMA)
                        };
                        newrow.Rate = calcRate(newrow.DebetCurrency, newrow.CreditCurrency, newrow.DebetSumma,
                            newrow.CreditSumma);
                        Rates.Add(newrow);
                    }
                }
                // ReSharper restore PossibleInvalidOperationException
            }
        }

        private decimal calcRate(Currency first, Currency second, decimal summafirst, decimal summasecond)
        {
            if (Equals(first,second)) return 1;
            if (summafirst == 0 || summasecond == 0) return 0;
            if (Equals(first,GlobalOptions.SystemProfile.NationalCurrency))
                return decimal.Round(summafirst / summasecond, 4);
            if (Equals(second,GlobalOptions.SystemProfile.NationalCurrency))
                return decimal.Round(summasecond / summafirst, 4);
            if (first.DocCode == CurrencyCode.USD) return decimal.Round(summafirst / summasecond, 4);
            if (second.DocCode == CurrencyCode.USD) return decimal.Round(summasecond / summafirst, 4);
            return decimal.Round(summafirst / summasecond, 4);
        }

        #endregion
    }

    [MetadataType(typeof(DataAnnotationsMutualAcountingInfoCurrenciesViewModel))]
    public class MutualAcountingInfoCurrenciesViewModel : RSViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        #endregion

        #region Properties

        public Currency DebetCurrency { set; get; }
        public Currency CreditCurrency { set; get; }
        public decimal Rate { set; get; }
        public decimal DebetSumma { set; get; }
        public decimal CreditSumma { set; get; }

        #endregion

        #region Commands

        #endregion
    }

    [MetadataType(typeof(DataAnnotationsMutualAcountingInfoDocumentViewModel))]
    public class MutualAcountingInfoDocumentViewModel : RSViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        #endregion

        #region Properties

        public DateTime DocDate { set; get; }
        public int DocNum { set; get; }
        public Currency DebetCurrency { set; get; }
        public Currency CreditCurrency { set; get; }
        public decimal Rate { set; get; }
        public decimal DebetSumma { set; get; }
        public decimal CreditSumma { set; get; }

        #endregion

        #region Commands

        #endregion
    }

    public class DataAnnotationsMutualAcountingInfoDocumentViewModel : DataAnnotationForFluentApiBase,
        IMetadataProvider<MutualAcountingInfoDocumentViewModel>
    {
        void IMetadataProvider<MutualAcountingInfoDocumentViewModel>.BuildMetadata(
            MetadataBuilder<MutualAcountingInfoDocumentViewModel> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата").ReadOnly();
            builder.Property(_ => _.DocNum).AutoGenerated().DisplayName("№").ReadOnly();
            builder.Property(_ => _.DebetCurrency).AutoGenerated().DisplayName("Валюта дебета").ReadOnly();
            builder.Property(_ => _.DebetSumma).AutoGenerated().DisplayName("Сумма дебета").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.CreditCurrency).AutoGenerated().DisplayName("Валюта кредита").ReadOnly();
            builder.Property(_ => _.CreditSumma).AutoGenerated().DisplayName("Сумма кредита").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.Rate).AutoGenerated().DisplayName("Курс").ReadOnly().DisplayFormatString("n4");
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечания").ReadOnly();
        }
    }

    public class DataAnnotationsMutualAcountingInfoCurrenciesViewModel : DataAnnotationForFluentApiBase,
        IMetadataProvider<MutualAcountingInfoCurrenciesViewModel>
    {
        void IMetadataProvider<MutualAcountingInfoCurrenciesViewModel>.BuildMetadata(
            MetadataBuilder<MutualAcountingInfoCurrenciesViewModel> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.DebetCurrency).AutoGenerated().DisplayName("Валюта дебета").ReadOnly();
            builder.Property(_ => _.DebetSumma).AutoGenerated().DisplayName("Сумма дебета").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.CreditCurrency).AutoGenerated().DisplayName("Валюта кредита").ReadOnly();
            builder.Property(_ => _.CreditSumma).AutoGenerated().DisplayName("Сумма кредита").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.Rate).AutoGenerated().DisplayName("Курс").ReadOnly().DisplayFormatString("n4");
        }
    }
}
