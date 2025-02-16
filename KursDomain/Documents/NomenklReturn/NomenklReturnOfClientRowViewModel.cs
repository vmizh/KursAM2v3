using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;
using KursDomain.ICommon;
using KursDomain.IDocuments.NomenklReturn;
using KursDomain.References;

namespace KursDomain.Documents.NomenklReturn;

public sealed class NomenklReturnOfClientRowViewModel : RSViewModelBase, IEntity<NomenklReturnOfClientRow>,
    INomenklReturnOfClientRow
{
    private string myInvoiceText;
    private Nomenkl myNomenkl;
    private string myWaybillText;
    private decimal myCalcCost;

    public NomenklReturnOfClientRowViewModel()
    {
        Entity = DefaultValue();
    }

    public NomenklReturnOfClientRowViewModel(NomenklReturnOfClientRow entity, NomenklReturnOfClientViewModel parent)
    {
        Parent = parent;
        Entity = entity;
        LoadReferences();
    }

    public void LoadReferences()
    {
        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(Entity.NomenklDC) as Nomenkl;
    }

    public override object ToJson()
    {
        return new
        {
            Id,
            Номенклатурный_номер = NomenklNumber,
            Номенклатура = Nomenkl.Name,
            Количество = Quantity.ToString("n2"),
            Единица_измерения = ((IName)Nomenkl.Unit).Name,
            СуммаКлиент = SummaClient,
            СуммаСклад = SummaWarehouse,
            Примечание = Note
           
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

    [Display(AutoGenerateField = true, Name = "Номенклатура")]
    [ReadOnly(true)]
    public Nomenkl Nomenkl
    {
        get => myNomenkl;
        set
        {
            if (myNomenkl == value) return;
            myNomenkl = value;
            Entity.NomenklDC = myNomenkl?.DocCode ?? default;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Ном.№")]
    public string NomenklNumber => Nomenkl?.NomenklNumber;

    [Display(AutoGenerateField = true, Name = "Валюта")]
    public References.Currency Currency => Nomenkl?.Currency as References.Currency;

    [Display(AutoGenerateField = true, Name = "С/фактура")]
    public string InvoiceText

    {
        get => myInvoiceText;
        set
        {
            if (myInvoiceText == value) return;
            myInvoiceText = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Расх.накл")]
    public string WaybillText

    {
        get => myWaybillText;
        set
        {
            if (myWaybillText == value) return;
            myWaybillText = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Сумма клиенту")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaClient => Price * Quantity;

    [Display(AutoGenerateField = true, Name = "Сумма склад")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaWarehouse => Cost * Quantity;

    [Display(AutoGenerateField = true, Name = "Расч.себест.")]
    [DisplayFormat(DataFormatString = "n2")]
    [ReadOnly(true)]
    public decimal CalcCost
    {
        get => myCalcCost;
        set
        {
            if (myCalcCost == value) return;
            myCalcCost = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)] public NomenklReturnOfClientRow Entity { get; set; }

    public NomenklReturnOfClientRow DefaultValue()
    {
        return new NomenklReturnOfClientRow
        {
            Id = Guid.NewGuid(),
            DocId = ((NomenklReturnOfClientViewModel)Parent)?.Id ?? Guid.Empty
        };
    }

    [Display(AutoGenerateField = false)]
    public Guid DocId
    {
        get => Entity.DocId;
        set
        {
            if (Entity.DocId == value) return;
            Entity.DocId = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    public decimal NomenklDC
    {
        get => Entity.NomenklDC;
        set
        {
            if (Entity.NomenklDC == value) return;
            Entity.NomenklDC = value;
            myNomenkl = GlobalOptions.ReferencesCache.GetNomenkl(Entity.NomenklDC) as Nomenkl;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Nomenkl));
        }
    }


    [Display(AutoGenerateField = false)]
    public Guid? InvoiceRowId
    {
        get => Entity.InvoiceRowId;
        set
        {
            if (Entity.InvoiceRowId == value) return;
            Entity.InvoiceRowId = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Кол-во")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Quantity
    {
        get => Entity.Quantity;
        set
        {
            if (Entity.Quantity == value) return;
            Entity.Quantity = value;
            UpdateParent();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(SummaClient));
            RaisePropertyChanged(nameof(SummaWarehouse));
        }
    }

    [Display(AutoGenerateField = true, Name = "Цена клиенту")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Price
    {
        get => Entity.Price;
        set
        {
            if (Entity.Price == value) return;
            Entity.Price = value;
            UpdateParent();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(SummaClient));
        }
    }


    [Display(AutoGenerateField = true, Name = "Цена склада")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Cost
    {
        get => Entity.Cost;
        set
        {
            if (Entity.Cost == value) return;
            Entity.Cost = value;
            UpdateParent();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(SummaWarehouse));
        }
    }

    private void UpdateParent()
    {
        switch (Parent)
        {
            case null:
                return;
            case NomenklReturnOfClientViewModel p:
                p.RaisePropertyChanged(nameof(NomenklReturnOfClientViewModel.SummaWarehouse));
                p.RaisePropertyChanged(nameof(NomenklReturnOfClientViewModel.SummaClient));
                break;
        }
    }
}
