using KursDomain.ICommon;
using KursDomain.IReferences;
using System.Collections.Generic;
using Data;
using System.Diagnostics;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class PayCondition : IPayCondition, IDocCode, IName, IEqualityComparer<IDocCode>
{
    public bool IsDefault { get; set; }
    public decimal DocCode { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Условие оплаты: {Name}";
    public override string ToString()
    {
        return Name;
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

    public void LoadFromEntity(SD_179 entity)
    {
        DocCode = entity.DOC_CODE;
        Name = entity.PT_NAME;
        IsDefault = entity.DEFAULT_VALUE == 1;
    }
}
