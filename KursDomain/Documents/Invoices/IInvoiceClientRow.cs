using System;
using System.ComponentModel.DataAnnotations;
using KursDomain.References;

namespace KursDomain.Documents.Invoices;

public interface IInvoiceClientRow
{
    [Display(AutoGenerateField = false)] public decimal DocCode { set; get; }
    [Display(AutoGenerateField = false)] public int Code { set; get; }
    [Display(AutoGenerateField = false)] public Guid Id { set; get; }
    [Display(AutoGenerateField = false)] public Guid DocId { set; get; }

    [Display(AutoGenerateField = true, Name = "Номенклатура")]
    public Nomenkl Nomenkl { set; get; }

    [Display(AutoGenerateField = true, Name = "Ном.№")]
    public string NomNomenkl { set; get; }

    [Display(AutoGenerateField = true, Name = "Услуга")]
    public bool IsUsluga { set; get; }

    [Display(AutoGenerateField = true, Name = "Кол-во")]
    [DisplayFormat(DataFormatString = "n4")]
    public decimal Quantity { set; get; }

    [Display(AutoGenerateField = true, Name = "Цена")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Price { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Summa { set; get; }

    [Display(AutoGenerateField = true, Name = "Наценка дилера на единицу")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal? SFT_NACENKA_DILERA { set; get; }

    [Display(AutoGenerateField = true, Name = "Отгружено")]
    [DisplayFormat(DataFormatString = "n4")]
    public decimal Shipped { set; get; }

    [Display(AutoGenerateField = true, Name = "Остаток")]
    [DisplayFormat(DataFormatString = "n4")]
    public decimal Rest { set; get; }

    [Display(AutoGenerateField = true, Name = "Текущий остаток")]
    [DisplayFormat(DataFormatString = "n4")]
    public decimal CurrentRemains { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public string Note { set; get; }

    [Display(AutoGenerateField = true, Name = "НДС %")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal NDSPercent { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма НДС")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal? SFT_SUMMA_NDS { set; get; }

    [Display(AutoGenerateField = true, Name = "Счет дох./расх.")]
    public SDRSchet SDRSchet { set; get; }

    [Display(AutoGenerateField = true, Name = "Груз. декларация")]
    public string GruzoDeclaration { set; get; }


}
