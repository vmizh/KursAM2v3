using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq} {ParentDC,nq}")]
public class ProductType : IProductType, IDocCode, IName, IEqualityComparer<IDocCode>
{
    public decimal? ParentDC { get; set; }
    public string FullName { get; set; }
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

    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Тип продукции: {Name}";

    public void LoadFromEntity(SD_50 entity)
    {
        DocCode = entity.DOC_CODE;
        Name = entity.PROD_NAME;
        FullName = entity.PROD_FULL_NAME;
        ParentDC = entity.PROD_PARENT_DC;
    }

    public override string ToString()
    {
        return Name;
    }
}
