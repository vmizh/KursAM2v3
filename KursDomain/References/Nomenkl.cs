using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using Helper;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {NomenklNumber,nq} {Name,nq} {Currency,nq}")]
public class Nomenkl : IDocCode, IDocGuid, IName, IEquatable<Nomenkl>, INomenkl
{
    [Display(AutoGenerateField = false)] public DateTime UpdateDate { get; set; }

    [Display(AutoGenerateField = false, Name = "Наименование")]
    [ReadOnly(true)]
    public Guid MainId { get; set; }

    [Display(AutoGenerateField = false)] public decimal DocCode { get; set; }

    [Display(AutoGenerateField = false)] public Guid Id { get; set; }

    public bool Equals(Nomenkl other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    [Display(AutoGenerateField = true, Name = "Наименование")]
    [ReadOnly(true)]
    public string Name { get; set; }

    [Display(AutoGenerateField = true, Name = "Примечания")]
    [ReadOnly(true)]
    public string Notes { get; set; }

    [Display(AutoGenerateField = false, Name = "Описание")]
    [ReadOnly(true)]
    public string Description => $"{NomenklNumber} {Name} {Currency}";

    [Display(AutoGenerateField = true, Name = "Ном.№")]
    [ReadOnly(true)]
    public string NomenklNumber { get; set; }

    [Display(AutoGenerateField = true, Name = "Ед.изм.")]
    [ReadOnly(true)]
    public IUnit Unit { get; set; }

    [Display(AutoGenerateField = false, Name = "Категория")]
    [ReadOnly(true)]
    public INomenklGroup Group { get; set; }

    [Display(AutoGenerateField = true, Name = "Полное имя")]
    [ReadOnly(true)]
    public string FullName { get; set; }

    [Display(AutoGenerateField = true, Name = "Услуга")]
    [ReadOnly(true)]
    public bool IsUsluga { get; set; }

    [Display(AutoGenerateField = true, Name = "Продукт")]
    [ReadOnly(true)]
    public bool IsProguct { get; set; }

    [Display(AutoGenerateField = true, Name = "Накл.расх.")]
    [ReadOnly(true)]
    public bool IsNakladExpense { get; set; }

    [Display(AutoGenerateField = true, Name = "Валюта")]
    [ReadOnly(true)]
    public ICurrency Currency { get; set; }

    [Display(AutoGenerateField = true, Name = "НДС,%")]
    [ReadOnly(true)]
    public decimal? DefaultNDSPercent { get; set; }

    [Display(AutoGenerateField = true, Name = "Тип товара")]
    [ReadOnly(true)]
    public INomenklType NomenklType { get; set; }

    [Display(AutoGenerateField = true, Name = "Счет. дох/расх")]
    [ReadOnly(true)]
    public ISDRSchet SDRSchet { get; set; }

    [Display(AutoGenerateField = true, Name = "Тип продукции")]
    [ReadOnly(true)]
    public IProductType ProductType { get; set; }

    [Display(AutoGenerateField = true, Name = "Удален")]
    [ReadOnly(true)]
    public bool IsDeleted { get; set; }

    [Display(AutoGenerateField = true, Name = "Вал.трансфер")]
    [ReadOnly(true)]
    public bool IsCurrencyTransfer { get; set; }

    [Display(AutoGenerateField = true, Name = "Усл. в рентабельности")]
    [ReadOnly(true)]
    public bool IsUslugaInRentabelnost { get; set; }

    [Display(AutoGenerateField = false, Name = "Основная ном.")]
    [ReadOnly(true)]
    public INomenklMain NomenklMain { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_83 entity, IReferencesCache refCache)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Id = entity.Id;
        Name = entity.NOM_NAME;
        FullName = entity.NOM_FULL_NAME;
        Notes = entity.NOM_NOTES;
        NomenklNumber = entity.NOM_NOMENKL;
        IsUsluga = entity.NOM_0MATER_1USLUGA == 1;
        IsProguct = entity.NOM_1PROD_0MATER == 1;
        IsNakladExpense = entity.NOM_1NAKLRASH_0NO == 1;
        DefaultNDSPercent = (decimal?) entity.NOM_NDS_PERCENT;
        IsDeleted = entity.NOM_DELETED == 1;
        IsCurrencyTransfer = entity.IsCurrencyTransfer ?? false;
        IsUslugaInRentabelnost = entity.IsUslugaInRent ?? false;
        UpdateDate = entity.UpdateDate ?? DateTime.MinValue;
        MainId = entity.MainId ?? Guid.Empty;

        Unit = refCache?.GetUnit(entity.NOM_ED_IZM_DC);
        Currency = refCache?.GetCurrency(entity.NOM_SALE_CRS_DC);
        Group = refCache?.GetNomenklGroup(entity.NOM_CATEG_DC);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Nomenkl) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }
}

