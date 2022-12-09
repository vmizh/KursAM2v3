using System;
using System.Diagnostics;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.References;

[DebuggerDisplay("{Id} {NomenklNumber,nq} {Name,nq}")]
public class NomenklMain : IDocGuid, IName, INomenklMain, IEquatable<NomenklMain>, IComparable<NomenklMain>, IComparable
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    public decimal? DefaultNDSPercent { get; set; }

    public int CompareTo(NomenklMain other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Id.CompareTo(other.Id);
    }

    public Guid Id { get; set; }

    public bool Equals(NomenklMain other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public string Description { get; }
    public string NomenklNumber { get; set; }
    public IUnit Unit { get; set; }
    public INomenklGroup Category { get; set; }
    public string FullName { get; set; }
    public bool IsUsluga { get; set; }
    public bool IsProguct { get; set; }
    public bool IsNakladExpense { get; set; }
    public INomenklType NomenklType { get; set; }
    public IProductType ProductType { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsCurrencyTransfer { get; set; }
    public bool IsRentabelnost { get; set; }
    public bool IsOnlyState { get; set; }
    public ICountry Country { get; set; }


    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(Data.NomenklMain entity, IReferencesCache refCache)
    {
        if (entity == null)
        {
            Id = Guid.Empty;
            return;
        }

        Id = entity.Id;
        Name = entity.Name;
        Notes = entity.Note;
        NomenklNumber = entity.NomenklNumber;
        Unit = refCache?.GetUnit(entity.UnitDC);
        Category = refCache?.GetNomenklGroup(entity.CategoryDC);
        FullName = entity.FullName;
        IsUsluga = entity.IsUsluga;
        IsProguct = entity.IsComplex;
        IsNakladExpense = entity.IsNakladExpense;
        IsOnlyState = entity.IsOnlyState ?? false;
        IsCurrencyTransfer = entity.IsCurrencyTransfer ?? false;
        IsRentabelnost = entity.IsRentabelnost ?? false;
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
        return Equals((NomenklMain) obj);
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Id.GetHashCode();
    }
    
}
