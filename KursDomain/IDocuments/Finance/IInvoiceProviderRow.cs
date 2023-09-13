using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursDomain.IDocuments.Finance;

public interface IInvoiceProviderRow : IRowDC, IRowId
{
    [Display(AutoGenerateField = false)] Unit PostUnit { set; get; }
    [Display(AutoGenerateField = false)] Unit UchUnit { set; get; }
    [Display(AutoGenerateField = false)] decimal SFT_POST_KOL { set; get; }

    [Display(AutoGenerateField = true, Name = "Ном.№", Order = 1)]
    [ReadOnly(true)]
    string NomenklNumber { set; get; }

    [Display(AutoGenerateField = true, Name = "Номенклатура", Order = 2)]
    [ReadOnly(true)]
    Nomenkl Nomenkl { set; get; }

    [Display(AutoGenerateField = true, Name = "Кол-во", Order = 3)]
    [DisplayFormat(DataFormatString = "n2")]
    decimal Quantity { set; get; }

    [Display(AutoGenerateField = true, Name = "Ед.Изм", Order = 4)]
    [ReadOnly(true)]
    Unit Unit { set; get; }

    [Display(AutoGenerateField = true, Name = "Цена", Order = 5)]
    [DisplayFormat(DataFormatString = "n2")]
    decimal Price { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма", Order = 6)]
    [ReadOnly(true)]
    decimal Summa { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма накл.", Order = 7)]
    [ReadOnly(true)]
    decimal? SummaNaklad { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    string Note { set; get; }


    [Display(AutoGenerateField = true, Name = "% НДС")]
    decimal NDSPercent { set; get; }


    [Display(AutoGenerateField = true, Name = "Сумма НДС")]
    decimal? NDSSumma { set; get; }


    [Display(AutoGenerateField = true, Name = "Услуга")]
    [ReadOnly(true)]
    bool IsUsluga { get; }

    [Display(AutoGenerateField = true, Name = "Накл.расход")]
    [ReadOnly(true)]
    bool IsNaklad { get; }

    [Display(AutoGenerateField = false)] bool IsIncludeInPrice { set; get; }


    [Display(AutoGenerateField = true, Name = "Счет дох/расх")]
    SDRSchet SDRSchet { set; get; }
}
