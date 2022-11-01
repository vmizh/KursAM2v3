using System.ComponentModel.DataAnnotations;
using Core;
using Core.Helper;
using DevExpress.Mvvm.DataAnnotations;

namespace KursDomain.Documents.Currency;

[MetadataType(typeof(DataAnnotationsRemainderCurrenciesDatePeriod))]
public class RemainderCurrenciesDatePeriod : DatePeriod
{
    private decimal myCrsDC;

    public RemainderCurrenciesDatePeriod()
    {
        DefaultValue();
    }

    public RemainderCurrenciesDatePeriod(DatePeriod d) : this()
    {
        UpdateFromDatePeriod(d);
    }

    public decimal CrsDC
    {
        set
        {
            if (myCrsDC == value) return;
            myCrsDC = value;
            RaisePropertyChanged();
        }
        get => myCrsDC;
    }

    private void DefaultValue()
    {
        SummaStartGBP = 0;
        SummaEndGBP = 0;
        SummaInGBP = 0;
        SummaOutGBP = 0;
        SummaStartCHF = 0;
        SummaEndCHF = 0;
        SummaInCHF = 0;
        SummaOutCHF = 0;
        SummaStartRUB = 0;
        SummaEndRUB = 0;
        SummaInRUB = 0;
        SummaOutRUB = 0;
        SummaStartSEK = 0;
        SummaEndSEK = 0;
        SummaInSEK = 0;
        SummaOutSEK = 0;
        SummaStartUSD = 0;
        SummaEndUSD = 0;
        SummaInUSD = 0;
        SummaOutUSD = 0;
        SummaStartEUR = 0;
        SummaEndEUR = 0;
        SummaInEUR = 0;
        SummaOutEUR = 0;
    }

    private void UpdateFromDatePeriod(DatePeriod d)
    {
        Name = d.Name;
        Id = d.Id;
        ParentId = d.ParentId;
        PeriodType = d.PeriodType;
        DateStart = d.DateStart;
        DateEnd = d.DateEnd;
    }

    #region Currency

    #region CHF

    private decimal? mySummaOutCHF;

    public decimal? SummaOutCHF
    {
        set
        {
            if (mySummaOutCHF == value) return;
            mySummaOutCHF = value;
            RaisePropertyChanged();
        }
        get => mySummaOutCHF;
    }

    private decimal? mySummaInCHF;

    public decimal? SummaInCHF
    {
        set
        {
            if (mySummaInCHF == value) return;
            mySummaInCHF = value;
            RaisePropertyChanged();
        }
        get => mySummaInCHF;
    }

    private decimal? mySummaEndCHF;

    public decimal? SummaEndCHF
    {
        set
        {
            if (mySummaEndCHF == value) return;
            mySummaEndCHF = value;
            RaisePropertyChanged();
        }
        get => mySummaEndCHF;
    }

    private decimal? mySummaStartCHF;

    public decimal? SummaStartCHF
    {
        set
        {
            if (mySummaStartCHF == value) return;
            mySummaStartCHF = value;
            RaisePropertyChanged();
        }
        get => mySummaStartCHF;
    }

    #endregion

    #region EUR

    private decimal? mySummaOutEUR;

    public decimal? SummaOutEUR
    {
        set
        {
            if (mySummaOutEUR == value) return;
            mySummaOutEUR = value;
            RaisePropertyChanged();
        }
        get => mySummaOutEUR;
    }

    private decimal? mySummaInEUR;

    public decimal? SummaInEUR
    {
        set
        {
            if (mySummaInEUR == value) return;
            mySummaInEUR = value;
            RaisePropertyChanged();
        }
        get => mySummaInEUR;
    }

    private decimal? mySummaEndEUR;

    public decimal? SummaEndEUR
    {
        set
        {
            if (mySummaEndEUR == value) return;
            mySummaEndEUR = value;
            RaisePropertyChanged();
        }
        get => mySummaEndEUR;
    }

