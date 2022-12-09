using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;
using System;
using System.Diagnostics;

namespace KursDomain.References;

/// <summary>
/// SD_77 тип продукции для счетов
/// </summary>
[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class NomenklProductType : IName, IDocCode, INomenklProductType, IEquatable<NomenklProductType>, 
    ILoadFromEntity<SD_77>,IComparable
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Тип продукции: {Name}";
    public decimal DocCode { get; set; }
    public ISDRSchet SDRSchet { get; set; }
    
    public void LoadFromEntity(SD_77 entity, IReferencesCache cache)
    {
        DocCode = entity.DOC_CODE;
        Name = entity.TV_NAME;
        SDRSchet = cache.GetSDRSchet(entity.TV_SHPZ_DC);
    }

    public bool Equals(NomenklProductType other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((NomenklProductType) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
}
