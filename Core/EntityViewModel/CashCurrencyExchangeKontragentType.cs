using System.ComponentModel.DataAnnotations;

namespace Core.EntityViewModel
{
    public enum CashCurrencyExchangeKontragentType
    {
        [Display(Name = "Контрагент")] Kontragent = 1,
        [Display(Name = "Сотрудник")] Employee = 2,
        [Display(Name = "Не выбран")] NotChoice = 5
    }
}