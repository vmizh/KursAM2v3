using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.References;

namespace KursDomain.IDocuments.Finance;

public interface IInvoiceClientRow
{
    [Display(AutoGenerateField = false)] public decimal DocCode { set; get; }
    [Display(AutoGenerateField = false)] public int Code { set; get; }
    [Display(AutoGenerateField = false)] public Guid Id { set; get; }
    [Display(AutoGenerateField = false)] public Guid DocId { set; get; }

    [Display(AutoGenerateField = true, Name = "Номенклатура", Order = 2), ReadOnly(true)]
    public Nomenkl Nomenkl { set; get; }

    [Display(AutoGenerateField = true, Name = "Ном.№", Order = 1), ReadOnly(true)]
    public string NomNomenkl { set; get; }

    [Display(AutoGenerateField = true, Name = "Ед.Изм", Order = 3), ReadOnly(true)]
    Unit Unit { set; get; }

    [Display(AutoGenerateField = true, Name = "Услуга", Order = 10), ReadOnly(true)]
    public bool IsUsluga { set; get; }

    [Display(AutoGenerateField = true, Name = "Кол-во", Order = 4)]
    [DisplayFormat(DataFormatString = "n4")]
    public decimal Quantity { set; get; }

    [Display(AutoGenerateField = true, Name = "Цена", Order = 5)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Price { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма", Order = 6), ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Summa { set; get; }

    [Display(AutoGenerateField = true, Name = "Наценка дилера на единицу", Order = 11)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal? SFT_NACENKA_DILERA { set; get; }

    [Display(AutoGenerateField = true, Name = "Отгружено", Order = 12)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Shipped { set; get; }

    [Display(AutoGenerateField = true, Name = "Остаток", Order = 13), ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Rest { set; get; }

    [Display(AutoGenerateField = true, Name = "Текущий остаток", Order = 14), ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal CurrentRemains { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание", Order = 7)]
    public string Note { set; get; }

    [Display(AutoGenerateField = true, Name = "НДС %", Order = 8)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal NDSPercent { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма НДС", Order = 9)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal? SFT_SUMMA_NDS { set; get; }

    [Display(AutoGenerateField = true, Name = "Счет дох./расх.", Order = 15)]
    public SDRSchet SDRSchet { set; get; }

    [Display(AutoGenerateField = true, Name = "Груз. декларация", Order = 16)]
    public string GruzoDeclaration { set; get; }


}
