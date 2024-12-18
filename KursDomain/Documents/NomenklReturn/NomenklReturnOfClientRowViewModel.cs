using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;
using KursDomain.IDocuments.NomenklReturn;
using KursDomain.References;

namespace KursDomain.Documents.NomenklReturn;

public sealed class NomenklReturnOfClientRowViewModel : RSViewModelBase, IEntity<NomenklReturnOfClientRow>,
    INomenklReturnOfClientRow
{
    private string myInvoiceText;
    private Nomenkl myNomenkl;

    public NomenklReturnOfClientRowViewModel()
    {
        Entity = DefaultValue();
    }

    public NomenklReturnOfClientRowViewModel(NomenklReturnOfClientRow entity, NomenklReturnOfClientViewModel parent)
    {
        Parent = parent;
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
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Ном.№")]
    public string NomenklNumber => Nomenkl?.NomenklNumber;

    [Display(AutoGenerateField = true, Name = "Валюта")]
    public References.Currency Currency => Nomenkl?.Currency as References.Currency;

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
    [Display(AutoGenerateField = false)]
    public NomenklReturnOfClientRow Entity { get; set; }

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
    public decimal Quantity {
        get => Entity.Quantity;
        set
        {
            if (Entity.Quantity == value) return;
            Entity.Quantity = value;
            RaisePropertyChanged();
        }
    }
    [Display(AutoGenerateField = true, Name = "Цена клиенту")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Price {
        get => Entity.Price;
        set
        {
            if (Entity.Price == value) return;
            Entity.Price = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Сумма клиенту")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaClient => Price * Quantity;


    [Display(AutoGenerateField = true, Name = "Цена склада")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Cost {
        get => Entity.Cost;
        set
        {
            if (Entity.Cost == value) return;
            Entity.Cost = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Сумма склад")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaWarehouse => Cost * Quantity;
}
