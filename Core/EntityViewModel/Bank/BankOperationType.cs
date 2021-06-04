using System.ComponentModel.DataAnnotations;

namespace Core.EntityViewModel.Bank
{
    public enum BankOperationType
    {
        [Display(Name = "Контрагент")] Kontragent = 2,

        [Display(Name = "Приходный кассовый ордер")]
        CashIn = 3,

        [Display(Name = "Расходный кассовый ордер")]
        CashOut = 4,
        [Display(Name = "Банк получатель")] BankIn = 5,
        [Display(Name = "Банк отправитель")] BankOut = 6,
        [Display(Name = "Не выбран")] NotChoice = 1,
        [Display(Name = "Обмен валюты")] CurrencyChange = 7
    }
}