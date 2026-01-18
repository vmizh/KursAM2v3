using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using Data;
using KursDomain.Base;
using KursDomain.ICommon;
using KursDomain.IDocuments.TransferOut;
using KursDomain.References;
using KursDomain.Wrapper.Base;
using Prism.Events;

namespace KursDomain.Wrapper.TransferOut;

public sealed class TransferOutBalansWrapper : BaseWrapper<TransferOutBalans>, ITransferOutBalans,
    IEquatable<TransferOutBalansWrapper>
{
    private int _PositionCount;
    private StorageLocationsWrapper _StorageLocation;

    public TransferOutBalansWrapper(TransferOutBalans model, IEventAggregator eventAggregator,
        IMessageDialogService messageDialogService) : base(model, eventAggregator, messageDialogService)
    {
    }

    [Display(AutoGenerateField = true, Name = "Кол-во поз.")]
    [ReadOnly(true)]
    public int PositionCount
    {
        get => _PositionCount;
        set
        {
            if (_PositionCount == value) return;
            _PositionCount = value;
            RaisePropertiesChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Сумма")]
    [DisplayFormat(DataFormatString = "n2")]
    [ReadOnly(true)]
    public decimal Summa => getSumma();

    public override string Description =>
        $"Перевод за баланс. №{DocNum} от {DocDate.ToShortDateString()} со склада {Warehouse?.Name}";

    public bool Equals(TransferOutBalansWrapper other)
    {
        if (other == null) return false;
        return Id == other.Id;
    }

    [Display(AutoGenerateField = false, Name = "Id")]
    public override Guid Id
    {
        get => GetValue<Guid>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Дата")]
    public DateTime DocDate
    {
        get => GetValue<DateTime>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "№")]
    [ReadOnly(true)]
    public int DocNum
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Создатель")]
    [ReadOnly(true)]
    public string Creator
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public override string Note
    {
        get => GetValue<string>();
        set => SetValue(value);
    }


    public ObservableCollection<CurrencyValue> CurrenciesSummaries { get; set; } =
        new ObservableCollection<CurrencyValue>();

    [Display(AutoGenerateField = true, Name = "Валюта")]
    public Currency Currency
    {
        get => GlobalOptions.ReferencesCache.GetCurrency(Model.CurrencyDC) as Currency;
        set
        {
            if (Model.CurrencyDC == (value?.DocCode ?? 0)) return;
            SetValue(value?.DocCode ?? 0, nameof(TransferOutBalans.CurrencyDC));
        }
    }

    [Display(AutoGenerateField = true, Name = "Склад")]
    public Warehouse Warehouse
    {
        get => GlobalOptions.ReferencesCache.GetWarehouse(Model.WarehouseDC) as Warehouse;
        set
        {
            if (Model.WarehouseDC == (value?.DocCode ?? 0)) return;
            SetValue(value?.DocCode ?? 0, nameof(TransferOutBalans.WarehouseDC));
        }
    }

    [Display(AutoGenerateField = false, Name = "Строки")]
    public ICollection<TransferOutBalansRowsWrapper> Rows { get; set; } =
        new ObservableCollection<TransferOutBalansRowsWrapper>();

    [Display(AutoGenerateField = true, Name = "Место хранения")]
    public StorageLocationsWrapper StorageLocation
    {
        get => _StorageLocation;
        set
        {
            if (_StorageLocation == value) return;
            _StorageLocation = value;
            SetValue(_StorageLocation?.Id, nameof(Model.StorageLocationId));
            RaisePropertyChanged();
        }
    }

    private decimal getSumma()
    {
         return Math.Round(Rows.Sum(_ => _.Summa), 2);
      
    }

    public override void StartLoad(bool isFullLoad = true)
    {
        if (Model.StorageLocations != null)
            StorageLocation =
                new StorageLocationsWrapper(Model.StorageLocations, EventAggregator, MessageDialogService);

        if (isFullLoad && Model.TransferOutBalansRows is { Count: > 0 })
            foreach (var row in Model.TransferOutBalansRows)
            {
                var newRow = new TransferOutBalansRowsWrapper(row, EventAggregator,MessageDialogService);
                newRow.Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(row.NomenklDC) as References.Nomenkl;
                newRow.PropertyChanged += (o, e) => { UpdateSummaries(); };
                Rows.Add(newRow);
            }

        UpdateSummaries();
    }

    public void UpdateSummaries()
    {
        PositionCount = Rows.Count;
        CurrenciesSummaries.Clear();
        var curs = Rows.Select(_ => _.Currency).Distinct().ToList();
        foreach (var crs in curs)
            CurrenciesSummaries.Add(new CurrencyValue
            {
                Currency = crs,
                Value = Math.Round(Rows.Where(_ => _.Currency == crs).Sum(_ => _.Summa), 2)
            });
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TransferOutBalansWrapper)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(TransferOutBalansWrapper left, TransferOutBalansWrapper right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TransferOutBalansWrapper left, TransferOutBalansWrapper right)
    {
        return !Equals(left, right);
    }

    public override object ToJson()
    {
        var res = new
        {
            Статус = State == RowStatus.NewRow ? "Новый" : "Изменен",
            Id,
            Номер = DocNum,
            Дата = DocDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
            Склад = Warehouse.Name,
            Место_хранения = StorageLocation.Name,
            Создатель = Creator,
            Примечание = Note,
            Позиции = Rows.Select(_ => _.ToJson())
        };

        return res;
    }
}
