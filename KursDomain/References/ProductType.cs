using System;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences.Nomenkl;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq} {ParentDC,nq}")]
public class ProductType : IProductType, IDocCode, IName, IEquatable<ProductType>, IComparable, ICache
{
    public int CompareTo(object obj)
    {
        var c = obj as ProductType;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    public decimal DocCode { get; set; }

    public bool Equals(ProductType other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    [JsonIgnore]
    public string Description => $"Тип продукции: {Name}";
    public decimal? ParentDC { get; set; }
    public string FullName { get; set; }

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

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ProductType) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }

    public void LoadFromCache()
    {
    }

    public DateTime LastUpdateServe { get; set; }
}
