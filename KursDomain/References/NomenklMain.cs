using System;
using KursDomain.ICommon;
using KursDomain.IReferences.Nomenkl;
using System.Collections.Generic;
using System.Diagnostics;
using KursDomain.IReferences;
using IProductType = KursDomain.IReferences.Nomenkl.IProductType;

namespace KursDomain.References;

[DebuggerDisplay("{Id} {NomenklNumber,nq} {Name,nq} {Currency,nq}")]
public class NomenklMain : IDocGuid, IName,  INomenklMain, IEqualityComparer<IDocGuid>
{
  

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
        Category = refCache?.GetNomenklCategory(entity.CategoryDC);
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

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description { get; }
    public string NomenklNumber { get; set; }
    public IUnit Unit { get; set; }
    public INomenklCategory Category { get; set; }
    public string FullName { get; set; }
    public bool IsUsluga { get; set; }
    public bool IsProguct { get; set; }
    public bool IsNakladExpense { get; set; }
    public decimal? DefaultNDSPercent { get; set; }
    public INomenklType NomenklType { get; set; }
    public IProductType ProductType { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsCurrencyTransfer { get; set; }
    public bool IsRentabelnost { get; set; }
    public bool IsOnlyState { get; set; }
    public ICountry Country { get; set; }
}
