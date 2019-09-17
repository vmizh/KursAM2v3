using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Core;
using Core.EntityViewModel;
using Core.Helper;
using Core.ViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using DevExpress.Mvvm.DataAnnotations;

namespace KursAM2.ViewModel.Management.ManagementBalans
{
    public class SummaCurrencies : RSViewModelBase, ISummaMultiCurrency
    {
        public Currency Currency { set; get; }
        public decimal Summa { set; get; }
        public decimal SummaUSD { set; get; }
        public decimal SummaEUR { set; get; }
        public decimal SummaGBP { set; get; }
        public decimal SummaCHF { set; get; }
        public decimal SummaSEK { set; get; }
        public decimal SummaRUB { set; get; }

        public void SetSumma(Currency crs, decimal summa)
        {
            Currency = crs;
            Summa = summa;
            switch (crs.Name)
            {
                case CurrencyCode.RUBName:
                case CurrencyCode.RURName:
                    SummaRUB = summa;
                    return;
                case CurrencyCode.USDName:
                    SummaUSD = summa;
                    return;
                case CurrencyCode.EURName:
                    SummaEUR = summa;
                    return;
                case CurrencyCode.GBPName:
                    SummaGBP = summa;
                    return;
                case CurrencyCode.SEKName:
                    SummaSEK = summa;
                    return;
                case CurrencyCode.CHFName:
                    SummaCHF = summa;
                    return;
            }
        }

