using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.Documents.Cash;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

/// <summary>
///     Касса
/// </summary>
[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class CashBox : ICashBox, IDocCode, IName, IEquatable<CashBox>, ILoadFromEntity<SD_22>, IComparable, ICache
{
    public CashBox()
    {
        LoadFromCache();
    }

    public decimal? DefaultCurrencyDC { set; get; }
    public decimal? KontragentDC { get; set; }
    public decimal? CentrResponsibilityDC { get; set; }

    public void LoadFromCache()
    {
        if (GlobalOptions.ReferencesCache is not RedisCacheReferences cache) return;
        if (CentrResponsibilityDC is not null)
            CentrResponsibility = cache.GetItem<CentrResponsibility>(CentrResponsibilityDC.Value);
        if (DefaultCurrencyDC is not null)
            DefaultCurrency = cache.GetItem<Currency>(DefaultCurrencyDC.Value);
        if (KontragentDC is not null)
            Kontragent = cache.GetItem<Kontragent>(KontragentDC.Value);
    }

    [JsonIgnore] public ICurrency DefaultCurrency { get; set; }

    public bool IsNegativeRests { get; set; }

    [JsonIgnore] public IKontragent Kontragent { get; set; }

    [JsonIgnore] public ICentrResponsibility CentrResponsibility { get; set; }

    public bool IsNoBalans { get; set; }

    [JsonIgnore] public IEnumerable<ICashBoxStartRests> StartRests { get; set; }

    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : string.Compare(Name, c.Name, StringComparison.Ordinal);
    }

    public decimal DocCode { get; set; }

    public bool Equals(CashBox other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
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

    public string Name { get; set; }
    public string Notes { get; set; }

    [JsonIgnore] public string Description => $"Касса: {Name}";

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((CashBox)obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
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
    private ICashBox CashBox { get; }
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

[MetadataType(typeof(DataAnnotationsCash))]
public class CashViewModel : RSViewModelBase, IEntity<SD_22>
{
    private CentrResponsibility myCO;
    private SD_22 myEntity;
    private bool myIsCanNegative;

    public CashViewModel()
    {
        Entity = DefaultValue();
    }

    public CashViewModel(SD_22 entity)
    {
        Entity = entity ?? DefaultValue();
    }

    public override decimal DocCode
    {
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public string CA_NAME
    {
        get => Entity.CA_NAME;
        set
        {
            if (Entity.CA_NAME == value) return;
            Entity.CA_NAME = value;
            RaisePropertyChanged();
        }
    }

    public override string Name
    {
        get => Entity.CA_NAME;
        set
        {
            if (Entity.CA_NAME == value) return;
            Entity.CA_NAME = value;
            RaisePropertyChanged();
        }
    }

    public decimal CA_CRS_DC
    {
        get => Entity.CA_CRS_DC;
        set
        {
            if (Entity.CA_CRS_DC == value) return;
            Entity.CA_CRS_DC = value;
            RaisePropertyChanged();
        }
    }

    public short? CA_NEGATIVE_RESTS
    {
        get => Entity.CA_NEGATIVE_RESTS;
        set
        {
            if (Entity.CA_NEGATIVE_RESTS == value) return;
            Entity.CA_NEGATIVE_RESTS = value;
            RaisePropertyChanged();
        }
    }

    public bool IsCanNegative
    {
        get => myIsCanNegative;
        set
        {
            if (myIsCanNegative == value) return;
            myIsCanNegative = value;
            CA_NEGATIVE_RESTS = (short?)(myIsCanNegative ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    public decimal? CA_KONTRAGENT_DC
    {
        get => Entity.CA_KONTRAGENT_DC;
        set
        {
            if (Entity.CA_KONTRAGENT_DC == value) return;
            Entity.CA_KONTRAGENT_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CA_CENTR_OTV_DC
    {
        get => Entity.CA_CENTR_OTV_DC;
        set
        {
            if (Entity.CA_CENTR_OTV_DC == value) return;
            Entity.CA_CENTR_OTV_DC = value;
            RaisePropertyChanged();
        }
    }

    public CentrResponsibility CO
    {
        get => myCO;
        set
        {
            if (myCO != null && myCO.Equals(value)) return;
            myCO = value;
            if (myCO != null)
                CA_CENTR_OTV_DC = myCO.DocCode;
            RaisePropertyChanged();
        }
    }

    public decimal? CA_KONTR_DC
    {
        get => Entity.CA_KONTR_DC;
        set
        {
            if (Entity.CA_KONTR_DC == value) return;
            Entity.CA_KONTR_DC = value;
            RaisePropertyChanged();
        }
    }

    public short? CA_NO_BALANS
    {
        get => Entity.CA_NO_BALANS;
        set
        {
            if (Entity.CA_NO_BALANS == value) return;
            Entity.CA_NO_BALANS = value;
            RaisePropertyChanged();
        }
    }

    public SD_40 SD_40
    {
        get => Entity.SD_40;
        set
        {
            if (Entity.SD_40 == value) return;
            Entity.SD_40 = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public SD_22 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_22 DefaultValue()
    {
        return new SD_22
        {
            DOC_CODE = -1
        };
    }

    public List<SD_22> LoadList()
    {
        throw new NotImplementedException();
    }

    public SD_22 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public virtual void Save(SD_22 doc)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public void Delete()
    {
        throw new NotImplementedException();
    }

    public void Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Delete(decimal dc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(SD_22 ent)
    {
        CA_NAME = ent.CA_NAME;
        CA_CRS_DC = ent.CA_CRS_DC;
        CA_NEGATIVE_RESTS = ent.CA_NEGATIVE_RESTS;
        CA_KONTRAGENT_DC = ent.CA_KONTRAGENT_DC;
        CA_CENTR_OTV_DC = ent.CA_CENTR_OTV_DC;
        CA_KONTR_DC = ent.CA_KONTR_DC;
        CA_NO_BALANS = ent.CA_NO_BALANS;
        SD_40 = ent.SD_40;
    }

    public void UpdateTo(SD_22 ent)
    {
        ent.CA_NAME = CA_NAME;
        ent.CA_CRS_DC = CA_CRS_DC;
        ent.CA_NEGATIVE_RESTS = CA_NEGATIVE_RESTS;
        ent.CA_KONTRAGENT_DC = CA_KONTRAGENT_DC;
        ent.CA_CENTR_OTV_DC = CA_CENTR_OTV_DC;
        ent.CA_KONTR_DC = CA_KONTR_DC;
        ent.CA_NO_BALANS = CA_NO_BALANS;
        ent.SD_40 = SD_40;
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public virtual SD_22 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public virtual SD_22 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public SD_22 Load(decimal dc)
    {
        throw new NotImplementedException();
    }
}

[MetadataType(typeof(DataAnnotationsCashReference))]
public sealed class CashReference : CashViewModel
{
    private Currency myDefaultCurrency;

    public CashReference()
    {
        DefaultCurrency = GlobalOptions.ReferencesCache.GetCurrency(CA_CRS_DC) as Currency;
    }

    public CashReference(SD_22 entity) : base(entity)
    {
        DefaultCurrency = GlobalOptions.ReferencesCache.GetCurrency(CA_CRS_DC) as Currency;
        CO = GlobalOptions.ReferencesCache.GetCentrResponsibility(CA_CENTR_OTV_DC) as CentrResponsibility;
        if (Entity.TD_22 != null && Entity.TD_22.Count > 0)
            foreach (var r in Entity.TD_22)
            {
                var newItem = new CashStartRemains(r)
                {
                    Cash = this,
                    Parent = this,
                    ParentDC = DocCode,
                    State = RowStatus.NotEdited
                };
                StartRemains.Add(newItem);
            }
    }

    public Currency DefaultCurrency
    {
        get => myDefaultCurrency;
        set
        {
            if (Equals(myDefaultCurrency, value)) return;
            myDefaultCurrency = value;
            if (myDefaultCurrency != null)
                CA_CRS_DC = myDefaultCurrency.DocCode;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<CashStartRemains> StartRemains { set; get; } =
        new ObservableCollection<CashStartRemains>();
}

public class DataAnnotationsCash : DataAnnotationForFluentApiBase, IMetadataProvider<CashViewModel>
{
    void IMetadataProvider<CashViewModel>.BuildMetadata(MetadataBuilder<CashViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
    }
}

public class DataAnnotationsCashReference : DataAnnotationForFluentApiBase, IMetadataProvider<CashReference>
{
    void IMetadataProvider<CashReference>.BuildMetadata(MetadataBuilder<CashReference> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.StartRemains).NotAutoGenerated();
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
        builder.Property(_ => _.DefaultCurrency).AutoGenerated().DisplayName("Валюта по умолчанию");
        builder.Property(_ => _.IsCanNegative).AutoGenerated().DisplayName("Отр.остатки");
        builder.Property(_ => _.CO).AutoGenerated().DisplayName("Центр ответственности");
    }
}
