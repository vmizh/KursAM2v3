using System;
using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq}/{Id} {Name,nq} {ParentDC,nq}")]
public class Region : IRegion, IDocCode, IDocGuid, IName, IEqualityComparer<IDocCode>
{
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description =>$"Регион: {Name}";
    public decimal? ParentDC { get; set; }
    public decimal DocCode { get; set; }
    public Guid Id { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_23 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        ParentDC = entity.REG_PARENT_DC;
        Name = entity.REG_NAME;
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
