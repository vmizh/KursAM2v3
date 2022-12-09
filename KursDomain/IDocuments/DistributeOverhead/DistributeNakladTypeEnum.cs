using System.ComponentModel.DataAnnotations;

namespace KursDomain.IDocuments.DistributeOverhead;

/// <summary>
///     Типы распределения накладных расходов
/// </summary>
public enum DistributeNakladTypeEnum
{
    [Display(Name = "Нет распределния")] NotDistribute = 0,

    [Display(Name = "По цене")] PriceValue = 1,

    [Display(Name = "По сумме")] SummaValue = 2,

    [Display(Name = "По количеству")] QuantityValue = 3,

    [Display(Name = "По объему")] VolumeValue = 4,

    [Display(Name = "По весу")] WeightValue = 5,

    [Display(Name = "Вручную")] ManualValue = 6
}
