using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using KursDomain.References;

namespace KursDomain.IDocuments.Finance;

public interface IInvoiceProvider
{
    [Display(AutoGenerateField = false)] decimal DocCode { set; get; }

    [Display(AutoGenerateField = false)] Guid Id { set; get; }

    [Display(AutoGenerateField = true, Name = "Накл. расходы") ]
    [DisplayFormat(DataFormatString = "n2")]
    decimal? NakladDistributedSumma { set; get; }

    [Display(AutoGenerateField = true, Name = "Ответственный")]
    Employee PersonaResponsible { set; get; }

    [Display(AutoGenerateField = true, Name = "№")]
    int? SF_IN_NUM { set; get; }

    [Display(AutoGenerateField = true, Name = "№ поставщика")]
    string SF_POSTAV_NUM { set; get; }

    [Display(AutoGenerateField = true, Name = "Дата")]
    DateTime DocDate { set; get; }

    [Display(AutoGenerateField = true, Name = "Поставщик")]
    Kontragent Kontragent { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма")]
    decimal Summa { get; }

    [Display(AutoGenerateField = true, Name = "Отгружено")]
    decimal SummaFact { set; get; }

    [Display(AutoGenerateField = true, Name = "Валюта")]
    Currency Currency { set; get; }

    [Display(AutoGenerateField = true, Name = "Оплачен")]
    bool IsPay { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма оплаты")]
    decimal PaySumma { get; }

    [Display(AutoGenerateField = true, Name = "Усл.платежа")]
    PayCondition PayCondition { set; get; }

    [Display(AutoGenerateField = true, Name = "Акцептован")]
    bool IsAccepted { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    string Note { set; get; }

    [Display(AutoGenerateField = true, Name = "Создатель")]
    string CREATOR { set; get; }

    [Display(AutoGenerateField = true, Name = "Форма расчетов")]
    PayForm FormRaschet { set; get; }

    [Display(AutoGenerateField = true, Name = "НДС вкл. в цену")]
    bool IsNDSInPrice { set; get; }

    [Display(AutoGenerateField = true, Name = "Центр ответст.")]
    CentrResponsibility CO { set; get; }

    [Display(AutoGenerateField = true, Name = "Получатель")]
    Kontragent KontrReceiver { set; get; }

    ObservableCollection<IInvoiceProviderRow> Rows { set; get; }
}
