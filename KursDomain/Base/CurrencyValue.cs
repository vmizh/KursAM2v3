using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KursDomain.References;

namespace KursDomain.Base;

public class CurrencyValue
{
    [Display(AutoGenerateField = true,Name = "Валюта", Order = 2)]
    [ReadOnly(true)]
    public Currency Currency { get; set; }

    [Display(AutoGenerateField = true,Name = "Сумма",Order = 1)]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Value { get; set; }
}