[MetadataType(typeof(DataAnnotationsNomenklViewModel))]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class NomenklViewModel : RSViewModelBase, IEntity<SD_83>
{
    private Currency myCurrency;
    private SD_83 myEntity;
    private NomenklGroup myGroup;
    private ProductType myNomenklType;
    private Unit myUnit;

    public NomenklViewModel()
    {
        Entity = DefaultValue();
    }

    public NomenklViewModel(SD_83 entity)
    {
        Entity = entity ?? DefaultValue();
        LoadReference();
    }

    public override Guid Id
    {
        get => Entity.Id;
        set
        {
            if (Entity.Id == value) return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
    }

    [GridColumnView("%  НДС", SettingsType.Combo)]
    public decimal? NDSPercent
    {
        get => Entity.NOM_NDS_PERCENT == null ? null : (decimal) Entity.NOM_NDS_PERCENT;
        set
        {
            if (Math.Abs((Entity.NOM_NDS_PERCENT ?? 0) - (double) (value ?? 0)) < 0.01) return;
            Entity.NOM_NDS_PERCENT = (double?) value;
            RaisePropertyChanged();
        }
    }

    //s.NOM_1NAKLRASH_0NO,
    [GridColumnView("Накл. расход", SettingsType.Default)]
    public bool IsNaklRashod
    {
        get => Entity.NOM_1NAKLRASH_0NO == 1;
        set
        {
            if (Entity.NOM_1NAKLRASH_0NO == (value ? 1 : 0)) return;
            Entity.NOM_1NAKLRASH_0NO = value ? 1 : 0;
            RaisePropertyChanged();
        }
    }

    //s.NOM_1PROD_0MATER,
    [GridColumnView("Продукт", SettingsType.Default)]
    public bool IsProduct
    {
        get => Entity.NOM_1PROD_0MATER == 1;
        set
        {
            if (Entity.NOM_1PROD_0MATER == (value ? 1 : 0)) return;
            Entity.NOM_1PROD_0MATER = value ? 1 : 0;
            RaisePropertyChanged();
        }
    }

    //s.NOM_0MATER_1USLUGA,
    [GridColumnView("Услуга", SettingsType.Default)]
    public bool IsUsluga
    {
        get => Entity.NOM_0MATER_1USLUGA == 1;
        set
        {
            if (Entity.NOM_0MATER_1USLUGA == (value ? 1 : 0)) return;
            Entity.NOM_0MATER_1USLUGA = value ? 1 : 0;
            RaisePropertyChanged();
        }
    }

    [GridColumnView("Рентабельность", SettingsType.Default)]
    public bool IsRentabelnost
    {
        get => Entity.IsUslugaInRent ?? false;
        set
        {
            if (Entity.IsUslugaInRent == value) return;
            Entity.IsUslugaInRent = value;
            RaisePropertyChanged();
        }
    }

    [GridColumnView("Валютный учет", SettingsType.Default)]
    public bool IsCurrencyTransfer
    {
        get => Entity.IsCurrencyTransfer ?? false;
        set
        {
            if (Entity.IsCurrencyTransfer == value) return;
            Entity.IsCurrencyTransfer = value;
            RaisePropertyChanged();
        }
    }

    [GridColumnView("Ном. №", SettingsType.Default)]
    public string NomenklNumber
    {
        get => Entity.NOM_NOMENKL;
        set
        {
            if (Entity.NOM_NOMENKL == value) return;
            Entity.NOM_NOMENKL = value;
            RaisePropertyChanged();
        }
    }

    //s.NOM_FULL_NAME,
    [GridColumnView("Полное имя", SettingsType.Default)]
    public string NameFull
    {
        get => Entity.NOM_FULL_NAME;
        set
        {
            if (Entity.NOM_FULL_NAME == value) return;
            Entity.NOM_FULL_NAME = value;
            RaisePropertyChanged();
        }
    }

    [GridColumnView("Полное имя (доп)", SettingsType.Default)]
    public string PolnoeName
    {
        get => Entity.NOM_POLNOE_IMIA;
        set
        {
            if (Entity.NOM_POLNOE_IMIA == value) return;
            Entity.NOM_POLNOE_IMIA = value;
            RaisePropertyChanged();
        }
    }

    //s.NOM_DELETED,
    [GridColumnView("Удалена", SettingsType.Default)]
    public bool IsDeleted
    {
        get => Entity.NOM_DELETED == 1;
        set
        {
            if (Entity.NOM_DELETED == (value ? 1 : 0)) return;
            Entity.NOM_DELETED = value ? 1 : 0;
            RaisePropertyChanged();
        }
    }

    //sd_119
    [GridColumnView("Полное имя (доп)", SettingsType.Default)]
    public ProductType NomenklType
    {
        get => myNomenklType;
        set
        {
            if (myNomenklType != null && myNomenklType.Equals(value)) return;
            myNomenklType = value;
            RaisePropertyChanged();
        }
    }

    public Currency Currency
    {
        get => GlobalOptions.ReferencesCache.GetCurrency(NOM_SALE_CRS_DC) as Currency;
        set
        {
            if (Equals(myCurrency, value)) return;
            NOM_SALE_CRS_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    public Unit Unit
    {
        get => myUnit;
        set
        {
            if (myUnit != null && myUnit.Equals(value)) return;
            myUnit = value;
            RaisePropertyChanged();
        }
    }

    //s.NOM_CATEG_DC,
    [GridColumnView("Группа", SettingsType.Default)]
    public NomenklGroup Group
    {
        get => myGroup;
        set
        {
            if (myGroup != null && myGroup.Equals(value)) return;
            myGroup = value;
            RaisePropertyChanged();
        }
    }

    [GridColumnView("НДС в цене", SettingsType.Default)]
    public bool IsNDSInPrice
    {
        get => Entity.SF_NDS_1INCLUD_0NO == 1;
        set
        {
            if (Entity.SF_NDS_1INCLUD_0NO == (value ? 1 : 0)) return;
            Entity.SF_NDS_1INCLUD_0NO = value ? 1 : 0;
            RaisePropertyChanged();
        }
    }

    public Guid MainId
    {
        set
        {
            if (Entity.MainId == value) return;
            Entity.MainId = value;
            RaisePropertyChanged();
        }
        get => Entity.MainId ?? Guid.Empty;
    }

    public decimal DOC_CODE
    {
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public override decimal DocCode
    {
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public string NOM_BAR_KOD
    {
        get => Entity.NOM_BAR_KOD;
        set
        {
            if (Entity.NOM_BAR_KOD == value) return;
            Entity.NOM_BAR_KOD = value;
            RaisePropertyChanged();
        }
    }

    public string NOM_NOMENKL
    {
        get => Entity.NOM_NOMENKL;
        set
        {
            if (Entity.NOM_NOMENKL == value) return;
            Entity.NOM_NOMENKL = value;
            RaisePropertyChanged();
        }
    }

    public decimal NOM_ED_IZM_DC
    {
        get => Entity.NOM_ED_IZM_DC;
        set
        {
            if (Entity.NOM_ED_IZM_DC == value) return;
            Entity.NOM_ED_IZM_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal NOM_CATEG_DC
    {
        get => Entity.NOM_CATEG_DC;
        set
        {
            if (Entity.NOM_CATEG_DC == value) return;
            Entity.NOM_CATEG_DC = value;
            RaisePropertyChanged();
        }
    }

    public override string Name
    {
        get => NOM_NAME;
        set
        {
            if (NOM_NAME == value) return;
            NOM_NAME = value;
            RaisePropertyChanged();
        }
    }

    public string NOM_FULL_NAME
    {
        get => Entity.NOM_FULL_NAME;
        set
        {
            if (Entity.NOM_FULL_NAME == value) return;
            Entity.NOM_FULL_NAME = value;
            RaisePropertyChanged();
        }
    }

    public string NOM_NAME
    {
        get => Entity.NOM_NAME;
        set
        {
            if (Entity.NOM_NAME == value) return;
            Entity.NOM_NAME = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_SROK_HRAN_DNEY
    {
        get => Entity.NOM_SROK_HRAN_DNEY;
        set
        {
            if (Entity.NOM_SROK_HRAN_DNEY == value) return;
            Entity.NOM_SROK_HRAN_DNEY = value;
            RaisePropertyChanged();
        }
    }

    public int NOM_0MATER_1USLUGA
    {
        get => Entity.NOM_0MATER_1USLUGA;
        set
        {
            if (Entity.NOM_0MATER_1USLUGA == value) return;
            Entity.NOM_0MATER_1USLUGA = value;
            RaisePropertyChanged();
        }
    }

    public int NOM_1PROD_0MATER
    {
        get => Entity.NOM_1PROD_0MATER;
        set
        {
            if (Entity.NOM_1PROD_0MATER == value) return;
            Entity.NOM_1PROD_0MATER = value;
            RaisePropertyChanged();
        }
    }

    public int NOM_1NAKLRASH_0NO
    {
        get => Entity.NOM_1NAKLRASH_0NO;
        set
        {
            if (Entity.NOM_1NAKLRASH_0NO == value) return;
            Entity.NOM_1NAKLRASH_0NO = value;
            RaisePropertyChanged();
        }
    }

    public int? NOM_1PRICELIST_0NO
    {
        get => Entity.NOM_1PRICELIST_0NO;
        set
        {
            if (Entity.NOM_1PRICELIST_0NO == value) return;
            Entity.NOM_1PRICELIST_0NO = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_SALE_PRICE
    {
        get => Entity.NOM_SALE_PRICE;
        set
        {
            if (Entity.NOM_SALE_PRICE == value) return;
            Entity.NOM_SALE_PRICE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_SALE_CRS_DC
    {
        get => Entity.NOM_SALE_CRS_DC;
        set
        {
            if (Entity.NOM_SALE_CRS_DC == value) return;
            Entity.NOM_SALE_CRS_DC = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Currency));
        }
    }

    public double? NOM_NDS_PERCENT
    {
        get => Entity.NOM_NDS_PERCENT;
        set
        {
            if (Entity.NOM_NDS_PERCENT == value) return;
            Entity.NOM_NDS_PERCENT = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_SALE_CRS_PRICE
    {
        get => Entity.NOM_SALE_CRS_PRICE;
        set
        {
            if (Entity.NOM_SALE_CRS_PRICE == value) return;
            Entity.NOM_SALE_CRS_PRICE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_TYPE_DC
    {
        get => Entity.NOM_TYPE_DC;
        set
        {
            if (Entity.NOM_TYPE_DC == value) return;
            Entity.NOM_TYPE_DC = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_NACEN_KOMPL_PERS
    {
        get => Entity.NOM_NACEN_KOMPL_PERS;
        set
        {
            if (Entity.NOM_NACEN_KOMPL_PERS == value) return;
            Entity.NOM_NACEN_KOMPL_PERS = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_NACEN_KOMPL_SUM
    {
        get => Entity.NOM_NACEN_KOMPL_SUM;
        set
        {
            if (Entity.NOM_NACEN_KOMPL_SUM == value) return;
            Entity.NOM_NACEN_KOMPL_SUM = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_NACEN_OTD_PERS
    {
        get => Entity.NOM_NACEN_OTD_PERS;
        set
        {
            if (Entity.NOM_NACEN_OTD_PERS == value) return;
            Entity.NOM_NACEN_OTD_PERS = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_NACEN_OTD_SUM
    {
        get => Entity.NOM_NACEN_OTD_SUM;
        set
        {
            if (Entity.NOM_NACEN_OTD_SUM == value) return;
            Entity.NOM_NACEN_OTD_SUM = value;
            RaisePropertyChanged();
        }
    }

    public short? NOM_RAZDELN_UCHET
    {
        get => Entity.NOM_RAZDELN_UCHET;
        set
        {
            if (Entity.NOM_RAZDELN_UCHET == value) return;
            Entity.NOM_RAZDELN_UCHET = value;
            RaisePropertyChanged();
        }
    }

    public string NOM_SHPZ
    {
        get => Entity.NOM_SHPZ;
        set
        {
            if (Entity.NOM_SHPZ == value) return;
            Entity.NOM_SHPZ = value;
            RaisePropertyChanged();
        }
    }

    public int? NOM_ACC
    {
        get => Entity.NOM_ACC;
        set
        {
            if (Entity.NOM_ACC == value) return;
            Entity.NOM_ACC = value;
            RaisePropertyChanged();
        }
    }

    public int? NOM_SUBACC
    {
        get => Entity.NOM_SUBACC;
        set
        {
            if (Entity.NOM_SUBACC == value) return;
            Entity.NOM_SUBACC = value;
            RaisePropertyChanged();
        }
    }

    public int? NOM_MBP
    {
        get => Entity.NOM_MBP;
        set
        {
            if (Entity.NOM_MBP == value) return;
            Entity.NOM_MBP = value;
            RaisePropertyChanged();
        }
    }

    public decimal NOM_PRODUCT_DC
    {
        get => Entity.NOM_PRODUCT_DC;
        set
        {
            if (Entity.NOM_PRODUCT_DC == value) return;
            Entity.NOM_PRODUCT_DC = value;
            RaisePropertyChanged();
        }
    }

    public string NOM_OKP
    {
        get => Entity.NOM_OKP;
        set
        {
            if (Entity.NOM_OKP == value) return;
            Entity.NOM_OKP = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_PRODUCTOR_DC
    {
        get => Entity.NOM_PRODUCTOR_DC;
        set
        {
            if (Entity.NOM_PRODUCTOR_DC == value) return;
            Entity.NOM_PRODUCTOR_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_PROIZVODSTVO
    {
        get => Entity.NOM_PROIZVODSTVO;
        set
        {
            if (Entity.NOM_PROIZVODSTVO == value) return;
            Entity.NOM_PROIZVODSTVO = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_KOMPLEKT_ID
    {
        get => Entity.NOM_KOMPLEKT_ID;
        set
        {
            if (Entity.NOM_KOMPLEKT_ID == value) return;
            Entity.NOM_KOMPLEKT_ID = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_BASE_KOMPLEKT_DC
    {
        get => Entity.NOM_BASE_KOMPLEKT_DC;
        set
        {
            if (Entity.NOM_BASE_KOMPLEKT_DC == value) return;
            Entity.NOM_BASE_KOMPLEKT_DC = value;
            RaisePropertyChanged();
        }
    }

    public string NOM_POLNOE_IMIA
    {
        get => Entity.NOM_POLNOE_IMIA;
        set
        {
            if (Entity.NOM_POLNOE_IMIA == value) return;
            Entity.NOM_POLNOE_IMIA = value;
            RaisePropertyChanged();
        }
    }

    public int? NOM_1PRINT_IN_PR_LIST_NO
    {
        get => Entity.NOM_1PRINT_IN_PR_LIST_NO;
        set
        {
            if (Entity.NOM_1PRINT_IN_PR_LIST_NO == value) return;
            Entity.NOM_1PRINT_IN_PR_LIST_NO = value;
            RaisePropertyChanged();
        }
    }

    public int? NOM_DELETED
    {
        get => Entity.NOM_DELETED;
        set
        {
            if (Entity.NOM_DELETED == value) return;
            Entity.NOM_DELETED = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_SHPZ_DEF_IN_COMPL_DC
    {
        get => Entity.NOM_SHPZ_DEF_IN_COMPL_DC;
        set
        {
            if (Entity.NOM_SHPZ_DEF_IN_COMPL_DC == value) return;
            Entity.NOM_SHPZ_DEF_IN_COMPL_DC = value;
            RaisePropertyChanged();
        }
    }

    public short? NOM_DEEP_LEVEL
    {
        get => Entity.NOM_DEEP_LEVEL;
        set
        {
            if (Entity.NOM_DEEP_LEVEL == value) return;
            Entity.NOM_DEEP_LEVEL = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_VES
    {
        get => Entity.NOM_VES;
        set
        {
            if (Entity.NOM_VES == value) return;
            Entity.NOM_VES = value;
            RaisePropertyChanged();
        }
    }

    public short? NOM_1EDIT_KOMP_PRICE_0NO
    {
        get => Entity.NOM_1EDIT_KOMP_PRICE_0NO;
        set
        {
            if (Entity.NOM_1EDIT_KOMP_PRICE_0NO == value) return;
            Entity.NOM_1EDIT_KOMP_PRICE_0NO = value;
            RaisePropertyChanged();
        }
    }

    public string NOM_NOTES
    {
        get => Entity.NOM_NOTES;
        set
        {
            if (Entity.NOM_NOTES == value) return;
            Entity.NOM_NOTES = value;
            RaisePropertyChanged();
        }
    }

    public string NOM_STRANA_IZGOTOV
    {
        get => Entity.NOM_STRANA_IZGOTOV;
        set
        {
            if (Entity.NOM_STRANA_IZGOTOV == value) return;
            Entity.NOM_STRANA_IZGOTOV = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_GTD_FROM_NOMENKL_DC
    {
        get => Entity.NOM_GTD_FROM_NOMENKL_DC;
        set
        {
            if (Entity.NOM_GTD_FROM_NOMENKL_DC == value) return;
            Entity.NOM_GTD_FROM_NOMENKL_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? NOM_VHOD_KONTR
    {
        get => Entity.NOM_VHOD_KONTR;
        set
        {
            if (Entity.NOM_VHOD_KONTR == value) return;
            Entity.NOM_VHOD_KONTR = value;
            RaisePropertyChanged();
        }
    }

    public int? NOM_GAR_NUM_ENABLE
    {
        get => Entity.NOM_GAR_NUM_ENABLE;
        set
        {
            if (Entity.NOM_GAR_NUM_ENABLE == value) return;
            Entity.NOM_GAR_NUM_ENABLE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_CO_ZAK_DEF_DC
    {
        get => Entity.NOM_CO_ZAK_DEF_DC;
        set
        {
            if (Entity.NOM_CO_ZAK_DEF_DC == value) return;
            Entity.NOM_CO_ZAK_DEF_DC = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_SROK_PROIZVODSTVA
    {
        get => Entity.NOM_SROK_PROIZVODSTVA;
        set
        {
            if (Entity.NOM_SROK_PROIZVODSTVA == value) return;
            Entity.NOM_SROK_PROIZVODSTVA = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_TARA_DC
    {
        get => Entity.NOM_TARA_DC;
        set
        {
            if (Entity.NOM_TARA_DC == value) return;
            Entity.NOM_TARA_DC = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_KOL_IN_ED_TARA
    {
        get => Entity.NOM_KOL_IN_ED_TARA;
        set
        {
            if (Entity.NOM_KOL_IN_ED_TARA == value) return;
            Entity.NOM_KOL_IN_ED_TARA = value;
            RaisePropertyChanged();
        }
    }

    public short? NOM_IN_PRODUCE_DO_MAIN_NOM
    {
        get => Entity.NOM_IN_PRODUCE_DO_MAIN_NOM;
        set
        {
            if (Entity.NOM_IN_PRODUCE_DO_MAIN_NOM == value) return;
            Entity.NOM_IN_PRODUCE_DO_MAIN_NOM = value;
            RaisePropertyChanged();
        }
    }

    public short? NOM_PROIZV_UCH_PO_PARTIAM
    {
        get => Entity.NOM_PROIZV_UCH_PO_PARTIAM;
        set
        {
            if (Entity.NOM_PROIZV_UCH_PO_PARTIAM == value) return;
            Entity.NOM_PROIZV_UCH_PO_PARTIAM = value;
            RaisePropertyChanged();
        }
    }

    public decimal? NOM_CO_REALIZ_DEF_DC
    {
        get => Entity.NOM_CO_REALIZ_DEF_DC;
        set
        {
            if (Entity.NOM_CO_REALIZ_DEF_DC == value) return;
            Entity.NOM_CO_REALIZ_DEF_DC = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_HEIGHT
    {
        get => Entity.NOM_HEIGHT;
        set
        {
            if (Entity.NOM_HEIGHT == value) return;
            Entity.NOM_HEIGHT = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_LENGTH
    {
        get => Entity.NOM_LENGTH;
        set
        {
            if (Entity.NOM_LENGTH == value) return;
            Entity.NOM_LENGTH = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_WIDTH
    {
        get => Entity.NOM_WIDTH;
        set
        {
            if (Entity.NOM_WIDTH == value) return;
            Entity.NOM_WIDTH = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_OBYEM
    {
        get => Entity.NOM_OBYEM;
        set
        {
            if (Entity.NOM_OBYEM == value) return;
            Entity.NOM_OBYEM = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_BRUTTO_HEIGHT
    {
        get => Entity.NOM_BRUTTO_HEIGHT;
        set
        {
            if (Entity.NOM_BRUTTO_HEIGHT == value) return;
            Entity.NOM_BRUTTO_HEIGHT = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_BRUTTO_LENGTH
    {
        get => Entity.NOM_BRUTTO_LENGTH;
        set
        {
            if (Entity.NOM_BRUTTO_LENGTH == value) return;
            Entity.NOM_BRUTTO_LENGTH = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_DRUTTO_WIDTH
    {
        get => Entity.NOM_DRUTTO_WIDTH;
        set
        {
            if (Entity.NOM_DRUTTO_WIDTH == value) return;
            Entity.NOM_DRUTTO_WIDTH = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_BRUTTO_VES
    {
        get => Entity.NOM_BRUTTO_VES;
        set
        {
            if (Entity.NOM_BRUTTO_VES == value) return;
            Entity.NOM_BRUTTO_VES = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_BRUTTO_WIDTH
    {
        get => Entity.NOM_BRUTTO_WIDTH;
        set
        {
            if (Entity.NOM_BRUTTO_WIDTH == value) return;
            Entity.NOM_BRUTTO_WIDTH = value;
            RaisePropertyChanged();
        }
    }

    public double? NOM_BRUTTO_OBYEM
    {
        get => Entity.NOM_BRUTTO_OBYEM;
        set
        {
            if (Entity.NOM_BRUTTO_OBYEM == value) return;
            Entity.NOM_BRUTTO_OBYEM = value;
            RaisePropertyChanged();
        }
    }

    public int? SF_NDS_1INCLUD_0NO
    {
        get => Entity.SF_NDS_1INCLUD_0NO;
        set
        {
            if (Entity.SF_NDS_1INCLUD_0NO == value) return;
            Entity.SF_NDS_1INCLUD_0NO = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? UpdateDate
    {
        get => Entity.UpdateDate;
        set
        {
            if (Entity.UpdateDate == value) return;
            Entity.UpdateDate = value;
            RaisePropertyChanged();
        }
    }

    public SD_119 SD_119
    {
        get => Entity.SD_119;
        set
        {
            if (Entity.SD_119 == value) return;
            Entity.SD_119 = value;
            RaisePropertyChanged();
        }
    }

    public SD_175 SD_175
    {
        get => Entity.SD_175;
        set
        {
            if (Entity.SD_175 == value) return;
            Entity.SD_175 = value;
            RaisePropertyChanged();
        }
    }

    public SD_303 SD_303
    {
        get => Entity.SD_303;
        set
        {
            if (Entity.SD_303 == value) return;
            Entity.SD_303 = value;
            RaisePropertyChanged();
        }
    }

    public SD_40 SD_40
    {
        get => Entity.SD_40;
        set
        {
            if (Entity.SD_40 == value) return;
            Entity.SD_40 = value;
            RaisePropertyChanged();
        }
    }

    public SD_40 SD_401
    {
        get => Entity.SD_401;
        set
        {
            if (Entity.SD_401 == value) return;
            Entity.SD_401 = value;
            RaisePropertyChanged();
        }
    }

    public SD_43 SD_43
    {
        get => Entity.SD_43;
        set
        {
            if (Entity.SD_43 == value) return;
            Entity.SD_43 = value;
            RaisePropertyChanged();
        }
    }

    public SD_50 SD_50
    {
        get => Entity.SD_50;
        set
        {
            if (Entity.SD_50 == value) return;
            Entity.SD_50 = value;
            RaisePropertyChanged();
        }
    }

    public SD_82 SD_82
    {
        get => Entity.SD_82;
        set
        {
            if (Entity.SD_82 == value) return;
            Entity.SD_82 = value;
            RaisePropertyChanged();
        }
    }

    public SD_83 SD_832
    {
        get => Entity.SD_832;
        set
        {
            if (Entity.SD_832 == value) return;
            Entity.SD_832 = value;
            RaisePropertyChanged();
        }
    }

    public SD_83 SD_833
    {
        get => Entity.SD_833;
        set
        {
            if (Entity.SD_833 == value) return;
            Entity.SD_833 = value;
            RaisePropertyChanged();
        }
    }

    public TD_193 TD_193
    {
        get => Entity.TD_193;
        set
        {
            if (Entity.TD_193 == value) return;
            Entity.TD_193 = value;
            RaisePropertyChanged();
        }
    }

    public XD_83 XD_83
    {
        get => Entity.XD_83;
        set
        {
            if (Entity.XD_83 == value) return;
            Entity.XD_83 = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public SD_83 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_83 DefaultValue()
    {
        return new SD_83
        {
            DOC_CODE = -1,
            Id = Guid.NewGuid()
        };
    }

    public bool Check()
    {
        return !string.IsNullOrEmpty(Name) && Currency != null;
    }

    public List<SD_83> LoadList()
    {
        throw new NotImplementedException();
    }

    private void LoadReference()
    {
        // ReSharper disable once PossibleNullReferenceException
        Unit = GlobalOptions.ReferencesCache.GetUnit(Entity.NOM_ED_IZM_DC) as Unit;
        Group = GlobalOptions.ReferencesCache.GetNomenklGroup(Entity.NOM_CATEG_DC) as NomenklGroup;
        NomenklType = GlobalOptions.ReferencesCache.GetProductType(Entity.NOM_TYPE_DC) as ProductType;
    }

    public override string ToString()
    {
        return Name;
    }

    public virtual void Save(SD_83 doc)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public void Delete()
    {
        throw new NotImplementedException();
    }

    public void Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Delete(decimal dc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(SD_83 ent)
    {
        NOM_BAR_KOD = ent.NOM_BAR_KOD;
        NOM_NOMENKL = ent.NOM_NOMENKL;
        NOM_ED_IZM_DC = ent.NOM_ED_IZM_DC;
        NOM_CATEG_DC = ent.NOM_CATEG_DC;
        NOM_FULL_NAME = ent.NOM_FULL_NAME;
        NOM_NAME = ent.NOM_NAME;
        NOM_SROK_HRAN_DNEY = ent.NOM_SROK_HRAN_DNEY;
        NOM_0MATER_1USLUGA = ent.NOM_0MATER_1USLUGA;
        NOM_1PROD_0MATER = ent.NOM_1PROD_0MATER;
        NOM_1NAKLRASH_0NO = ent.NOM_1NAKLRASH_0NO;
        NOM_1PRICELIST_0NO = ent.NOM_1PRICELIST_0NO;
        NOM_SALE_PRICE = ent.NOM_SALE_PRICE;
        NOM_SALE_CRS_DC = ent.NOM_SALE_CRS_DC;
        NOM_NDS_PERCENT = ent.NOM_NDS_PERCENT;
        NOM_SALE_CRS_PRICE = ent.NOM_SALE_CRS_PRICE;
        NOM_TYPE_DC = ent.NOM_TYPE_DC;
        NOM_NACEN_KOMPL_PERS = ent.NOM_NACEN_KOMPL_PERS;
        NOM_NACEN_KOMPL_SUM = ent.NOM_NACEN_KOMPL_SUM;
        NOM_NACEN_OTD_PERS = ent.NOM_NACEN_OTD_PERS;
        NOM_NACEN_OTD_SUM = ent.NOM_NACEN_OTD_SUM;
        NOM_RAZDELN_UCHET = ent.NOM_RAZDELN_UCHET;
        NOM_SHPZ = ent.NOM_SHPZ;
        NOM_ACC = ent.NOM_ACC;
        NOM_SUBACC = ent.NOM_SUBACC;
        NOM_MBP = ent.NOM_MBP;
        NOM_PRODUCT_DC = ent.NOM_PRODUCT_DC;
        NOM_OKP = ent.NOM_OKP;
        NOM_PRODUCTOR_DC = ent.NOM_PRODUCTOR_DC;
        NOM_PROIZVODSTVO = ent.NOM_PROIZVODSTVO;
        NOM_KOMPLEKT_ID = ent.NOM_KOMPLEKT_ID;
        NOM_BASE_KOMPLEKT_DC = ent.NOM_BASE_KOMPLEKT_DC;
        NOM_POLNOE_IMIA = ent.NOM_POLNOE_IMIA;
        NOM_1PRINT_IN_PR_LIST_NO = ent.NOM_1PRINT_IN_PR_LIST_NO;
        NOM_DELETED = ent.NOM_DELETED;
        NOM_SHPZ_DEF_IN_COMPL_DC = ent.NOM_SHPZ_DEF_IN_COMPL_DC;
        NOM_DEEP_LEVEL = ent.NOM_DEEP_LEVEL;
        NOM_VES = ent.NOM_VES;
        NOM_1EDIT_KOMP_PRICE_0NO = ent.NOM_1EDIT_KOMP_PRICE_0NO;
        NOM_NOTES = ent.NOM_NOTES;
        NOM_STRANA_IZGOTOV = ent.NOM_STRANA_IZGOTOV;
        NOM_GTD_FROM_NOMENKL_DC = ent.NOM_GTD_FROM_NOMENKL_DC;
        NOM_VHOD_KONTR = ent.NOM_VHOD_KONTR;
        NOM_GAR_NUM_ENABLE = ent.NOM_GAR_NUM_ENABLE;
        NOM_CO_ZAK_DEF_DC = ent.NOM_CO_ZAK_DEF_DC;
        NOM_SROK_PROIZVODSTVA = ent.NOM_SROK_PROIZVODSTVA;
        NOM_TARA_DC = ent.NOM_TARA_DC;
        NOM_KOL_IN_ED_TARA = ent.NOM_KOL_IN_ED_TARA;
        NOM_IN_PRODUCE_DO_MAIN_NOM = ent.NOM_IN_PRODUCE_DO_MAIN_NOM;
        NOM_PROIZV_UCH_PO_PARTIAM = ent.NOM_PROIZV_UCH_PO_PARTIAM;
        NOM_CO_REALIZ_DEF_DC = ent.NOM_CO_REALIZ_DEF_DC;
        NOM_HEIGHT = ent.NOM_HEIGHT;
        NOM_LENGTH = ent.NOM_LENGTH;
        NOM_WIDTH = ent.NOM_WIDTH;
        NOM_OBYEM = ent.NOM_OBYEM;
        NOM_BRUTTO_HEIGHT = ent.NOM_BRUTTO_HEIGHT;
        NOM_BRUTTO_LENGTH = ent.NOM_BRUTTO_LENGTH;
        NOM_DRUTTO_WIDTH = ent.NOM_DRUTTO_WIDTH;
        NOM_BRUTTO_VES = ent.NOM_BRUTTO_VES;
        NOM_BRUTTO_WIDTH = ent.NOM_BRUTTO_WIDTH;
        NOM_BRUTTO_OBYEM = ent.NOM_BRUTTO_OBYEM;
        SF_NDS_1INCLUD_0NO = ent.SF_NDS_1INCLUD_0NO;
        UpdateDate = ent.UpdateDate;
        MainId = ent.MainId ?? Guid.Empty;
        SD_119 = ent.SD_119;
        SD_175 = ent.SD_175;
        SD_303 = ent.SD_303;
        SD_40 = ent.SD_40;
        SD_401 = ent.SD_401;
        SD_43 = ent.SD_43;
        SD_50 = ent.SD_50;
        SD_82 = ent.SD_82;
        SD_832 = ent.SD_832;
        SD_833 = ent.SD_833;
        TD_193 = ent.TD_193;
        XD_83 = ent.XD_83;
    }

    public void UpdateTo(SD_83 ent)
    {
        ent.NOM_BAR_KOD = NOM_BAR_KOD;
        ent.NOM_NOMENKL = NOM_NOMENKL;
        ent.NOM_ED_IZM_DC = NOM_ED_IZM_DC;
        ent.NOM_CATEG_DC = NOM_CATEG_DC;
        ent.NOM_FULL_NAME = NOM_FULL_NAME;
        ent.NOM_NAME = NOM_NAME;
        ent.NOM_SROK_HRAN_DNEY = NOM_SROK_HRAN_DNEY;
        ent.NOM_0MATER_1USLUGA = NOM_0MATER_1USLUGA;
        ent.NOM_1PROD_0MATER = NOM_1PROD_0MATER;
        ent.NOM_1NAKLRASH_0NO = NOM_1NAKLRASH_0NO;
        ent.NOM_1PRICELIST_0NO = NOM_1PRICELIST_0NO;
        ent.NOM_SALE_PRICE = NOM_SALE_PRICE;
        ent.NOM_SALE_CRS_DC = NOM_SALE_CRS_DC;
        ent.NOM_NDS_PERCENT = NOM_NDS_PERCENT;
        ent.NOM_SALE_CRS_PRICE = NOM_SALE_CRS_PRICE;
        ent.NOM_TYPE_DC = NOM_TYPE_DC;
        ent.NOM_NACEN_KOMPL_PERS = NOM_NACEN_KOMPL_PERS;
        ent.NOM_NACEN_KOMPL_SUM = NOM_NACEN_KOMPL_SUM;
        ent.NOM_NACEN_OTD_PERS = NOM_NACEN_OTD_PERS;
        ent.NOM_NACEN_OTD_SUM = NOM_NACEN_OTD_SUM;
        ent.NOM_RAZDELN_UCHET = NOM_RAZDELN_UCHET;
        ent.NOM_SHPZ = NOM_SHPZ;
        ent.NOM_ACC = NOM_ACC;
        ent.NOM_SUBACC = NOM_SUBACC;
        ent.NOM_MBP = NOM_MBP;
        ent.NOM_PRODUCT_DC = NOM_PRODUCT_DC;
        ent.NOM_OKP = NOM_OKP;
        ent.NOM_PRODUCTOR_DC = NOM_PRODUCTOR_DC;
        ent.NOM_PROIZVODSTVO = NOM_PROIZVODSTVO;
        ent.NOM_KOMPLEKT_ID = NOM_KOMPLEKT_ID;
        ent.NOM_BASE_KOMPLEKT_DC = NOM_BASE_KOMPLEKT_DC;
        ent.NOM_POLNOE_IMIA = NOM_POLNOE_IMIA;
        ent.NOM_1PRINT_IN_PR_LIST_NO = NOM_1PRINT_IN_PR_LIST_NO;
        ent.NOM_DELETED = NOM_DELETED;
        ent.NOM_SHPZ_DEF_IN_COMPL_DC = NOM_SHPZ_DEF_IN_COMPL_DC;
        ent.NOM_DEEP_LEVEL = NOM_DEEP_LEVEL;
        ent.NOM_VES = NOM_VES;
        ent.NOM_1EDIT_KOMP_PRICE_0NO = NOM_1EDIT_KOMP_PRICE_0NO;
        ent.NOM_NOTES = NOM_NOTES;
        ent.NOM_STRANA_IZGOTOV = NOM_STRANA_IZGOTOV;
        ent.NOM_GTD_FROM_NOMENKL_DC = NOM_GTD_FROM_NOMENKL_DC;
        ent.NOM_VHOD_KONTR = NOM_VHOD_KONTR;
        ent.NOM_GAR_NUM_ENABLE = NOM_GAR_NUM_ENABLE;
        ent.NOM_CO_ZAK_DEF_DC = NOM_CO_ZAK_DEF_DC;
        ent.NOM_SROK_PROIZVODSTVA = NOM_SROK_PROIZVODSTVA;
        ent.NOM_TARA_DC = NOM_TARA_DC;
        ent.NOM_KOL_IN_ED_TARA = NOM_KOL_IN_ED_TARA;
        ent.NOM_IN_PRODUCE_DO_MAIN_NOM = NOM_IN_PRODUCE_DO_MAIN_NOM;
        ent.NOM_PROIZV_UCH_PO_PARTIAM = NOM_PROIZV_UCH_PO_PARTIAM;
        ent.NOM_CO_REALIZ_DEF_DC = NOM_CO_REALIZ_DEF_DC;
        ent.NOM_HEIGHT = NOM_HEIGHT;
        ent.NOM_LENGTH = NOM_LENGTH;
        ent.NOM_WIDTH = NOM_WIDTH;
        ent.NOM_OBYEM = NOM_OBYEM;
        ent.NOM_BRUTTO_HEIGHT = NOM_BRUTTO_HEIGHT;
        ent.NOM_BRUTTO_LENGTH = NOM_BRUTTO_LENGTH;
        ent.NOM_DRUTTO_WIDTH = NOM_DRUTTO_WIDTH;
        ent.NOM_BRUTTO_VES = NOM_BRUTTO_VES;
        ent.NOM_BRUTTO_WIDTH = NOM_BRUTTO_WIDTH;
        ent.NOM_BRUTTO_OBYEM = NOM_BRUTTO_OBYEM;
        ent.SF_NDS_1INCLUD_0NO = SF_NDS_1INCLUD_0NO;
        ent.UpdateDate = UpdateDate;
        ent.MainId = MainId;
        ent.SD_119 = SD_119;
        ent.SD_175 = SD_175;
        ent.SD_303 = SD_303;
        ent.SD_40 = SD_40;
        ent.SD_401 = SD_401;
        ent.SD_43 = SD_43;
        ent.SD_50 = SD_50;
        ent.SD_82 = SD_82;
        ent.SD_832 = SD_832;
        ent.SD_833 = SD_833;
        ent.TD_193 = TD_193;
        ent.XD_83 = XD_83;
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public SD_83 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public SD_83 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual SD_83 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual SD_83 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    //TODO Перенести в номенкл менеджер

    #region Аналитические функции

    public static decimal Quantity(Warehouse warehouse, Nomenkl nom, DateTime date)
    {
        return Quantity(warehouse.DocCode, nom.DocCode, date);
    }

    public static decimal Quantity(decimal? storeDC, decimal nomDC, DateTime date)
    {
        if (storeDC != null)
            return Quantity(storeDC.Value, nomDC, date);
        return 0;
    }

    public static decimal Quantity(decimal storeDC, decimal nomDC, DateTime date)
    {
        decimal q = 0;
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var qq =
                    ctx.NomenklMoveWithPrice.AsNoTracking()
                        .Where(_ => _.NomDC == nomDC && _.StoreDC == storeDC && _.Date <= date);
                if (qq.Any())
                    q =
                        ctx.NomenklMoveWithPrice.AsNoTracking()
                            .Where(_ => _.NomDC == nomDC && _.StoreDC == storeDC && _.Date <= date)
                            .Sum(_ => _.Prihod - _.Rashod);
            }

            return q;
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(null, ex);
            return 0;
        }
    }

    public static decimal Price(decimal nomDC, DateTime date)
    {
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var nomPrice = ctx.NOM_PRICE.AsNoTracking()
                    .FirstOrDefault(_ => _.NOM_DC == nomDC &&
                                         _.DATE ==
                                         ctx.NOM_PRICE.Where(
                                                 c => c.NOM_DC == nomDC && c.DATE <= date)
                                             .Max(d => d.DATE));
                if (nomPrice == null) return 0;
                var q = GlobalOptions.SystemProfile.NomenklCalcType == NomenklCalcType.Standart
                    ? nomPrice.PRICE
                    : nomPrice.PRICE_WO_NAKLAD;
                return q;
            }
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(null, ex);
            return 0;
        }
    }

    public static decimal Price(Nomenkl nom, DateTime date)
    {
        return Price(nom.DocCode, date);
    }

    public static decimal PriceWithOutNaklad(decimal nomDC, DateTime date)
    {
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var nomPrice = ctx.NOM_PRICE.FirstOrDefault(_ => _.NOM_DC == nomDC &&
                                                                 _.DATE ==
                                                                 ctx.NOM_PRICE.Where(
                                                                         c => c.NOM_DC == nomDC && c.DATE <= date)
                                                                     .Max(d => d.DATE));
                if (nomPrice == null) return 0;
                var q = nomPrice.PRICE_WO_NAKLAD;
                return q;
            }
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(null, ex);
            return 0;
        }
    }

    public static decimal PriceWithOutNaklad(Nomenkl nom, DateTime date)
    {
        return PriceWithOutNaklad(nom.DocCode, date);
    }

    public static decimal QuantityAll(Nomenkl nom, DateTime date)
    {
        return QuantityAll(nom.DocCode, date);
    }

    public static decimal QuantityAll(decimal nomDC, DateTime date)
    {
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var q =
                    ctx.NomenklMoveWithPrice.AsNoTracking()
                        .Where(_ => _.NomDC == nomDC && _.Date <= date)
                        .Sum(_ => _.Prihod - _.Rashod);
                return q;
            }
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(null, ex);
            return 0;
        }
    }

    public static List<NomenklMoveWithPrice> GetMoveWithNakopit(decimal nomDC)
    {
        var ret = new List<NomenklMoveWithPrice>();
        var nom = GlobalOptions.ReferencesCache.GetNomenkl(nomDC);
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var s = "SELECT NomDC, Date, SUM(Prihod)Prihod, SUM(Rashod) Rashod " +
                        "FROM NomenklMoveWithPrice " +
                        $" where NomDC = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                        "GROUP BY NomDC, Date " +
                        "HAVING SUM(Prihod) != 0 OR SUM(Rashod) != 0" +
                        "ORDER BY 1, 2";
                var q = ctx.Database.SqlQuery<ReturnQueryNomMove>(s).ToList();
                decimal nak = 0;
                foreach (var d in q)
                {
                    nak += d.Prihod - d.Prihod;
                    ret.Add(new NomenklMoveWithPrice
                    {
                        Date = d.Date,
                        Prihod = d.Prihod,
                        Rashod = d.Rashod,
                        Nakopit = nak,
                        NomDC = nomDC,
                        NomCrsDC = ((IDocCode) nom.Currency).DocCode,
                        NomName = ((IName) nom).Name,
                        NomNomenkl = nom.NomenklNumber
                    });
                }
            }

            return ret;
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(null, ex);
            return null;
        }
    }

    /// <summary>
    ///     Возвращает последнюю дату, когда кол-во на складах по номенклатуре равно нулю
    /// </summary>
    public static DateTime GetLastDateNomenklZero(decimal nomDC, DateTime date)
    {
        var ret = new DateTime(2000, 1, 1);
        var d = GetMoveWithNakopit(nomDC).Where(_ => _.Date <= date).ToList();
        var nomenklMoveWithPriceNakopits = d as IList<NomenklMoveWithPrice>;
        if (!nomenklMoveWithPriceNakopits.Any()) return ret;
        return nomenklMoveWithPriceNakopits.All(_ => _.Nakopit != 0)
            ? ret
            : nomenklMoveWithPriceNakopits.Max(_ => _.Date);
    }

    private class ReturnQueryNomMove
    {
        //NomDC,Date,SUM(Prihod) Prihod,SUM(Rashod) Rashod 
        public DateTime Date { set; get; }
        public decimal Prihod { set; get; }
        public decimal Rashod { set; get; }
    }

    #endregion
}

public class DataAnnotationsNomenklViewModel : DataAnnotationForFluentApiBase, IMetadataProvider<NomenklViewModel>
{
    void IMetadataProvider<NomenklViewModel>.BuildMetadata(MetadataBuilder<NomenklViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.NomenklType).NotAutoGenerated();
        builder.Property(_ => _.Currency).NotAutoGenerated();
        builder.Property(_ => _.Group).NotAutoGenerated();
        builder.Property(_ => _.NOM_NAME).AutoGenerated().DisplayName("Наименование");
        builder.Property(_ => _.NomenklNumber).AutoGenerated().DisplayName("Ном.№").ReadOnly();
        builder.Property(_ => _.IsUsluga).AutoGenerated().DisplayName("Услуга");
        builder.Property(_ => _.Unit).AutoGenerated().DisplayName("Ед. Изм.");
    }
}