    private decimal? mySummaStartEUR;

    public decimal? SummaStartEUR
    {
        set
        {
            if (mySummaStartEUR == value) return;
            mySummaStartEUR = value;
            RaisePropertyChanged();
        }
        get => mySummaStartEUR;
    }

    #endregion

    #region GBP

    private decimal? mySummaOutGBP;

    public decimal? SummaOutGBP
    {
        set
        {
            if (mySummaOutGBP == value) return;
            mySummaOutGBP = value;
            RaisePropertyChanged();
        }
        get => mySummaOutGBP;
    }

    private decimal? mySummaInGBP;

    public decimal? SummaInGBP
    {
        set
        {
            if (mySummaInGBP == value) return;
            mySummaInGBP = value;
            RaisePropertyChanged();
        }
        get => mySummaInGBP;
    }

    private decimal? mySummaEndGBP;

    public decimal? SummaEndGBP
    {
        set
        {
            if (mySummaEndGBP == value) return;
            mySummaEndGBP = value;
            RaisePropertyChanged();
        }
        get => mySummaEndGBP;
    }

    private decimal? mySummaStartGBP;

    public decimal? SummaStartGBP
    {
        set
        {
            if (mySummaStartGBP == value) return;
            mySummaStartGBP = value;
            RaisePropertyChanged();
        }
        get => mySummaStartGBP;
    }

    #endregion

    #region RUB

    private decimal? mySummaOutRUB;

    public decimal? SummaOutRUB
    {
        set
        {
            if (mySummaOutRUB == value) return;
            mySummaOutRUB = value;
            RaisePropertyChanged();
        }
        get => mySummaOutRUB;
    }

    private decimal? mySummaInRUB;

    public decimal? SummaInRUB
    {
        set
        {
            if (mySummaInRUB == value) return;
            mySummaInRUB = value;
            RaisePropertyChanged();
        }
        get => mySummaInRUB;
    }

    private decimal? mySummaEndRUB;

    public decimal? SummaEndRUB
    {
        set
        {
            if (mySummaEndRUB == value) return;
            mySummaEndRUB = value;
            RaisePropertyChanged();
        }
        get => mySummaEndRUB;
    }

    private decimal? mySummaStartRUB;

    public decimal? SummaStartRUB
    {
        set
        {
            if (mySummaStartRUB == value) return;
            mySummaStartRUB = value;
            RaisePropertyChanged();
        }
        get => mySummaStartRUB;
    }

    #endregion

    #region USD

    private decimal? mySummaOutUSD;

    public decimal? SummaOutUSD
    {
        set
        {
            if (mySummaOutUSD == value) return;
            mySummaOutUSD = value;
            RaisePropertyChanged();
        }
        get => mySummaOutUSD;
    }

    private decimal? mySummaInUSD;

    public decimal? SummaInUSD
    {
        set
        {
            if (mySummaInUSD == value) return;
            mySummaInUSD = value;
            RaisePropertyChanged();
        }
        get => mySummaInUSD;
    }

    private decimal? mySummaEndUSD;

    public decimal? SummaEndUSD
    {
        set
        {
            if (mySummaEndUSD == value) return;
            mySummaEndUSD = value;
            RaisePropertyChanged();
        }
        get => mySummaEndUSD;
    }

    private decimal? mySummaStartUSD;

    public decimal? SummaStartUSD
    {
        set
        {
            if (mySummaStartUSD == value) return;
            mySummaStartUSD = value;
            RaisePropertyChanged();
        }
        get => mySummaStartUSD;
    }

    #endregion

    #region SEK

    private decimal? mySummaOutSEK;

    public decimal? SummaOutSEK
    {
        set
        {
            if (mySummaOutSEK == value) return;
            mySummaOutSEK = value;
            RaisePropertyChanged();
        }
        get => mySummaOutSEK;
    }

    private decimal? mySummaInSEK;

