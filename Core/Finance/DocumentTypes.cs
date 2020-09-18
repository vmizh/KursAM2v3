using System.ComponentModel.DataAnnotations;

namespace Core.Finance
{
    public enum DocumentTypes
    {
        [Display(Name = "Тип не указан")] NotType = 0,

        [Display(Name = "Банковская транзакция")]
        Bank = 101,

        [Display(Name = "Приходный кассовый ордер")]
        CashIn = 33,

        [Display(Name = "Расходный кассовый ордер")]
        CashOut = 34,

        //Счет фактура поставщика
        [Display(Name = "Счет-фактура поставщика")]
        AccountIn = 26,

        //Счет фактура для клиента
        [Display(Name = "Счет фактура для клиента")]
        AccountOut = 84,

        // Продажа за наличный расчет
        SaleForCash = 259,

        // Акт взаимозачета
        [Display(Name = "Акт взаимозачета")] ActZachet = 110,

        //акт сверки
        ActReconciliation = 430
    }
}