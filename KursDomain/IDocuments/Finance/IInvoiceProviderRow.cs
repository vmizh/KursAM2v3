using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KursDomain.References;

namespace KursDomain.IDocuments.Finance;

public interface IInvoiceProviderRow
{
    [Display(AutoGenerateField = false)] decimal DocCode { set; get; }

    [Display(AutoGenerateField = false)] int Code { set; get; }

    [Display(AutoGenerateField = false)] Guid Id { set; get; }

    [Display(AutoGenerateField = false)] Guid DocId { set; get; }

    string Note { set; get; }

    [Display(AutoGenerateField = false)] Unit PostUnit { set; get; }

    [Display(AutoGenerateField = false)] Unit UchUnit { set; get; }

    [Display(AutoGenerateField = false)] decimal SFT_POST_KOL { set; get; }

    [Display(AutoGenerateField = true, Name = "Номенклатура"), ReadOnly(true)]
    Nomenkl Nomenkl { set; get; }

    [Display(AutoGenerateField = true, Name = "Ном.№"), ReadOnly(true)]
    string NomenklNumber { set; get; }

    [Display(AutoGenerateField = true, Name = "Ед.Изм"), ReadOnly(true)]
    Unit Unit { set; get; }

    [Display(AutoGenerateField = true, Name = "Цена")]
    decimal Price { set; get; }

    [Display(AutoGenerateField = true, Name = "Кол-во")]
    decimal Quantity { set; get; }

    [Display(AutoGenerateField = true, Name = "% НДС")]
    decimal NDSPercent { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма накл."), ReadOnly(true)]
    decimal? SummaNaklad { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма НДС")]
    decimal? NDSSumma { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма"), ReadOnly(true)]
    decimal Summa { set; get; }

    [Display(AutoGenerateField = true, Name = "Услуга"), ReadOnly(true)]
    bool IsUsluga { get; }

    [Display(AutoGenerateField = true, Name = "Накл.расход"), ReadOnly(true)]
    bool IsNaklad { get; }

    [Display(AutoGenerateField = false)] bool IsIncludeInPrice { set; get; }

    //[Display(AutoGenerateField = false)] decimal? SFT_SUMMA_K_OPLATE_KONTR_CRS { set; get; }

    [Display(AutoGenerateField = true, Name = "Счет дох/расх")]
    SDRSchet SDRSchet { set; get; }
}
