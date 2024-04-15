using System.ComponentModel.DataAnnotations;

namespace KursDomain.Documents.CommonReferences;

public enum DocumentType
{
    /// <summary>
    ///     Не документ
    /// </summary>
    [Display(Name = "Не документ")] None = 0,

    /// <summary>
    ///     Приходный касовый ордер
    /// </summary>
    [Display(Name = "Приходный касовый ордер")]
    CashIn = 33,

    /// <summary>
    ///     Расходный касовый ордер
    /// </summary>
    [Display(Name = "Расходный касовый ордер")]
    CashOut = 34,

    /// <summary>
    ///     Банковская проводка
    /// </summary>
    [Display(Name = "Банковская проводка")]
    Bank = 101,

    /// <summary>
    ///     Обмен валюты
    /// </summary>
    [Display(Name = "Обмен валюты")] CurrencyChange = 251,

    /// <summary>
    ///     Акт взаимозачета
    /// </summary>
    [Display(Name = "Акт взаимозачета")] MutualAccounting = 110,

    /// <summary>
    ///     Акт конвертации
    /// </summary>
    [Display(Name = "Акт конвертации")] CurrencyConvertAccounting = 111,

    /// <summary>
    ///     Инвентаризационная ведомость
    /// </summary>
    [Display(Name = "Инвентаризационная ведомость")]
    InventoryList = 359,

    /// <summary>
    ///     Приходный складской ордер
    /// </summary>
    [Display(Name = "Приходный складской ордер")]
    StoreOrderIn = 357,
    /// <summary>
    ///     Приходный складской ордер
    /// </summary>
    [Display(Name = "Приходный складской ордер")]
    StoreOrderOut = 358,

    /// <summary>
    ///     Счет-фактура клиентам
    /// </summary>
    [Display(Name = "Счет-фактура клиентам")]
    InvoiceClient = 84,

    /// <summary>
    ///     Счет-фактура поставщиков
    /// </summary>
    [Display(Name = "Счет-фактура поставщиков")]
    InvoiceProvider = 26,

    /// <summary>
    ///     Расходная накладная
    /// </summary>
    [Display(Name = "Расходная накладная")]
    Waybill = 368,

    /// <summary>
    ///     Ведомость начисления заработной платы
    /// </summary>
    [Display(Name = "Ведомость начисления заработной платы")]
    PayRollVedomost = 903,

    /// <summary>
    ///     Акт валютной таксировки номенклатур
    /// </summary>
    [Display(Name = "Акт валютной таксировки номенклатур")]
    NomenklTransfer = 10001,

    /// <summary>
    ///     Справочник проектов
    /// </summary>
    [Display(Name = "Справочник проектов")]
    ProjectsReference = 10002,

    /// <summary>
    ///     Договор для клиентов
    /// </summary>
    [Display(Name = "Договор для клиентов")]
    DogovorClient = 9,

    /// <summary>
    ///     Договор от поставщиков
    /// </summary>
    [Display(Name = "Договор от поставщиков")]
    DogovorOfSupplier = 112,

    /// <summary>
    ///     Продажа за наличный расчет
    /// </summary>
    [Display(Name = "Продажа за наличный расчет")]
    SaleForCash = 259,

    /// <summary>
    ///     Акт сверки
    /// </summary>
    [Display(Name = "Акт сверки")] ActReconciliation = 430,

    /// <summary>
    ///     Акт списания
    /// </summary>
    [Display(Name = "Акт списания")] AktSpisaniya = 72,

    /// <summary>
    ///     Внебалансовые начисления от поставщиков
    /// </summary>
    [Display(Name = "Внебалансовые начисления для клиентов")]
    AccruedAmountForClient = 1004,

    /// <summary>
    ///     Типы начислений для внебаласовых контрагентов
    /// </summary>
    [Display(Name = "Прямые затраты")] AccruedAmountOfSupplier = 74,

    //Ведомости начислений акционерам
    [Display(Name = "Ведомости начислений акционерам")]
    StockHolderAccrual = 79,

    [Display(Name = "Распределение накладных расходов")]
    Naklad = 1005,

    [Display(Name = "Перевод за баланс")]
    TransferOutBalans = 1006


}
