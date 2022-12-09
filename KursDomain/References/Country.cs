using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq}/{Id} {Name,nq} {ParentDC,nq}")]
public class Country : ICountry, IDocGuid, IName, IEquatable<Country>, IComparable
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    [MaxLength(2)] public string Alpha2 { get; set; }

    [MaxLength(3)] public string Alpha3 { get; set; }

    public string ISO { get; set; }

    [MaxLength(150)] public string ForeignName { get; set; }

    [MaxLength(150)] public string RussianName { get; set; }

    public string Location { get; set; }
    public string LocationPrecise { get; set; }
    public byte[] Flag { get; set; }
    public Guid Id { get; set; }

    public bool Equals(Country other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    [MaxLength(150)] public string Name { get; set; }

    public string Notes { get; set; }
    public string Description => $"Страна: {Name}";

    public override string ToString()
    {
        return Name;
    }


    public void LoadFromEntity(COUNTRY entity)
    {
        Id = Guid.Parse(entity.ID);
        Name = entity.NAME;
        Notes = entity.NOTES;
        RussianName = entity.RUSSIAN_NAME;
        ForeignName = entity.FOREIGN_NAME;
        ISO = entity.ISO;
        Alpha2 = entity.ALPHA2;
        Alpha3 = entity.ALPHA3;
        Location = entity.LOCATION;
        LocationPrecise = entity.LOCATION_PRECISE;
    }

    public void LoadFromEntity(Countries entity)
    {
        Id = entity.Id;
        Name = entity.Name;
        RussianName = entity.Name;
        ForeignName = entity.ForeignName;
        ISO = entity.Iso.ToString();
        Alpha2 = entity.Sign2;
        Alpha3 = entity.Sign3;
    }

    public bool Equals(IDocGuid x, IDocGuid y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Id.Equals(y.Id);
    }

    public int GetHashCode(IDocGuid obj)
    {
        return obj.Id.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Country) obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
