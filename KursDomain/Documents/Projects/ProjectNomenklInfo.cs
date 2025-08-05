using System.ComponentModel.DataAnnotations;

namespace KursDomain.Documents.Projects;

public class ProjectNomenklInfo
{
    [Display(AutoGenerateField = true, Name = "Ном.№", Order = 1)]
    public string NomenklNumber { set; get; }
    [Display(AutoGenerateField = true, Name = "Наименование", Order = 2)]
    public string NomenklName { set; get; }
    [Display(AutoGenerateField = true, Name = "Ед.изм.",Order = 4)]
    public string Unit { set; get; }
    [Display(AutoGenerateField = true, Name = "Кол-во",Order = 3)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Quantity { set; get; }
    [Display(AutoGenerateField = true, Name = "Цена", Order = 5)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal UnitPrice { set; get; }
    [Display(AutoGenerateField = true, Name = "Сумма",Order = 6)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Summa { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание",Order = 7)]
    public string Note { set; get; }
 
}

public class ProjectInvoiceNomenklInfo
{
    [Display(AutoGenerateField = true, Name = "Ном.№", Order = 1)]
    public string NomenklNumber { set; get; }
    [Display(AutoGenerateField = true, Name = "Наименование", Order = 2)]
    public string NomenklName { set; get; }
    [Display(AutoGenerateField = true, Name = "Ед.изм.",Order = 4)]
    public string Unit { set; get; }
    [Display(AutoGenerateField = true, Name = "Кол-во",Order = 3)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Quantity { set; get; }
    [Display(AutoGenerateField = true, Name = "Цена", Order = 5)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal UnitPrice { set; get; }
    [Display(AutoGenerateField = true, Name = "Сумма",Order = 6)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Summa { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание",Order = 7)]
    public string Note { set; get; }

    [Display(AutoGenerateField = true, Name = "Услуга",Order = 8)]
    public bool IsUsluga { set; get; }

    [Display(AutoGenerateField = true, Name = "Отгружено",Order = 9)]
    public decimal Shipped { set; get; }
    [Display(AutoGenerateField = true, Name = "Сумма отгрузки",Order = 9)]
    public decimal? ShippedSumma { set; get; }

 
}

