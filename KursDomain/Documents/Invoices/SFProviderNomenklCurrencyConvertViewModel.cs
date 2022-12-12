using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Core.ViewModel.Base;
using Data;
using KursDomain.ICommon;
using KursDomain.IDocuments.DistributeOverhead;
using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.Documents.Invoices;

[DebuggerDisplay("{DocCode,nq}/{Code} {Description,nq}")]
public class SFProviderNomenklCurrencyConvert : ISFProviderNomenklCurrencyConvert,
    ILoadFromEntity<TD_26_CurrencyConvert>
{
    
    public SFProviderNomenklCurrencyConvert()
    {
    }

    public SFProviderNomenklCurrencyConvert(Guid id, INomenkl nomenklTo, INomenkl nomenklFrom, DateTime date,
        decimal quantity, decimal rate, decimal price, decimal priceWithNaklad, string note, IWarehouse warehouse)
    {
        Id = id;
        NomenklTo = nomenklTo;
        NomenklFrom = nomenklFrom;
        Date = date;
        Quantity = quantity;
        Rate = rate;
        Price = price;
        PriceWithNaklad = priceWithNaklad;
        Notes = note;
        Warehouse = warehouse;
    }

    [Display(AutoGenerateField = true, Name = "Ном.№ в")]
    public string NomenklNumberTo => NomenklTo?.NomenklNumber;

    [Display(AutoGenerateField = true, Name = "Валюта из")]
    public References.Currency FromCurrency => NomenklFrom?.Currency as References.Currency;

    [Display(AutoGenerateField = true, Name = "Валюта в")]
    public References.Currency ToCurrency => NomenklTo?.Currency as References.Currency;

    [Display(AutoGenerateField = true, Name = "Ном.№ из")]
    public string NomenklNumberFrom => NomenklFrom?.NomenklNumber;

    [Display(AutoGenerateField = true, Name = "Кол-во на складе")]
    public decimal MaxQuantityOnWarehouse { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public string Notes { get; set; }

    public void LoadFromEntity(TD_26_CurrencyConvert entity, IReferencesCache cache)
    {
        Id = entity.Id;
        NomenklTo = cache.GetNomenkl(entity.NomenklId);
        Date = entity.Date;
        Quantity = entity.Quantity;
        Rate = entity.Rate;
        Price = entity.Price;
        PriceWithNaklad = entity.PriceWithNaklad;
        Notes = entity.Note;
        Warehouse = cache.GetWarehouse(entity.StoreDC);
    }

    [Display(AutoGenerateField = false)] public Guid Id { get; set; }

    [Display(AutoGenerateField = true, Name = "Номенклатура из")]
    public INomenkl NomenklTo { get; set; }

    [Display(AutoGenerateField = true, Name = "Номенклатура в")]
    public INomenkl NomenklFrom { get; set; }

    [Display(AutoGenerateField = true, Name = "Дата конвертации")]
    public DateTime Date { get; set; }

    [Display(AutoGenerateField = true, Name = "Кол-во")]
    public decimal Quantity { get; set; }

    [Display(AutoGenerateField = true, Name = "Курс")]
    public decimal Rate { get; set; }

    public decimal FromPrice { get; set; }
    public decimal FromPriceWithNaklad { get; set; }

    [Display(AutoGenerateField = true, Name = "Цена")]
    public decimal Price { get; set; }

    [Display(AutoGenerateField = true, Name = "Цена (с накл.)")]
    public decimal PriceWithNaklad { get; set; }

    [Display(AutoGenerateField = true, Name = "Сумма")]
    public decimal Summa => Price * Quantity;

    [Display(AutoGenerateField = true, Name = "Сумма (с накл.)-из")]
    public decimal FromSummaWithNaklad => FromPriceWithNaklad * Quantity;

    [Display(AutoGenerateField = true, Name = "Сумма - из")]
    public decimal FromSumma => FromPrice * Quantity;

    [Display(AutoGenerateField = true, Name = "Сумма (с накл.)")]
    public decimal SummaWithNaklad => PriceWithNaklad * Quantity;

    [Display(AutoGenerateField = true, Name = "Склад")]
    public IWarehouse Warehouse { get; set; }

    public IEnumerable<IDistributeNakladRow> NakladRows { get; set; } = new List<IDistributeNakladRow>();

    public string Description =>
        $"Вал. конв. товара {NomenklFrom}({NomenklNumberFrom})" +
        $" {FromPrice:n2} {FromCurrency} в {NomenklTo}({NomenklNumberTo}) {Price:n2} {ToCurrency}";

    public IInvoiceProviderRow InvoiceProviderRow { get; set; }

    public override string ToString()
    {
        return Description;
    }
}

[DebuggerDisplay("{DocCode,nq}/{Code} {Description,nq}")]
public class SFProviderNomenklCurrencyConvertViewModel : RSViewModelBase, ISFProviderNomenklCurrencyConvert
{
    #region Constructors

    public SFProviderNomenklCurrencyConvertViewModel(TD_26_CurrencyConvert entity, IReferencesCache cache)
    {
        Entity = entity;
        Cache = cache;
    }

    #endregion

    public string NomenklFromNomenklNumber => NomenklFrom?.NomenklNumber;
    public string NomenklToNomenklNumber => NomenklTo?.NomenklNumber;
    public References.Currency NomenklFromCurrency => NomenklFrom?.Currency as References.Currency;
    public References.Currency NomenklToCurrency => NomenklTo?.Currency as References.Currency;

    public override string Note
    {
        get => _note;
        set
        {
            if (Equals(_note, value)) return;
            _note = value;
            RaisePropertyChanged();
        }
    }

    public override Guid Id
    {
        get => Entity.Id;
        set
        {
            if (Equals(Entity.Id, value)) return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
    }

    public INomenkl NomenklTo
    {
        get => Cache.GetNomenkl(Entity.NomenklId);
        set
        {
            if (Equals(Cache.GetNomenkl(Entity.NomenklId), value)) return;
            Entity.NomenklId = ((IDocGuid)value).Id;
            RaisePropertyChanged();
        }
    }

    public INomenkl NomenklFrom
    {
        get => _nomenklFrom;
        set
        {
            if (Equals(_nomenklFrom, value)) return;
            _nomenklFrom = value;
            RaisePropertyChanged();
        }
    }

    public DateTime Date
    {
        get => Entity.Date;
        set
        {
            if (Equals(Entity.Date, value)) return;
            Entity.Date = value;
            RaisePropertyChanged();
        }
    }

    public decimal Quantity
    {
        get => Entity.Quantity;
        set
        {
            if (Equals(Entity.Quantity, value)) return;
            Entity.Quantity = value;
            RaisePropertyChanged();
        }
    }

    public decimal Rate
    {
        get => Entity.Rate;
        set
        {
            if (Equals(Entity.Rate, value)) return;
            Entity.Rate = value;
            RaisePropertyChanged();
        }
    }

    public decimal FromPrice
    {
        get => _fromPrice;
        set
        {
            if (Equals(_fromPrice, value)) return;
            _fromPrice = value;
            RaisePropertyChanged();
        }
    }

    public decimal FromPriceWithNaklad
    {
        get => _fromPriceWithNaklad;
        set
        {
            if (Equals(_fromPriceWithNaklad, value)) return;
            _fromPriceWithNaklad = value;
            RaisePropertyChanged();
        }
    }

    public decimal Price
    {
        get => Entity.Price;
        set
        {
            if (Equals(Entity.Price, value)) return;
            Entity.Price = value;
            RaisePropertyChanged();
        }
    }

    public decimal PriceWithNaklad
    {
        get => Entity.PriceWithNaklad;
        set
        {
            if (Equals(Entity.PriceWithNaklad, value)) return;
            Entity.PriceWithNaklad = value;
            RaisePropertyChanged();
        }
    }

    public decimal FromSumma => FromPrice * Quantity;
    public decimal FromSummaWithNaklad => FromPriceWithNaklad * Quantity;

    public decimal Summa
    {
        get => _summa;
        set
        {
            if (Equals(_summa, value)) return;
            _summa = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaWithNaklad
    {
        get => _summaWithNaklad;
        set
        {
            if (Equals(_summaWithNaklad, value)) return;
            _summaWithNaklad = value;
            RaisePropertyChanged();
        }
    }

    public decimal SFDocCode
    {
        get => _sfDocCode;
        set
        {
            if (Equals(_sfDocCode, value)) return;
            _sfDocCode = value;
            RaisePropertyChanged();
        }
    }

    public int SFCode
    {
        get => _sfCode;
        set
        {
            if (Equals(_sfCode, value)) return;
            _sfCode = value;
            RaisePropertyChanged();
        }
    }

    public string NomenklNumberFrom => NomenklFrom?.NomenklNumber;
    public References.Currency FromCurrency  => NomenklFrom?.Currency as References.Currency;
    public string NomenklNumberTo => NomenklTo?.NomenklNumber;
    public References.Currency ToCurrency => NomenklTo?.Currency as References.Currency;

    public override string Description =>  $"Вал. конв. товара {NomenklFrom}({NomenklNumberFrom})" +
                                           $" {FromPrice:n2} {FromCurrency} в {NomenklTo}({NomenklNumberTo}) {Price:n2} {ToCurrency}";

    public IInvoiceProviderRow InvoiceProviderRow { get; set; }

    public IWarehouse Warehouse
    {
        get => _warehouse;
        set
        {
            if (Equals(_warehouse, value)) return;
            _warehouse = value;
            RaisePropertyChanged();
        }
    }

    public IEnumerable<IDistributeNakladRow> NakladRows { get; set; } = new List<IDistributeNakladRow>();

    //TODO Реализовать метод показа курса на экране
    private decimal ShowRate()
    {
        return Math.Round(Entity.Rate, 4);
    }

    #region fields

    private readonly TD_26_CurrencyConvert Entity;
    private readonly IReferencesCache Cache;
    private INomenkl _nomenklFrom;
    private decimal _fromPrice;
    private decimal _fromPriceWithNaklad;
    private decimal _summa;
    private decimal _summaWithNaklad;
    private string _note;
    private decimal _sfDocCode;
    private int _sfCode;
    private IWarehouse _warehouse;

    #endregion
}
