using KursDomain.ICommon;
using KursDomain.IReferences;
using System.Collections.Generic;
using System.Diagnostics;
using Data;

namespace KursDomain.References;

/// <summary>
/// Форма платежа
/// </summary>
[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class PayForm :  IPayForm, IDocCode, IName, IEqualityComparer<IDocCode>
{
    public decimal DocCode { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Форма платежа: {Name}";
    
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

    public void LoadFromEntity(SD_189 entity)
    {
        DocCode = entity.DOC_CODE;
        Name = entity.OOT_NAME;
    }
}
