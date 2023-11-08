using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursDomain.IDocuments.Finance;
// Счет фактура от поставщика
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

    [Display(AutoGenerateField = true, Name = "Кол-во", Order = 4)]
    [DisplayFormat(DataFormatString = "n2")]
    decimal Quantity { set; get; }

    [Display(AutoGenerateField = true, Name = "Ед.Изм", Order = 3)]
    [ReadOnly(true)]
    Unit Unit { set; get; }

    [Display(AutoGenerateField = true, Name = "Цена", Order = 5)]
    [DisplayFormat(DataFormatString = "n2")]
    decimal Price { set; get; }

    [Display(AutoGenerateField = true, Name = "Цена с НДС", Order = 6)]
    [DisplayFormat(DataFormatString = "n2")]
    decimal PriceWithNDS { get; }

    [Display(AutoGenerateField = true, Name = "Сумма", Order = 7)]
    [ReadOnly(true)]
    decimal Summa { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма накл.", Order = 8)]
    [ReadOnly(true)]
    decimal? SummaNaklad { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание", Order = 9)]
    [MaxLength(100)]
    string Note { set; get; }


    [Display(AutoGenerateField = true, Name = "% НДС", Order = 10)]
    decimal NDSPercent { set; get; }


    [Display(AutoGenerateField = true, Name = "Сумма НДС", Order = 11)]
    decimal? NDSSumma { set; get; }


    [Display(AutoGenerateField = true, Name = "Услуга", Order = 12)]
    [ReadOnly(true)]
    bool IsUsluga { get; }

    [Display(AutoGenerateField = true, Name = "Накл.расход", Order = 13)]
    [ReadOnly(true)]
    bool IsNaklad { get; }

    [Display(AutoGenerateField = false)] bool IsIncludeInPrice { set; get; }


    [Display(AutoGenerateField = true, Name = "Счет дох/расх", Order = 14)]
    SDRSchet SDRSchet { set; get; }

    [Display(AutoGenerateField = true, Name = "Получено", Order = 10)]
    [ReadOnly(true)]
    decimal Shipped { set; get; }
}
