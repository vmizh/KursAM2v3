using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using KursDomain.Documents.Invoices;
using KursDomain.References;

namespace KursDomain.IDocuments.Finance;

public interface IInvoiceClient
{
    [Display(AutoGenerateField = false)] decimal DocCode { set; get; }

    [Display(AutoGenerateField = false)] Guid Id { set; get; }

    [Display(AutoGenerateField = true, Name = "Поставщик")]
    Kontragent Receiver { set; get; }

    [Display(AutoGenerateField = true, Name = "Центр ответственности")]
    CentrResponsibility CO { set; get; }

    [Display(AutoGenerateField = true, Name = "Тип продукции")]
    NomenklProductType VzaimoraschetType { set; get; }

    [Display(AutoGenerateField = true, Name = "Форма расчетов")]
    PayForm FormRaschet { set; get; }

    [Display(AutoGenerateField = true, Name = "Условия оплаты")]
    PayCondition PayCondition { set; get; }

    [Display(AutoGenerateField = true, Name = "Дата")]
    DateTime DocDate { set; get; }

    [Display(AutoGenerateField = true, Name = "№")]
    int InnerNumber { set; get; }

    [Display(AutoGenerateField = true, Name = "Внешний №")]
    string OuterNumber { set; get; }

    [Display(AutoGenerateField = true, Name = "Клиент")]
    Kontragent Client { set; get; }

    [Display(AutoGenerateField = true, Name = "Валюта")]
    References.Currency Currency { set; get; }

    [Display(AutoGenerateField = true, Name = "Отгружено")]
    [DisplayFormat(DataFormatString = "n2", NullDisplayText = "0")]
    decimal SummaOtgruz { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма дилера")]
    [DisplayFormat(DataFormatString = "n2", NullDisplayText = "0")]
    decimal DilerSumma { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    string Note { set; get; }

    [Display(AutoGenerateField = true, Name = "Дилер")]
    Kontragent Diler { set; get; }

    [Display(AutoGenerateField = true, Name = "Акцептован")]
    bool IsAccepted { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма")]
    [DisplayFormat(DataFormatString = "n2", NullDisplayText = "0")]
    decimal Summa { set; get; }

    [Display(AutoGenerateField = true, Name = "Создатель")]
    string CREATOR { set; get; }

    [Display(AutoGenerateField = true, Name = "НДС в цене")]
    bool IsNDSIncludeInPrice { set; get; }

    [Display(AutoGenerateField = true, Name = "Оплачено")]
    [DisplayFormat(DataFormatString = "n2", NullDisplayText = "0")]
    decimal PaySumma { set; get; }

    [Display(AutoGenerateField = true, Name = "Ответственный")]
    References.Employee PersonaResponsible { set; get; }

    [Display(AutoGenerateField = false)] ObservableCollection<IInvoiceClientRow> Rows { set; get; }
}
