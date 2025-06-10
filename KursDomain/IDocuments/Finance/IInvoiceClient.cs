using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KursDomain.References;

namespace KursDomain.IDocuments.Finance;

public interface IInvoiceClient : ILastChanged
{
    [Display(AutoGenerateField = false)] decimal DocCode { set; get; }

    [Display(AutoGenerateField = false)] Guid Id { set; get; }

    [Display(AutoGenerateField = true, Name = "Поставщик", Order = 9)]
    Kontragent Receiver { set; get; }

    [Display(AutoGenerateField = true, Name = "Центр ответственности", Order = 12)]
    CentrResponsibility CO { set; get; }

    [Display(AutoGenerateField = true, Name = "Тип продукции", Order = 13)]
    NomenklProductType VzaimoraschetType { set; get; }

    [Display(AutoGenerateField = true, Name = "Форма расчетов", Order = 15)]
    PayForm FormRaschet { set; get; }

    [Display(AutoGenerateField = true, Name = "Условия оплаты", Order = 14)]
    PayCondition PayCondition { set; get; }

    [Display(AutoGenerateField = true, Name = "Дата", Order = 1)]
    DateTime DocDate { set; get; }

    [Display(AutoGenerateField = true, Name = "№", Order = 2)]
    int InnerNumber { set; get; }

    [Display(AutoGenerateField = true, Name = "Внешний №", Order = 3)]
    string OuterNumber { set; get; }

    [Display(AutoGenerateField = true, Name = "Клиент", Order = 4)]
    Kontragent Client { set; get; }

    [Display(AutoGenerateField = true, Name = "Валюта", Order = 6)]
    Currency Currency { set; get; }

    [Display(AutoGenerateField = true, Name = "Отгружено", Order = 8)]
    [DisplayFormat(DataFormatString = "n2", NullDisplayText = "0")]
    decimal SummaOtgruz { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма дилера", Order = 17)]
    [DisplayFormat(DataFormatString = "n2", NullDisplayText = "0")]
    decimal DilerSumma { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание", Order = 10)]
    string Note { set; get; }

    [Display(AutoGenerateField = true, Name = "Дилер", Order = 16)]
    Kontragent Diler { set; get; }

    [Display(AutoGenerateField = true, Name = "Акцептован", Order = 18)]
    bool IsAccepted { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма", Order = 5)]
    [DisplayFormat(DataFormatString = "n2", NullDisplayText = "0")]
    decimal Summa { set; get; }

    [Display(AutoGenerateField = true, Name = "Создатель", Order = 11)]
    string CREATOR { set; get; }

    [Display(AutoGenerateField = true, Name = "НДС в цене", Order = 19)]
    bool IsNDSIncludeInPrice { set; get; }

    [Display(AutoGenerateField = true, Name = "Оплачено", Order = 7)]
    [DisplayFormat(DataFormatString = "n2", NullDisplayText = "0")]
    decimal PaySumma { set; get; }

    [Display(AutoGenerateField = true, Name = "Ответственный", Order = 20)]
    Employee PersonaResponsible { set; get; }

    
    [Display(AutoGenerateField = true, Name = "Искл. из поиска", Order = 20, Description = "Исключить из диалогов поиска для оплаты")]
    public bool? IsExcludeFromPays { set; get; }


    /// <summary>
    /// Пользователь, последний изменивший документ
    /// </summary>
    [Display(AutoGenerateField = true, Name = "Посл.изменил", Order = 21)]
    public string LastChanger { set; get; }

    /// <summary>
    /// Дата последнего изменения
    /// </summary>
    [Display(AutoGenerateField = true, Name = "Дата посл.изм.", Order = 21)]
    public DateTime LastChangerDate { set; get; }

    [Display(AutoGenerateField = false)]
    public decimal? VazaimoraschetTypeDC { set; get; }

    [Display(AutoGenerateField = false)] ObservableCollection<IInvoiceClientRow> Rows { set; get; }
}

public interface IInvoiceClientRemains : IInvoiceClient {

    [Display(AutoGenerateField = true, Name = "Кол-во", Order = 21)]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "{0:n3")]
    public decimal NomQuantity { set; get; }

}