    public decimal? SummaInSEK
    {
        set
        {
            if (mySummaInSEK == value) return;
            mySummaInSEK = value;
            RaisePropertyChanged();
        }
        get => mySummaInSEK;
    }

    private decimal? mySummaEndSEK;

    public decimal? SummaEndSEK
    {
        set
        {
            if (mySummaEndSEK == value) return;
            mySummaEndSEK = value;
            RaisePropertyChanged();
        }
        get => mySummaEndSEK;
    }

    private decimal? mySummaStartSEK;

    public decimal? SummaStartSEK
    {
        set
        {
            if (mySummaStartSEK == value) return;
            mySummaStartSEK = value;
            RaisePropertyChanged();
        }
        get => mySummaStartSEK;
    }

    #endregion

    #endregion
}

public class DataAnnotationsRemainderCurrenciesDatePeriod : DataAnnotationForFluentApiBase,
    IMetadataProvider<RemainderCurrenciesDatePeriod>
{
    void IMetadataProvider<RemainderCurrenciesDatePeriod>.BuildMetadata(
        MetadataBuilder<RemainderCurrenciesDatePeriod> builder)
    {
        builder.Property(_ => _.PeriodType).NotAutoGenerated();
        builder.Property(_ => _.CrsDC).NotAutoGenerated();
        builder.Property(_ => _.SummaEndCHF).DisplayName("На конец");
        builder.Property(_ => _.SummaStartCHF).DisplayName("На начало");
        builder.Property(_ => _.SummaInCHF).DisplayName("Приход");
        builder.Property(_ => _.SummaOutCHF).DisplayName("Расход");
        builder.Property(_ => _.SummaEndEUR).DisplayName("На конец");
        builder.Property(_ => _.SummaStartEUR).DisplayName("На начало");
        builder.Property(_ => _.SummaInEUR).DisplayName("Приход");
        builder.Property(_ => _.SummaOutEUR).DisplayName("Расход");
        builder.Property(_ => _.SummaEndGBP).DisplayName("На конец");
        builder.Property(_ => _.SummaStartGBP).DisplayName("На начало");
        builder.Property(_ => _.SummaInGBP).DisplayName("Приход");
        builder.Property(_ => _.SummaOutGBP).DisplayName("Расход");
        builder.Property(_ => _.SummaEndRUB).DisplayName("На конец");
        builder.Property(_ => _.SummaStartRUB).DisplayName("На начало");
        builder.Property(_ => _.SummaInRUB).DisplayName("Приход");
        builder.Property(_ => _.SummaOutRUB).DisplayName("Расход");
        builder.Property(_ => _.SummaEndUSD).DisplayName("На конец");
        builder.Property(_ => _.SummaStartUSD).DisplayName("На начало");
        builder.Property(_ => _.SummaInUSD).DisplayName("Приход");
        builder.Property(_ => _.SummaOutUSD).DisplayName("Расход");
        builder.Property(_ => _.SummaEndSEK).DisplayName("На конец");
        builder.Property(_ => _.SummaStartSEK).DisplayName("На начало");
        builder.Property(_ => _.SummaInSEK).DisplayName("Приход");
        builder.Property(_ => _.SummaOutSEK).DisplayName("Расход");
        builder.Group("Период")
            .ContainsProperty(_ => _.Name)
            .ContainsProperty(_ => _.DateStart)
            .ContainsProperty(_ => _.DateEnd);
        builder.Group("RUB")
            .ContainsProperty(_ => _.SummaEndRUB)
            .ContainsProperty(_ => _.SummaStartRUB)
            .ContainsProperty(_ => _.SummaInRUB)
            .ContainsProperty(_ => _.SummaOutRUB);
        builder.Group("SEK")
            .ContainsProperty(_ => _.SummaEndSEK)
            .ContainsProperty(_ => _.SummaStartSEK)
            .ContainsProperty(_ => _.SummaInSEK)
            .ContainsProperty(_ => _.SummaOutSEK);
        builder.Group("CHF")
            .ContainsProperty(_ => _.SummaEndCHF)
            .ContainsProperty(_ => _.SummaStartCHF)
            .ContainsProperty(_ => _.SummaInCHF)
            .ContainsProperty(_ => _.SummaOutCHF);
        builder.Group("USD")
            .ContainsProperty(_ => _.SummaEndUSD)
            .ContainsProperty(_ => _.SummaStartUSD)
            .ContainsProperty(_ => _.SummaInUSD)
            .ContainsProperty(_ => _.SummaOutUSD);
        builder.Group("EUR")
            .ContainsProperty(_ => _.SummaEndEUR)
            .ContainsProperty(_ => _.SummaStartEUR)
            .ContainsProperty(_ => _.SummaInEUR)
            .ContainsProperty(_ => _.SummaOutEUR);
        builder.Group("GBP")
            .ContainsProperty(_ => _.SummaEndGBP)
            .ContainsProperty(_ => _.SummaStartGBP)
            .ContainsProperty(_ => _.SummaInGBP)
            .ContainsProperty(_ => _.SummaOutGBP);
    }
}

