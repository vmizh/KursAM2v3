using System;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq}/{Id} {Name,nq}")]
public class Warehouse : IWarehouse, IDocCode, IDocGuid, IName, IEquatable<Warehouse>
{
    public decimal DocCode { get; set; }
    public Guid Id { get; set; }

    public bool Equals(Warehouse other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Склад {Name} Кладовщик: {StoreKeeper}";
    public IEmployee StoreKeeper { get; set; }
    public IRegion Region { get; set; }
    public bool IsNegativeRest { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsOutBalans { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_27 entity, IReferencesCache refCache)
    {
        DocCode = entity.DOC_CODE;
        Id = entity.Id;
        Name = entity.SKL_NAME;
        Region = refCache.GetRegion(entity.SKL_REGION_DC);
        StoreKeeper = refCache.GetEmployee(entity.TABELNUMBER);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Warehouse) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }
}
