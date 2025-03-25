using Core.ViewModel.Base;
using Data;
using KursDomain.IDocuments.NomenklReturn;
using KursDomain.References;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using System.Linq;

namespace KursDomain.Documents.NomenklReturn;

public class NomenklReturnToProviderViewModel : RSViewModelBase, IEntity<NomenklReturnToProvider>, INomenklReturnToProvider
{
    private Kontragent myKontragent;
    private References.Warehouse myWarehouse;

    #region Methods

    public void LoadReferences()
    {
        if (Entity?.KontregentDC is not null)
            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(Entity.KontregentDC) as Kontragent;
        if (Entity?.WarehouseDC is not null)
            Warehouse = GlobalOptions.ReferencesCache.GetWarehouse(Entity.WarehouseDC) as References.Warehouse;
        if (Entity?.NomenklReturnToProviderRow is not null && Entity.NomenklReturnToProviderRow.Count > 0)
            foreach (var row in Entity.NomenklReturnToProviderRow)
                Rows.Add(new NomenklReturnToProviderRowViewModel(row, this));
    }

    public override object ToJson()
    {
        var res = new
        {
            DocCode,
            Номер = DocNum + "/" + DocExtNum,
            Дата = DocDate.ToShortDateString(),
            Контрагент = Kontragent.Name,
            Склад = Warehouse.Name,
            СуммаСклад = SummaWarehouse,
            СуммаКлиент = SummaClient,
            Примечание = Note,
            Позиции = Rows.Select(_ => _.ToJson())
        };
        return JsonConvert.SerializeObject(res);
    }

    #endregion

    #region Constructors

    public NomenklReturnToProviderViewModel()
    {
        Entity = DefaultValue();
    }

    public NomenklReturnToProviderViewModel(NomenklReturnToProvider entity)
    {
        Entity = entity ?? DefaultValue();
        LoadReferences();
    }

    #endregion

    #region Properties

    [Display(AutoGenerateField = true, Name = "Контрагент")]
    public Kontragent Kontragent
    {
        get => myKontragent;
        set
        {
            if (myKontragent == value) return;
            myKontragent = value;
            if (myKontragent is not null)
                Entity.KontregentDC = Kontragent.DocCode;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Currency));
        }
    }

    [Display(AutoGenerateField = true, Name = "Склад")]
    public References.Warehouse Warehouse
    {
        get => myWarehouse;
        set
        {
            if (myWarehouse == value) return;
            myWarehouse = value;
            if (myWarehouse is not null)
                Entity.WarehouseDC = myWarehouse.DocCode;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)] public NomenklReturnToProvider Entity { get; set; }

    public NomenklReturnToProvider DefaultValue()
    {
        return new NomenklReturnToProvider
        {
            Id = Guid.NewGuid(),
            DocDate = DateTime.Today
        };
    }

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = true, Name = "№")]
    [ReadOnly(true)]
    public int DocNum
    {
        get => Entity.DocNum;
        set
        {
            if (Entity.DocNum == value) return;
            Entity.DocNum = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Внешн.№")]
    public string DocExtNum
    {
        get => Entity.DocExtNum;
        set
        {
            if (Entity.DocExtNum == value) return;
            Entity.DocExtNum = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Дата")]
    [ReadOnly(true)]
    public DateTime DocDate
    {
        get => Entity.DocDate;
        set
        {
            if (Entity.DocDate == value) return;
            Entity.DocDate = value;
            RaisePropertyChanged();
        }
    }


    [Display(AutoGenerateField = true, Name = "Валюта")]
    public References.Currency Currency => Kontragent?.Currency as References.Currency;

    [Display(AutoGenerateField = true, Name = "Сумма клиента")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaClient
    {
        get => Rows is not null && Rows.Count > 0 ? Rows.Sum(_ => _.Price * _.Quantity) : 0;
        set => throw new NotImplementedException();
    }

    [Display(AutoGenerateField = false)]
    public decimal? PrihOrderDC { get; set; }
    [Display(AutoGenerateField = false)]
    public decimal? InvoiceProviderDC { get; set; }

    [Display(AutoGenerateField = true, Name = "Сумма склад")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaWarehouse
    {
        get => Rows is not null && Rows.Count > 0 ? Rows.Sum(_ => (_.Cost ?? 0) * _.Quantity) : 0;
        set => throw new NotImplementedException();
    }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public override string Note
    {
        get => Entity.Note;
        set
        {
            if (Entity.Note == value) return;
            Entity.Note = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Создатель")]
    [ReadOnly(true)]
    public string Creator
    {
        get => Entity.Creator;
        set
        {
            if (Entity.Creator == value) return;
            Entity.Creator = value;
            RaisePropertyChanged();
        }
    }

    public override string Description
    {
        get
        {
            var extNum = string.IsNullOrWhiteSpace(DocExtNum) ? string.Empty : $"/{DocExtNum}";
            return
                $"Возврат товара от клиента №{DocNum}{extNum} от {DocDate.ToShortDateString()} Контр-т: {Kontragent} Склад: {Warehouse} на сумму {SummaClient:n2}/{SummaWarehouse:n2} {Kontragent?.Currency}";
        }
    }


    [Display(AutoGenerateField = false)]
    public ObservableCollection<NomenklReturnToProviderRowViewModel> Rows { get; set; } =
        new ObservableCollection<NomenklReturnToProviderRowViewModel>();

    #endregion
}