        public void SetAddSumma(decimal summa)
        {
            Summa += summa;
            switch (Currency.Name)
            {
                case CurrencyCode.RUBName:
                case CurrencyCode.RURName:
                    SummaRUB += summa;
                    return;
                case CurrencyCode.USDName:
                    SummaUSD += summa;
                    return;
                case CurrencyCode.EURName:
                    SummaEUR += summa;
                    return;
                case CurrencyCode.GBPName:
                    SummaGBP += summa;
                    return;
                case CurrencyCode.SEKName:
                    SummaSEK += summa;
                    return;
                case CurrencyCode.CHFName:
                    SummaCHF += summa;
                    return;
            }
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ManagementBalanceGroupViewModel : SummaCurrencies
    {
        private decimal myRecalcCurrency;
        public decimal? ObjectDC { set; get; }
        public decimal SummaToCompare { set; get; }
        public decimal Result { set; get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Order { set; get; }
        public BalansSection Tag { set; get; }

        public decimal RecalcCurrency
        {
            get => myRecalcCurrency;
            set
            {
                if (myRecalcCurrency == value) return;
                myRecalcCurrency = value;
                RaisePropertyChanged();
            }
        }
    }

    public class ManagementBalansCompareGroupViewModel : SummaCompareCurrencies
    {
        public decimal? ObjectDC { set; get; }
        public decimal SummaToCompare { set; get; }
        public decimal Result { set; get; }
        public int Order { set; get; }
        public BalansSection Tag { get; set; }
        public decimal RecalcCurrency { set; get; }
    }

    public class SummaCompareCurrencies : SummaCurrencies
    {
        public decimal SummaUSD2 { set; get; }
        public decimal DeltaUSD => SummaUSD2 - SummaUSD;
        public decimal SummaRUB2 { set; get; }
        public decimal DeltaRUB => SummaRUB2 - SummaRUB;
        public decimal SummaEUR2 { set; get; }
        public decimal DeltaEUR => SummaEUR2 - SummaEUR;
        public decimal SummaGBP2 { set; get; }
        public decimal DeltaGBP => SummaGBP2 - SummaGBP;
        public decimal SummaCHF2 { set; get; }
        public decimal DeltaCHF => SummaCHF2 - SummaCHF;
        public decimal SummaSEK2 { set; get; }
        public decimal DeltaSEK => SummaSEK2 - SummaSEK;

        public bool IsDifferent => DeltaUSD != 0 || DeltaRUB != 0 || DeltaEUR != 0 || DeltaGBP != 0 || DeltaCHF != 0 ||
                                   DeltaSEK != 0;

        public void SetSumma(Currency crs, decimal summa, decimal summa2)
        {
            Currency = crs;
            Summa = summa;
            switch (crs.Name)
            {
                case CurrencyCode.RUBName:
                case CurrencyCode.RURName:
                    SummaRUB = summa;
                    SummaRUB2 = summa2;
                    return;
                case CurrencyCode.USDName:
                    SummaUSD = summa;
                    SummaUSD2 = summa2;
                    return;
                case CurrencyCode.EURName:
                    SummaEUR = summa;
                    SummaEUR2 = summa2;
                    return;
                case CurrencyCode.GBPName:
                    SummaGBP = summa;
                    SummaGBP2 = summa2;
                    return;
                case CurrencyCode.SEKName:
                    SummaSEK = summa;
                    SummaSEK2 = summa2;
                    return;
                case CurrencyCode.CHFName:
                    SummaCHF = summa;
                    SummaCHF2 = summa2;
                    return;
            }
        }

        public void SetAddSumma(decimal summa, decimal summa2)
        {
            Summa += summa;
            switch (Currency.Name)
            {
                case CurrencyCode.RUBName:
                case CurrencyCode.RURName:
                    SummaRUB += summa;
                    SummaRUB2 += summa2;
                    return;
                case CurrencyCode.USDName:
                    SummaUSD += summa;
                    SummaUSD2 += summa2;
                    return;
                case CurrencyCode.EURName:
                    SummaEUR += summa;
                    SummaEUR2 += summa2;
                    return;
                case CurrencyCode.GBPName:
                    SummaGBP += summa;
                    SummaGBP2 += summa2;
                    return;
                case CurrencyCode.SEKName:
                    SummaSEK += summa;
                    SummaSEK2 += summa2;
                    return;
                case CurrencyCode.CHFName:
                    SummaCHF += summa;
                    SummaCHF2 += summa2;
                    return;
            }
        }

        public void SetSumma2(Currency crs, decimal summa2)
        {
            if (Currency == null)
                Currency = crs;
            switch (crs.Name)
            {
                case CurrencyCode.RUBName:
                case CurrencyCode.RURName:
                    SummaRUB2 = summa2;
                    return;
                case CurrencyCode.USDName:
                    SummaUSD2 = summa2;
                    return;
                case CurrencyCode.EURName:
                    SummaEUR2 = summa2;
                    return;
                case CurrencyCode.GBPName:
                    SummaGBP2 = summa2;
                    return;
                case CurrencyCode.SEKName:
                    SummaSEK2 = summa2;
                    return;
                case CurrencyCode.CHFName:
                    SummaCHF2 = summa2;
                    return;
            }
        }
    }

    public class FinanseOperationItem : SummaCompareCurrencies
    {
        public DateTime Date { set; get; }
        public DocumentType DocumentType { set; get; }
        public string KontragentName { set; get; }
        public int? TabelNumber { set; get; }
        public decimal DocDC { set; get; }
        public decimal KontragentDC { set; get; }
        public new string Note { set; get; }
        public string CurrencyName { set; get; }
        public string DocNum { set; get; }
        public KontragentTypeEnum KontragentType { set; get; }
    }

    public class KontragentOperationsItemBase : KonragentBalansRowViewModel
    {
    }

    [MetadataType(typeof(DataAnnotationsKontragentOperationsItem))]
    public class KontragentOperationsItem : SummaCompareCurrencies
    {
    }

    public static class DataAnnotationsKontragentOperationsItem
    {
        public static void BuildMetadata(MetadataBuilder<KontragentOperationsItem> builder)
        {
            //builder.Property(_ => _.).DisplayName("").Description("").ReadOnly();
            builder.Property(_ => _.ParentId).NotAutoGenerated();
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.Parent).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.State).NotAutoGenerated();
            builder.Property(_ => _.StringId).NotAutoGenerated();
            builder.Property(_ => _.IsDifferent).NotAutoGenerated();
            builder.Property(_ => _.Name).NotAutoGenerated();
            builder.Property(_ => _.Summa).NotAutoGenerated();
            builder.Property(_ => _.Note).DisplayName("Примечание").Description("").ReadOnly();
            builder.Group("Документ")
                .ContainsProperty(_ => _.Currency)
                .ContainsProperty(_ => _.Note);
            builder.Property(_ => _.SummaRUB).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaRUB2)
                .DisplayName("На конец")
                .Description("На конец RUB")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaRUB)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("RUB")
                .ContainsProperty(_ => _.SummaRUB)
                .ContainsProperty(_ => _.SummaRUB2)
                .ContainsProperty(_ => _.DeltaRUB);
            builder.Property(_ => _.SummaUSD).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaUSD2)
                .DisplayName("На конец")
                .Description("На конец USD")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaUSD)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("USD")
                .ContainsProperty(_ => _.SummaUSD)
                .ContainsProperty(_ => _.SummaUSD2)
                .ContainsProperty(_ => _.DeltaUSD);
            builder.Property(_ => _.SummaEUR).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaEUR2)
                .DisplayName("На конец")
                .Description("На конец EUR")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaEUR)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("EUR")
                .ContainsProperty(_ => _.SummaEUR)
                .ContainsProperty(_ => _.SummaEUR2)
                .ContainsProperty(_ => _.DeltaEUR);
            builder.Property(_ => _.SummaGBP).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaGBP2)
                .DisplayName("На конец")
                .Description("На конец GBP")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaGBP)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("GBP")
                .ContainsProperty(_ => _.SummaGBP)
                .ContainsProperty(_ => _.SummaGBP2)
                .ContainsProperty(_ => _.DeltaGBP);
            builder.Property(_ => _.SummaCHF).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaCHF2)
                .DisplayName("На конец")
                .Description("На конец CHF")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaCHF)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("CHF")
                .ContainsProperty(_ => _.SummaCHF)
                .ContainsProperty(_ => _.SummaCHF2)
                .ContainsProperty(_ => _.DeltaCHF);
            builder.Property(_ => _.SummaSEK).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaSEK2)
                .DisplayName("На конец")
                .Description("На конец SEK")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaSEK)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("SEK")
                .ContainsProperty(_ => _.SummaSEK)
                .ContainsProperty(_ => _.SummaSEK2)
                .ContainsProperty(_ => _.DeltaSEK);
        }
    }

    [MetadataType(typeof(DataAnnotationsNomenklCompareBalansDeltaItem))]
    public class NomenklCompareBalansDeltaItem : SummaCompareCurrencies
    {
        public string NomenklName { set; get; }
        public string NomenklNumber { set; get; }
        public decimal NomenklDC { set; get; }
        public decimal StoreDC { set; get; }
        public decimal QuantityStart { set; get; }
        public decimal QuantityEnd { set; get; }
        public decimal QuantityDelta => QuantityEnd - QuantityStart;
        public decimal SummaStart { set; get; }
        public decimal SummaEnd { set; get; }
        public decimal PriceStart { set; get; }
        public decimal PriceEnd { set; get; }

        public ObservableCollection<NomenklCompareBalansOperation> NomenklOperations { set; get; } =
            new ObservableCollection<NomenklCompareBalansOperation>();

        public bool IsChanged()
        {
            return DeltaUSD != 0 || DeltaCHF != 0 || DeltaEUR != 0 || DeltaGBP != 0 || DeltaRUB != 0 || DeltaSEK != 0;
        }
    }

    public static class DataAnnotationsNomenklCompareBalansDeltaItem
    {
        public static void BuildMetadata(MetadataBuilder<NomenklCompareBalansDeltaItem> builder)
        {
            //builder.Property(_ => _.).DisplayName("").Description("").ReadOnly();
            builder.Property(_ => _.ParentId).NotAutoGenerated();
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.Parent).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.State).NotAutoGenerated();
            builder.Property(_ => _.StringId).NotAutoGenerated();
            builder.Property(_ => _.NomenklDC).NotAutoGenerated();
            builder.Property(_ => _.IsDifferent).NotAutoGenerated();
            builder.Property(_ => _.Name).NotAutoGenerated();
            builder.Property(_ => _.Summa).NotAutoGenerated();
            builder.Property(_ => _.StoreDC).NotAutoGenerated();
            builder.Property(_ => _.Currency).DisplayName("Валюта").Description("Валюта операции").ReadOnly();
            builder.Property(_ => _.NomenklName).ReadOnly().DisplayName("Номенклатура").Description("");
            builder.Property(_ => _.NomenklNumber).ReadOnly().DisplayName("Ном.№").Description("");
            builder.Property(_ => _.SummaStart)
                .ReadOnly()
                .DisplayName("Сумма на начало")
                .NumericMask("n4");
            builder.Property(_ => _.PriceStart)
                .ReadOnly()
                .DisplayName("Цена на начало")
                .NumericMask("n4");
            builder.Property(_ => _.PriceEnd)
                .ReadOnly()
                .DisplayName("Цена на конец")
                .NumericMask("n4");
            builder.Property(_ => _.SummaEnd)
                .ReadOnly()
                .DisplayName("Сумма на конец")
                .NumericMask("n4");
            builder.Property(_ => _.QuantityStart)
                .ReadOnly()
                .DisplayName("Кол-во на начало")
                .NumericMask("n2");
            builder.Property(_ => _.QuantityEnd)
                .ReadOnly()
                .DisplayName("Кол-во на конец")
                .NumericMask("n2");
            builder.Property(_ => _.QuantityDelta)
                .ReadOnly()
                .DisplayName("Кол-во разница")
                .NumericMask("n2");
            builder.Property(_ => _.Note).DisplayName("Примечание").Description("").ReadOnly();
            builder.Group("Номенклатура")
                .ContainsProperty(_ => _.NomenklName)
                .ContainsProperty(_ => _.NomenklNumber)
                .ContainsProperty(_ => _.Currency)
                .ContainsProperty(_ => _.Note);
            builder.Group("Движение")
                .ContainsProperty(_ => _.QuantityStart)
                .ContainsProperty(_ => _.PriceStart)
                .ContainsProperty(_ => _.SummaStart)
                .ContainsProperty(_ => _.QuantityEnd)
                .ContainsProperty(_ => _.SummaEnd)
                .ContainsProperty(_ => _.PriceEnd)
                .ContainsProperty(_ => _.QuantityDelta);
            builder.Property(_ => _.SummaRUB).ReadOnly().DisplayName("На начало").Description("").NumericMask("n4");
            builder.Property(_ => _.SummaRUB2)
                .DisplayName("На конец")
                .Description("На конец RUB")
                .ReadOnly()
                .NumericMask("n4");
            builder.Property(_ => _.DeltaRUB)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n4");
            builder.Group("RUB")
                .ContainsProperty(_ => _.SummaRUB)
                .ContainsProperty(_ => _.SummaRUB2)
                .ContainsProperty(_ => _.DeltaRUB);
            builder.Property(_ => _.SummaUSD).ReadOnly().DisplayName("На начало").Description("").NumericMask("n4");
            builder.Property(_ => _.SummaUSD2)
                .DisplayName("На конец")
                .Description("На конец USD")
                .ReadOnly()
                .NumericMask("n4");
            builder.Property(_ => _.DeltaUSD)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n4");
            builder.Group("USD")
                .ContainsProperty(_ => _.SummaUSD)
                .ContainsProperty(_ => _.SummaUSD2)
                .ContainsProperty(_ => _.DeltaUSD);
            builder.Property(_ => _.SummaEUR).ReadOnly().DisplayName("На начало").Description("").NumericMask("n4");
            builder.Property(_ => _.SummaEUR2)
                .DisplayName("На конец")
                .Description("На конец EUR")
                .ReadOnly()
                .NumericMask("n4");
            builder.Property(_ => _.DeltaEUR)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n4");
            builder.Group("EUR")
                .ContainsProperty(_ => _.SummaEUR)
                .ContainsProperty(_ => _.SummaEUR2)
                .ContainsProperty(_ => _.DeltaEUR);
            builder.Property(_ => _.SummaGBP).ReadOnly().DisplayName("На начало").Description("").NumericMask("n4");
            builder.Property(_ => _.SummaGBP2)
                .DisplayName("На конец")
                .Description("На конец GBP")
                .ReadOnly()
                .NumericMask("n4");
            builder.Property(_ => _.DeltaGBP)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n4");
            builder.Group("GBP")
                .ContainsProperty(_ => _.SummaGBP)
                .ContainsProperty(_ => _.SummaGBP2)
                .ContainsProperty(_ => _.DeltaGBP);
            builder.Property(_ => _.SummaCHF).ReadOnly().DisplayName("На начало").Description("").NumericMask("n4");
            builder.Property(_ => _.SummaCHF2)
                .DisplayName("На конец")
                .Description("На конец CHF")
                .ReadOnly()
                .NumericMask("n4");
            builder.Property(_ => _.DeltaCHF)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n4");
            builder.Group("CHF")
                .ContainsProperty(_ => _.SummaCHF)
                .ContainsProperty(_ => _.SummaCHF2)
                .ContainsProperty(_ => _.DeltaCHF);
            builder.Property(_ => _.SummaSEK).ReadOnly().DisplayName("На начало").Description("").NumericMask("n4");
            builder.Property(_ => _.SummaSEK2)
                .DisplayName("На конец")
                .Description("На конец SEK")
                .ReadOnly()
                .NumericMask("n4");
            builder.Property(_ => _.DeltaSEK)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n4");
            builder.Group("SEK")
                .ContainsProperty(_ => _.SummaSEK)
                .ContainsProperty(_ => _.SummaSEK2)
                .ContainsProperty(_ => _.DeltaSEK);
        }
    }

    [MetadataType(typeof(DataAnnotationsKontragentCompareBalansDeltaItem))]
    public class KontragentCompareBalansDeltaItem : SummaCompareCurrencies
    {
        public string KontragentName { set; get; }
        public decimal KontragentDC { set; get; }

        public ObservableCollection<ManagementBalansCompareWindowViewModel.KontragentBalansOperation>
            KontragentOperations { set; get; } =
            new ObservableCollection<ManagementBalansCompareWindowViewModel.KontragentBalansOperation>();

        public override string ToString()
        {
            return KontragentName + "(" + KontragentDC + ")";
        }
    }

    public static class DataAnnotationsKontragentCompareBalansDeltaItem
    {
        public static void BuildMetadata(MetadataBuilder<KontragentCompareBalansDeltaItem> builder)
        {
            //builder.Property(_ => _.).DisplayName("").Description("").ReadOnly();
            builder.Property(_ => _.ParentId).NotAutoGenerated();
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.Parent).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.State).NotAutoGenerated();
            builder.Property(_ => _.StringId).NotAutoGenerated();
            builder.Property(_ => _.KontragentDC).NotAutoGenerated();
            builder.Property(_ => _.IsDifferent).NotAutoGenerated();
            builder.Property(_ => _.Name).NotAutoGenerated();
            builder.Property(_ => _.KontragentOperations).NotAutoGenerated();
            builder.Property(_ => _.Currency).DisplayName("Валюта").Description("Валюта операции").ReadOnly();
            builder.Property(_ => _.KontragentName).ReadOnly().DisplayName("Котнрагент").Description("");
            builder.Property(_ => _.Summa).ReadOnly().DisplayName("Сумма").Description("Сумма операции");
            builder.Property(_ => _.Note).DisplayName("Примечание").Description("").ReadOnly();
            builder.Group("Контрагент")
                .ContainsProperty(_ => _.KontragentName)
                .ContainsProperty(_ => _.Note)
                .ContainsProperty(_ => _.Currency)
                .ContainsProperty(_ => _.Summa);
            builder.Property(_ => _.SummaRUB).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaRUB2)
                .DisplayName("На конец")
                .Description("На конец RUB")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaRUB)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("RUB")
                .ContainsProperty(_ => _.SummaRUB)
                .ContainsProperty(_ => _.SummaRUB2)
                .ContainsProperty(_ => _.DeltaRUB);
            builder.Property(_ => _.SummaUSD).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaUSD2)
                .DisplayName("На конец")
                .Description("На конец USD")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaUSD)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("USD")
                .ContainsProperty(_ => _.SummaUSD)
                .ContainsProperty(_ => _.SummaUSD2)
                .ContainsProperty(_ => _.DeltaUSD);
            builder.Property(_ => _.SummaEUR).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaEUR2)
                .DisplayName("На конец")
                .Description("На конец EUR")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaEUR)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("EUR")
                .ContainsProperty(_ => _.SummaEUR)
                .ContainsProperty(_ => _.SummaEUR2)
                .ContainsProperty(_ => _.DeltaEUR);
            builder.Property(_ => _.SummaGBP).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaGBP2)
                .DisplayName("На конец")
                .Description("На конец GBP")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaGBP)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("GBP")
                .ContainsProperty(_ => _.SummaGBP)
                .ContainsProperty(_ => _.SummaGBP2)
                .ContainsProperty(_ => _.DeltaGBP);
            builder.Property(_ => _.SummaCHF).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaCHF2)
                .DisplayName("На конец")
                .Description("На конец CHF")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaCHF)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("CHF")
                .ContainsProperty(_ => _.SummaCHF)
                .ContainsProperty(_ => _.SummaCHF2)
                .ContainsProperty(_ => _.DeltaCHF);
            builder.Property(_ => _.SummaSEK).ReadOnly().DisplayName("На начало").Description("").NumericMask("n2");
            builder.Property(_ => _.SummaSEK2)
                .DisplayName("На конец")
                .Description("На конец SEK")
                .ReadOnly()
                .NumericMask("n2");
            builder.Property(_ => _.DeltaSEK)
                .DisplayName("Результат")
                .Description("Результат")
                .ReadOnly()
                .NumericMask("n2");
            builder.Group("SEK")
                .ContainsProperty(_ => _.SummaSEK)
                .ContainsProperty(_ => _.SummaSEK2)
                .ContainsProperty(_ => _.DeltaSEK);
        }
    }

    public class ManagementBalansCompareRowViewModel : SummaCompareCurrencies
    {
        public decimal? QuantityStart { set; get; }
        public decimal? QuantityEnd { set; get; }
        public string CurrencyName { set; get; }
        public string Nomenkl { set; get; }
        public BalansSection Tag { set; get; }
        public decimal Delta => DeltaUSD + DeltaRUB + DeltaEUR + DeltaGBP + DeltaCHF + DeltaSEK;
    }

    public enum BalansSection
    {
        Unknown = 0,

        /// <summary>
        ///     Баланс главная строка
        /// </summary>
        Head = -1,

        /// <summary>
        ///     Основные средства
        /// </summary>
        MainInventory = 1,
        Cash = 2,
        Bank = 3,
        Debitors = 4,
        Creditors = 5,

        /// <summary>
        ///     Материалы на складах
        /// </summary>
        Store = 6,

        /// <summary>
        ///     Неотфактурованные поставки
        /// </summary>
        NotFactPrihod = 7,

        /// <summary>
        ///     Деньги в подотчет
        /// </summary>
        UnderReport = 8,

        /// <summary>
        ///     Заработная плата
        /// </summary>
        Salary = 9,

        /// <summary>
        ///     Деньги в пути
        /// </summary>
        MoneyInPath = 10
    }

    [MetadataType(typeof(DataAnnotationsNomenklCompareBalansOperation))]
    public class NomenklCompareBalansOperation : SummaCompareCurrencies
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

    public class DataAnnotationsNomenklCompareBalansOperation : DataAnnotationForFluentApiBase,
        IMetadataProvider<NomenklCompareBalansOperation>
    {
        void IMetadataProvider<NomenklCompareBalansOperation>.BuildMetadata(
            MetadataBuilder<NomenklCompareBalansOperation> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата");
            builder.Property(_ => _.DocPrice).AutoGenerated().DisplayName("Цена");
            builder.Property(_ => _.FinDocument).AutoGenerated().DisplayName("Фин. док-т");
            builder.Property(_ => _.TovarDocument).AutoGenerated().DisplayName("Склад док-т");
            //builder.Property(_ => _.CalcPrice).AutoGenerated().DisplayName("Цена");
            //builder.Property(_ => _.CalcPriceNaklad).AutoGenerated().DisplayName("Цена (c накл)");
        }
    }
}