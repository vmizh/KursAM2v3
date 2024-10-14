using System;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

/// <summary>
///     SD_77 тип продукции для счетов
/// </summary>
[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class NomenklProductType : IName, IDocCode, INomenklProductType, IEquatable<NomenklProductType>,
    ILoadFromEntity<SD_77>, IComparable, ICache
{
    public decimal? SDRSchetDC { get; set; }

    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : string.Compare(Name, c.Name, StringComparison.Ordinal);
    }

    public decimal DocCode { get; set; }

    public bool Equals(NomenklProductType other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public void LoadFromEntity(SD_77 entity, IReferencesCache cache)
    {
        DocCode = entity.DOC_CODE;
        Name = entity.TV_NAME;
        SDRSchet = cache.GetSDRSchet(entity.TV_SHPZ_DC);
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    [JsonIgnore]
    public string Description => $"Тип продукции: {Name}";

    [JsonIgnore] public ISDRSchet SDRSchet { get; set; }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((NomenklProductType)obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }

    public void LoadFromCache()
    {
        if (GlobalOptions.ReferencesCache is not RedisCacheReferences cache) return;
        if (SDRSchetDC is not null)
            SDRSchet = cache.GetItem<SDRSchet>(SDRSchetDC.Value);
    }

    public DateTime LastUpdateServe { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
