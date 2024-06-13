using System.ComponentModel.DataAnnotations;

namespace KursDomain.References;

/// <summary>
/// Типы объектов для утсановления прав по центрам ответственности
/// </summary>
public enum ResponsibleRightObjectTypeEnum
{
    [Display(Name = "Контрагент")]
    Kongtragent = 1,
    [Display(Name = "Склад")]
    Warehouse = 2,
    [Display(Name = "Касса")]
    Cash = 3,
    [Display(Name = "Банковский счет")]
    BankAccount = 4,
    [Display(Name = "Товар")]
    Nomenkl = 5
}
