using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

/// <summary>
/// Тип взаимозачета
/// </summary>
[DebuggerDisplay("{DocCode,nq} {Name,nq} Валют.конверт.:{IsCurrencyConvert,nq}")]
public class MutualSettlementType : IMutualSettlementType,IDocCode,IName, IEqualityComparer<IDocCode>
{
    public bool IsCurrencyConvert { get; set; }
    public decimal DocCode { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Тип взаимозачета: {Name}";

    public void LoadFromEntity(SD_111 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        IsCurrencyConvert = entity.IsCurrencyConvert;
        Name = entity.ZACH_NAME;
    }

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

    public override string ToString()
    {
        return Name;
    }
}
