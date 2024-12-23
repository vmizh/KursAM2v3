using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.ViewModel.Base;
using Data;
using KursDomain.IDocuments.NomenklReturn;
using KursDomain.References;

namespace KursDomain.Documents.NomenklReturn;

public class NomenklReturnOfClientViewModel : RSViewModelBase, IEntity<NomenklReturnOfClient>, INomenklReturnOfClient
{
    private string myInvoiceText;

    public NomenklReturnOfClientViewModel()
    {
        Entity = DefaultValue();
        Rows = new ObservableCollection<NomenklReturnOfClientRowViewModel>();
    }

    public NomenklReturnOfClientViewModel(NomenklReturnOfClient entity)
    {
        Entity = entity ?? DefaultValue();
        if (Entity.NomenklReturnOfClientRow is not null && Entity.NomenklReturnOfClientRow.Count > 0)
        {
            foreach (var row in Entity.NomenklReturnOfClientRow)
            {
                Rows.Add(new NomenklReturnOfClientRowViewModel(row,this));
            }
        }
    }

    [Display(AutoGenerateField = true, Name = "Контрагент")]
    public Kontragent Kontragent => GlobalOptions.ReferencesCache.GetKontragent(Entity.KontragentDC) as Kontragent;

    [Display(AutoGenerateField = true, Name = "Счет клиента")]
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

    [Display(AutoGenerateField = false)] public NomenklReturnOfClient Entity { get; set; }

    public NomenklReturnOfClient DefaultValue()
    {
        return new NomenklReturnOfClient
        {
            Id = Guid.NewGuid(),
            DocDate = DateTime.Today,
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

    [Display(AutoGenerateField = false)]
    public decimal KontragentDC
    {
        get => Entity.KontragentDC;
        set
        {
            if (Entity.KontragentDC == value) return;
            Entity.KontragentDC = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Kontragent));
            RaisePropertyChanged(nameof(Currency));
        }
    }

    [Display(AutoGenerateField = false)]
    public decimal? InvoiceClientDC
    {
        get => Entity.InvoiceClientDC;
        set
        {
            if (Entity.InvoiceClientDC == value) return;
            Entity.InvoiceClientDC = value;
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

    [Display(AutoGenerateField = true, Name = "Сумма склад")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaWarehouse
    {
        get => Rows is not null && Rows.Count > 0 ? Rows.Sum(_ => _.Cost * _.Quantity) : 0;
        set => throw new NotImplementedException();
    }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
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
    public string Creator {  get => Entity.Creator;
        set
        {
            if (Entity.Creator == value) return;
            Entity.Creator = value;
            RaisePropertyChanged();
        } }

    [Display(AutoGenerateField = false)]
    public ObservableCollection<NomenklReturnOfClientRowViewModel> Rows { get; set; } =
        new ObservableCollection<NomenklReturnOfClientRowViewModel>();
}
