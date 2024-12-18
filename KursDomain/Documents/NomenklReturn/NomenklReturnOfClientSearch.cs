using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Data;
using KursDomain.IDocuments.NomenklReturn;
using KursDomain.References;

namespace KursDomain.Documents.NomenklReturn;

public class NomenklReturnOfClientSearch : INomenklReturnOfClient
{
    [Display(AutoGenerateField = true, Name = "Контрагент")]
    public Kontragent Kontragent => GlobalOptions.ReferencesCache.GetKontragent(KontragentDC) as Kontragent;

    [Display(AutoGenerateField = false)] public Guid Id { get; set; }

    [Display(AutoGenerateField = true, Name = "№")]
    [ReadOnly(true)]
    public int DocNum { get; set; }

    [Display(AutoGenerateField = true, Name = "Внешн.№")]
    [ReadOnly(true)]
    public string DocExtNum { get; set; }

    [Display(AutoGenerateField = true, Name = "Дата")]
    [ReadOnly(true)]
    public DateTime DocDate { get; set; }

    [Display(AutoGenerateField = false)] public decimal KontragentDC { get; set; }

    [Display(AutoGenerateField = false)] public decimal? InvoiceClientDC { get; set; }

    [Display(AutoGenerateField = true, Name = "Валюта")]
    public References.Currency Currency => Kontragent?.Currency as References.Currency;

    [Display(AutoGenerateField = true, Name = "Сумма клиента")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaClient { get; set; }

    [Display(AutoGenerateField = true, Name = "Сумма склад")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaWarehouse { get; set; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public string Note { get; set; }

    [Display(AutoGenerateField = false)] public ObservableCollection<INomenklReturnOfClientRow> Rows { get; set; }

    [Display(AutoGenerateField = false)] public NomenklReturnOfClient Entity { get; set; }
}
