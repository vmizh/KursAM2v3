using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq} {ParentDC,nq}")]
public class CentrResponsibility : ICentrResponsibility, IDocCode, IName, IEqualityComparer<IDocCode>
{
    public string FullName { get; set; }
    public decimal? ParentDC { get; set; }
    public bool IsDeleted { get; set; }
    public decimal DocCode { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"ЦО: {Name}";

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

    public void LoadFromEntity(SD_40 entity)
    {
        DocCode = entity.DOC_CODE;
        Name = entity.CENT_NAME;
        FullName = entity.CENT_FULLNAME;
        ParentDC = entity.CENT_PARENT_DC;
        IsDeleted = entity.IS_DELETED == 1;
    }
}
