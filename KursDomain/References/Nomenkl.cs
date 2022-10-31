using System;
using KursDomain.ICommon;
using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;
using IProductType = KursDomain.IReferences.Nomenkl.IProductType;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {NomenklNumber,nq} {Name,nq} {Currency,nq}")]
public class Nomenkl : IDocCode, IDocGuid,IName, IEqualityComparer<IDocCode>, INomenkl
{
    public decimal DocCode { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"{NomenklNumber} {Name} {Currency}";
    public string NomenklNumber { get; set; }
    public IUnit Unit { get; set; }
    public INomenklCategory Category { get; set; }
    public string FullName { get; set; }
    public bool IsUsluga { get; set; }
    public bool IsProguct { get; set; }
    public bool IsNakladExpense { get; set; }
    public ICurrency Currency { get; set; }
    public decimal? DefaultNDSPercent { get; set; }
    public INomenklType NomenklType { get; set; }
    public ISDRSchet SDRSchet { get; set; }
    public IProductType ProductType { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsCurrencyTransfer { get; set; }
    public bool IsUslugaInRentabelnost { get; set; }
    public INomenklMain NomenklMain { get; set; }

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

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_83 entity, IReferencesCache refCache)
    {
        DocCode = entity.DOC_CODE;
        Id = entity.Id;
        Name = entity.NOM_NAME;
        FullName = entity.NOM_FULL_NAME;
        Notes = entity.NOM_NOTES;
        NomenklNumber = entity.NOM_NOMENKL;
        Unit = refCache.GetUnit(entity.NOM_ED_IZM_DC);
        Category = refCache.GetNomenklCategory(entity.NOM_CATEG_DC);
        IsUsluga = entity.NOM_0MATER_1USLUGA == 1;
        IsProguct = entity.NOM_1PROD_0MATER == 1;
        IsNakladExpense = entity.NOM_1NAKLRASH_0NO == 1;
        Currency = refCache.GetCurrency(entity.NOM_SALE_CRS_DC);
        DefaultNDSPercent = (decimal?)entity.NOM_NDS_PERCENT;
        IsDeleted = entity.NOM_DELETED == 1;
        IsCurrencyTransfer = entity.IsCurrencyTransfer ?? false;
        IsUslugaInRentabelnost = entity.IsUslugaInRent ?? false;
    }
}
