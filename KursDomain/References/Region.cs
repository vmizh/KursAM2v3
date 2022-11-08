using System;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq}/{Id} {Name,nq} {ParentDC,nq}")]
public class Region : IRegion, IDocCode, IDocGuid, IName, IEquatable<Region>
{
    public decimal DocCode { get; set; }
    public Guid Id { get; set; }

    public bool Equals(Region other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Регион: {Name}";
    public decimal? ParentDC { get; set; }

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

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Region) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }
}
