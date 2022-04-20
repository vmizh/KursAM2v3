using System;

namespace KursAM2.View.DialogUserControl.Invoices.ViewModels
{
    [Flags]
    public enum InvoiceProviderSearchType
    {
        // Весь список
        All = 1,
        // С учетом дат
        DataRange = 2,
        // Только не полностью оплаченные
        NotPay = 4,
        // Только не полностью отгруженные
        NotShipped = 8,
        //Только для конкретного контрагента
        OneKontragent = 16,
        //Множественный выбор
        MultipleSelected = 32,
        //Без позиций, по которым есть накладные расходы
        RemoveNakladRashod = 64,
        //Только услуги
        OnlyUslugi = 128
    }

    [Flags]
    public enum InvoiceClientSearchType
    {
        // Весь список
        All = 1,
        // С учетом дат
        DataRange = 2,
        // Только не полностью оплаченные
        NotPay = 4,
        // Только не полностью отгруженные
        NotShipped = 8,
        //Только для конкретного контрагента
        OneKontragent = 16,
        //Множественный выбор
        MultipleSelected = 32,
        //Без позиций, по которым есть накладные расходы
        OnlyNakladRashod = 64,
        //Не распределенные накладные расходы
        OnlyNakladRashodNotDistributed = 128


    }
}