public class DataAnnotationsRemainderDatePeriod : DataAnnotationForFluentApiBase,
    IMetadataProvider<ReminderDatePeriod>
{
    public void BuildMetadata(MetadataBuilder<ReminderDatePeriod> builder)
    {
        builder.Property(_ => _.PeriodType).NotAutoGenerated();
        builder.Property(_ => _.Currency).NotAutoGenerated().DisplayName("Валюта");
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Период");
        builder.Property(_ => _.SummaStart).AutoGenerated().DisplayName("Начало");
        builder.Property(_ => _.SummaIn).AutoGenerated().DisplayName("Приход");
        builder.Property(_ => _.SummaOut).AutoGenerated().DisplayName("Расход");
        builder.Property(_ => _.SummaEnd).AutoGenerated().DisplayName("Конец");
    }
}

[MetadataType(typeof(DataAnnotationsRemainderDatePeriod))]
public class ReminderDatePeriod : DatePeriod
{
    private References.Currency myCurrency;
    private decimal? mySummaEnd;
    private decimal? mySummaIn;
    private decimal? mySummaOut;
    private decimal? mySummaStart;

    public ReminderDatePeriod(DatePeriod p) : this()
    {
        UpdateFromDatePeriod(p);
    }

    public ReminderDatePeriod()
    {
        DefaultValue();
    }

    public References.Currency Currency
    {
        get => myCurrency;
        set
        {
            if (myCurrency == value) return;
            myCurrency = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SummaOut
    {
        set
        {
            if (mySummaOut == value) return;
            mySummaOut = value;
            RaisePropertyChanged();
        }
        get => mySummaOut;
    }

    public decimal? SummaIn
    {
        set
        {
            if (mySummaIn == value) return;
            mySummaIn = value;
            RaisePropertyChanged();
        }
        get => mySummaIn;
    }

    public decimal? SummaEnd
    {
        set
        {
            if (mySummaEnd == value) return;
            mySummaEnd = value;
            RaisePropertyChanged();
        }
        get => mySummaEnd;
    }

    public decimal? SummaStart
    {
        set
        {
            if (mySummaStart == value) return;
            mySummaStart = value;
            RaisePropertyChanged();
        }
        get => mySummaStart;
    }

    private void DefaultValue()
    {
        SummaStart = 0;
        SummaIn = 0;
        SummaOut = 0;
        SummaEnd = 0;
    }

    private void UpdateFromDatePeriod(DatePeriod d)
    {
        Name = d.Name;
        Id = d.Id;
        ParentId = d.ParentId;
        PeriodType = d.PeriodType;
        DateStart = d.DateStart;
        DateEnd = d.DateEnd;
    }
}
