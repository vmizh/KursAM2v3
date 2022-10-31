using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class SettlementType : ISettlementType, IDocCode, IName, IEqualityComparer<IDocCode>
{
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

    public int? MSType { get; set; }
    public ISDRSchet SDRSchet { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Тип взаимозачета: {Name}";

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
}
