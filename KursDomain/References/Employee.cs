using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq}/{Id} {Description,nq}")]
public class Employee : IEmployee, IDocCode, IDocGuid, IName, IEqualityComparer<Employee>
{
    public decimal DocCode { get; set; }
    public Guid Id { get; set; }
    public int TabelNumber { get; set; }
    public string NameFirst { get; set; }
    public string NameSecond { get; set; }
    public string NameLast { get; set; }
    public ICurrency Currency { get; set; }
    public string Position { get; set; }
    public bool IsDeleted { get; set; }

    public bool Equals(Employee x, Employee y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Id.Equals(y.Id);
    }

    public int GetHashCode(Employee obj)
    {
        return obj.Id.GetHashCode();
    }

    public string Name
    {
        get => $"{NameLast} {NameFirst} {NameSecond}";

        set { }
    }

    public string Notes { get; set; }
    public string Description => $"{Name} Таб.№:{TabelNumber} {Currency}";

    public override string ToString()
    {
        var fName = !string.IsNullOrWhiteSpace(NameFirst) ? NameFirst.ToUpper().First() + "." : null;
        var sName = !string.IsNullOrWhiteSpace(NameSecond) ? NameSecond.ToUpper().First() + "." : null;
        return $"{NameLast} {fName}{sName}";
    }

    public void LoadFromEntity(SD_2 entity, IReferencesCache refCache)
    {
        DocCode = entity.DOC_CODE;
        Currency = refCache.GetCurrency(entity.crs_dc);
        Guid.TryParse(entity.ID, out var id);
        Id = id;
        NameFirst = entity.NAME_FIRST;
        NameLast = entity.NAME_LAST;
        NameSecond = entity.NAME_SECOND;
        TabelNumber = entity.TABELNUMBER;
        Position = entity.STATUS_NOTES;
    }
}
