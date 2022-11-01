using KursDomain.ICommon;
using KursDomain.IReferences.Nomenkl;
using System.Collections.Generic;
using System.Diagnostics;
using Data;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class NomenklType : IDocCode, IName, INomenklType, IEqualityComparer<IDocCode>
{
    public decimal DocCode { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Тип продукции: {Name}";
    public bool IsDeleted { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_119 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.MC_NAME;
        IsDeleted = entity.MC_DELETED == 1;
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
}
