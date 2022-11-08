using System;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class SettlementType : ISettlementType, IDocCode, IName, IEquatable<SettlementType>
{
    public decimal DocCode { get; set; }

    public bool Equals(SettlementType other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Тип взаимозачета: {Name}";
    public int? MSType { get; set; }
    public ISDRSchet SDRSchet { get; set; }

    public void LoadFromEntity(SD_77 entity, IReferencesCache refCache)
    {
        DocCode = entity.DOC_CODE;
        refCache.GetSDRSchet(entity.TV_SHPZ_DC);
        Name = entity.TV_NAME;
        MSType = entity.TV_TYPE;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SettlementType) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }
}
