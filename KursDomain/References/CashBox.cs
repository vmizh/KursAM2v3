using System;
using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;

namespace KursDomain.References;

/// <summary>
///     Касса
/// </summary>
[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class CashBox : ICashBox, IDocCode, IName, IEqualityComparer<IDocCode>
{
    public ICurrency DefaultCurrency { get; set; }
    public bool IsNegativeRests { get; set; }
    public IKontragent Kontragent { get; set; }
    public ICentrResponsibility CentrResponsibility { get; set; }
    public bool IsNoBalans { get; set; }
    public IEnumerable<ICashBoxStartRests> StartRests { get; set; }
    public decimal DocCode { get; set; }

    public bool Equals(IDocCode x, IDocCode y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DocCode == y.DocCode;
    }

    public int GetHashCode(IDocCode obj)
    {
        return obj.DocCode.GetHashCode();
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Касса: {Name}";

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_22 entity, IReferencesCache referencesCache)
    {
        DefaultCurrency = referencesCache?.GetCurrency(entity.CA_CRS_DC);
        IsNegativeRests = entity.CA_NEGATIVE_RESTS == 1;
        Kontragent = referencesCache?.GetKontragent(entity.CA_KONTR_DC);
        CentrResponsibility = referencesCache?.GetCentrResponsibility(entity.CA_CENTR_OTV_DC);
        IsNoBalans = entity.CA_NEGATIVE_RESTS == 1;
        DocCode = entity.DOC_CODE;
        Name = entity.CA_NAME;
        if (entity.TD_22 != null)
        {
            //var newRest = new Cash
            //var rests = 
        }
    }
}

[DebuggerDisplay("{DocCode,nq} {Currency,nq} {SummaStart,nq} {DateStart.ToShortDateString(),nq}")]
public class CashBoxStartRests : ICashBoxStartRests, IName, IEqualityComparer<CashBoxStartRests>
{
    public CashBoxStartRests(ICashBox cashBox)
    {
        CashBox = cashBox;
    }
    public CashBoxStartRests()
    {
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private ICashBox CashBox { set; get; }
    public decimal DocCode { get; set; }
    public decimal Code { get; set; }
    public ICurrency Currency { get; set; }
    public DateTime DateStart { get; set; }
    public decimal SummaStart { get; set; }
    public decimal CashDateDC { get; set; }

    public bool Equals(CashBoxStartRests x, CashBoxStartRests y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DocCode == y.DocCode && x.Code == y.Code;
    }

    public int GetHashCode(CashBoxStartRests obj)
    {
        unchecked
        {
            return (obj.DocCode.GetHashCode() * 397) ^ obj.Code.GetHashCode();
        }
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Нач. остаток на {DateStart.ToShortDateString()}: {SummaStart} {Currency}";

    public override string ToString()
    {
        return $"{SummaStart} {Currency}";
    }
    
    public void LoadFromEntity(TD_22 entity)
    {
        DocCode = entity.DOC_CODE;
        Code = entity.CODE;
        DateStart = entity.DATE_START;
        SummaStart = entity.SUMMA_START;
        CashDateDC = entity.CASH_DATE_DC ?? 0;
    }
}
