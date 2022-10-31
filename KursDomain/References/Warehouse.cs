using System;
using KursDomain.ICommon;
using KursDomain.IReferences.Kontragent;
using System.Collections.Generic;
using KursDomain.IReferences;
using System.Diagnostics;
using Data;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq}/{Id} {Name,nq}")]
public class Warehouse : IWarehouse, IDocCode, IDocGuid, IName, IEqualityComparer<IDocCode>
{
    public IEmployee StoreKeeper { get; set; }
    public IRegion Region { get; set; }
    public bool IsNegativeRest { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsOutBalans { get; set; }
    public decimal DocCode { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Склад {Name} Кладовщик: {StoreKeeper}";

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

    public void LoadFromEntity(SD_27 entity, IReferencesCache refCache)
    {
        DocCode = entity.DOC_CODE;
        Id = entity.Id;
        Name = entity.SKL_NAME;
        Region = refCache.GetRegion(entity.SKL_REGION_DC);
        StoreKeeper = refCache.GetEmployee(entity.TABELNUMBER);

    }
}


