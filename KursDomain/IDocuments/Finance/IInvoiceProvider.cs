using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursDomain.IDocuments.Finance;

public interface IInvoiceProvider : IDocCode, IDocGuid
{
    //[Display(AutoGenerateField = false)] decimal DocCode { set; get; }
    // Поиск счетов-фактур поставщиков 
    //[Display(AutoGenerateField = false)] Guid Id { set; get; }

    [Display(AutoGenerateField = true, Name = "Накл. расходы", Order = 13) ]
    [DisplayFormat(DataFormatString = "n2")]
    decimal? NakladDistributedSumma { set; get; }

    [Display(AutoGenerateField = true, Name = "Ответственный", Order = 14)]
    Employee PersonaResponsible { set; get; }

    [Display(AutoGenerateField = true, Name = "№", Order = 2)]
    int? SF_IN_NUM { set; get; }

    [Display(AutoGenerateField = true, Name = "№ поставщика", Order = 3)]
    string SF_POSTAV_NUM { set; get; }

    [Display(AutoGenerateField = true, Name = "Дата", Order = 1)]
    DateTime DocDate { set; get; }

    [Display(AutoGenerateField = true, Name = "Поставщик", Order =4)]
    Kontragent Kontragent { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма", Order = 5)]
    [DisplayFormat(DataFormatString = "n2")]
    decimal Summa { get; }

    [Display(AutoGenerateField = true, Name = "Отгружено", Order = 8)]
    [DisplayFormat(DataFormatString = "n2")]
    decimal SummaFact { set; get; }

    [Display(AutoGenerateField = true, Name = "Валюта", Order = 6)]
    Currency Currency { set; get; }

    [Display(AutoGenerateField = true, Name = "Оплачен", Order = 12)]
    bool IsPay { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма оплаты", Order = 7)]
    [DisplayFormat(DataFormatString = "n2")]
    decimal PaySumma { get; }

    [Display(AutoGenerateField = true, Name = "Усл.платежа", Order = 15)]
    PayCondition PayCondition { set; get; }

    [Display(AutoGenerateField = true, Name = "Акцептован", Order = 11)]
    bool IsAccepted { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание", Order = 16)]
    [MaxLength(100)]
    string Note { set; get; }

    [Display(AutoGenerateField = true, Name = "Создатель", Order = 17)]
    string CREATOR { set; get; }

    [Display(AutoGenerateField = true, Name = "Форма расчетов", Order = 18)]
    PayForm FormRaschet { set; get; }

    [Display(AutoGenerateField = true, Name = "НДС вкл. в цену", Order = 19)]
    bool IsNDSInPrice { set; get; }

    [Display(AutoGenerateField = true, Name = "Центр ответст.", Order = 10)]
    CentrResponsibility CO { set; get; }

    [Display(AutoGenerateField = true, Name = "Получатель", Order = 9)]
    Kontragent KontrReceiver { set; get; }

    ObservableCollection<IInvoiceProviderRow> Rows { set; get; }
}
