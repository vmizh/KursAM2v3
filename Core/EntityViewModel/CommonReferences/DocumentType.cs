using System.ComponentModel.DataAnnotations;

namespace Core.EntityViewModel.CommonReferences
{
    public enum DocumentType
    {
        [Display(Name = "Не документ")] None = 0,

        [Display(Name = "Приходный касовый ордер")]
        CashIn = 33,

        [Display(Name = "Расходный касовый ордер")]
        CashOut = 34,

        [Display(Name = "Банковская проводка")]
        Bank = 101,
        [Display(Name = "Обмен валюты")] CurrencyChange = 251,
        [Display(Name = "Акт взаимозачета")] MutualAccounting = 110,
        [Display(Name = "Акт конвертации")] CurrencyConvertAccounting = 111,

        [Display(Name = "Инвентаризационная ведомость")]
        InventoryList = 359,

        [Display(Name = "Приходный складской ордер")]
        StoreOrderIn = 357,

        [Display(Name = "Счет-фактура клиентам")]
        InvoiceClient = 84,

        [Display(Name = "Счет-фактура поставщиков")]
        InvoiceProvider = 26,

        [Display(Name = "Расходная накладная")]
        Waybill = 368,

        [Display(Name = "Ведомость начисления заработной платы")]
        PayRollVedomost = 903,

        [Display(Name = "Акт валютной таксировки номенклатур")]
        NomenklTransfer = 10001,

        [Display(Name = "Справочник проектов")]
        ProjectsReference = 10002,

        [Display(Name = "Договор для клиентов")]
        DogovorClient = 9,

        [Display(Name = "Продажа за наличный раксчет")]
        SaleForCash = 259,

        [Display(Name = "Акт сверки")]
        ActReconciliation = 430,
        
        [Display(Name = "Акт списания")]
        AktSpisaniya = 1003

    }
}