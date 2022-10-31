using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences.Kontragent;

namespace KursDomain.References;
[DebuggerDisplay("{Id,nq} {Name,nq} {ParentId,nq}")]
public class KontragentCategory : IName, IKontragentCategory, IEqualityComparer<KontragentCategory>
{
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description { get; set; }
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public int? ParentId { get; set; }
    public bool Equals(KontragentCategory cat1, KontragentCategory cat2)
    {
        if (cat2 == null && cat1 == null)
            return true;
        if (cat1 == null || cat2 == null)
            return false;
        return cat1.Id == cat2.Id;
    }

    public int GetHashCode(KontragentCategory obj)
    {
        return (Id + Name).GetHashCode();
    }

    public void LoadFromEntity(UD_43 entity)
    {
        Id = entity.EG_ID;
        Name = entity.EG_NAME;
        IsDeleted = entity.EG_DELETED == 1;
        ParentId = entity.EG_PARENT_ID;
    }

    public override string ToString()
    {
        return Name;
    }
}
