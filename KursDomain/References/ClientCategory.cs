using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq} {ParentDC,nq")]
public class ClientCategory : IClientCategory, IDocCode, IName, IEqualityComparer<IDocCode>
{
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Категория контрагента: {Name}";
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

    public void LoadFromEntity(SD_148 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.CK_NAME;
    }

    public override string ToString()
    {
        return Name;
    }
}
