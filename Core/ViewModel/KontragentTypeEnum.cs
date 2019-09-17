using System.ComponentModel.DataAnnotations;

namespace Core.ViewModel
{
    public enum KontragentTypeEnum
    {
        [Display(Name = "Неизвестно")] Unknown,
        [Display(Name = "Контрагент")] Kontragent,
        [Display(Name = "Банк")] Bank,
        [Display(Name = "Касса")] Cash,
        [Display(Name = "Склад")] Store,
        [Display(Name = "Сотрудник")] Employee
    }
}