using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KursDomain.Annotations;
using KursDomain.References;
using KursDomain.Wrapper;

namespace KursDomain.RepositoryHelper;

public class NomenklStoreRemainItemWrapper
{
    public NomenklStoreRemainItemWrapper(NomenklStoreRemainItem item)
    {
        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(item.NomenklDC) as Nomenkl;
        Warehouse = GlobalOptions.ReferencesCache.GetWarehouse(item.StoreDC) as Warehouse;
        Remain = item.Remain;
        Price = item.Price;
        PriceWithNaklad = item.PriceWithNaklad;
        LastOperationDate = item.LastOperationDate;
    }

    [Display(AutoGenerateField = true, Name = "Ном.№")]
    [ReadOnly(true)]
    public string NomenklNumner => Nomenkl?.NomenklNumber;

    [Display(AutoGenerateField = true, Name = "Наименование")]
    [ReadOnly(true)]
    public Nomenkl Nomenkl { get; }

    [Display(AutoGenerateField = true, Name = "Валюта")]
    [ReadOnly(true)]
    public Currency Currency => Nomenkl?.Currency as Currency;

    [Display(AutoGenerateField = true, Name = "Склад")]
    [ReadOnly(true)]
    public Warehouse Warehouse { get; private set; }

    [Display(AutoGenerateField = true, Name = "Остаток")]
    [DisplayFormat(DataFormatString = "n2")]
    [ReadOnly(true)]
    public decimal Remain { get; }

    [Display(AutoGenerateField = true, Name = "Цена с накл.")]
    [DisplayFormat(DataFormatString = "n2")]
    [ReadOnly(true)]
    public decimal PriceWithNaklad { get; }

    [Display(AutoGenerateField = true, Name = "Цена")]
    [DisplayFormat(DataFormatString = "n2")]
    [ReadOnly(true)]
    public decimal Price { get; }

    [Display(AutoGenerateField = true, Name = "Сумма")]
    [DisplayFormat(DataFormatString = "n2")]
    [ReadOnly(true)]
    public decimal Summa => Price * Remain;

    [Display(AutoGenerateField = true, Name = "Сумма с накл.")]
    [DisplayFormat(DataFormatString = "n2")]
    [ReadOnly(true)]
    public decimal SummaWithNaklad => PriceWithNaklad * Remain;

    [Display(AutoGenerateField = true, Name = "Посл.опер.")]
    [ReadOnly(true)]
    public DateTime LastOperationDate { set; get; }
}

/// <summary>
///     остатки товаров на складе.
/// </summary>
public class NomenklStoreRemainItem
{
    public decimal NomenklDC { set; get; }
    public decimal NomCurrencyDC { set; get; }
    public string NomenklName { set; get; }
    public decimal StoreDC { set; get; }
    public string StoreName { set; get; }
    public decimal Prihod { set; get; }
    public decimal Rashod { set; get; }
    public decimal Remain { set; get; }
    public decimal PriceWithNaklad { set; get; }
    public decimal Price { set; get; }
    public decimal Summa { set; get; }
    public decimal SummaWithNaklad { set; get; }
    public DateTime LastOperationDate { set; get; }
}

public class NomenklCalcMove
{
    public decimal NomDC { set; get; }
    public decimal StoreDC { set; get; }
    public string StoreName { set; get; }
    public string NomNomenkl { set; get; }
    public string NomName { set; get; }
    public DateTime Date { set; get; }
    public decimal Start { set; get; }
    public decimal Prihod { set; get; }
    public decimal Rashod { set; get; }
    public decimal Ostatok { set; get; }
    public decimal Price { set; get; }
    public decimal PriceWithNaklad { set; get; }
    public decimal MoneyPrihod { set; get; }
    public decimal MoneyPrihodWithNaklad { set; get; }
    public decimal MoneyRashod { set; get; }
    public decimal MoneyRashodWithNaklad { set; get; }
    public decimal StartPrice { set; get; }
    public decimal StartPriceWithNaklad { set; get; }
}

public class NomenklStoreLocationItem
{
    [Display(AutoGenerateField = false, Name = "Место хранения Id")]
    [ReadOnly(true)]
    public Guid StorageLocationId { set; get; }
    [Display(AutoGenerateField = true, Name = "Место хранения")]
    [ReadOnly(true)]
    public StorageLocationsWrapper StorageLocations { set; get; }
    [Display(AutoGenerateField = true, Name = "Склад")]
    [CanBeNull] public Warehouse Warehouse => StorageLocations?.Warehouse;
    [Display(AutoGenerateField = false, Name = "Номенклатура DC")]
    [ReadOnly(true)]
    public decimal NomenklDC { set; get; }
    [Display(AutoGenerateField = true, Name = "Номенклатура")]
    [ReadOnly(true)]
    public Nomenkl Nomenkl { set; get; }
    [Display(AutoGenerateField = true, Name = "Ном.№")]
    public string NomenklNumber => Nomenkl?.NomenklNumber;
    [Display(AutoGenerateField = true, Name = "Кол-во")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Quantity { set; get; }
    [Display(AutoGenerateField = true, Name = "Сумма")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Summa { set; get; }
    [Display(AutoGenerateField = true, Name = "Цена")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Price => Quantity == 0 ? 0 : Summa / Quantity;
    [Display(AutoGenerateField = true, Name = "Валюта")]
    public Currency Currency => Nomenkl?.Currency as Currency;
}
