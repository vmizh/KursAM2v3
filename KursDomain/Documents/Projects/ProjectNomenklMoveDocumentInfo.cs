using System;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using KursDomain.Documents.CommonReferences;
using KursDomain.References;

namespace KursDomain.Documents.Projects;

public class ProjectNomenklMoveDocumentInfo
{
    [Display(AutoGenerateField = false)]
    public decimal DocCode { set; get; }

    [Display(AutoGenerateField = false)]
    public decimal NomenklDC { set; get; }

    [Display(AutoGenerateField = false)]
    public Guid Id { set; get; }

    [Display(AutoGenerateField = false)]
    public DocumentType DocumentType { set; get; }
    [Display(AutoGenerateField = true, Name = "Тип док-та", GroupName = "Основные данные", Order = 1)]
    public string DocType => DocumentType.GetDisplayAttributesFrom(DocumentType.GetType()).Name;

    [Display(AutoGenerateField = true,  Name = "Внут.№", GroupName = "Основные данные", Order = 2)]
    public int? InnerNumber { set; get; }

    [Display(AutoGenerateField = true,  Name = "Внеш.№", GroupName = "Основные данные", Order = 3)]
    public string ExtNumber { set; get; }

    [Display(AutoGenerateField = true, Name = "Дата", GroupName = "Основные данные", Order = 4)]
    public DateTime DocDate { set; get; }

    [Display(AutoGenerateField = true, Name = "Контрагент", GroupName = "Основные данные", Order = 5)]
    public Kontragent Kontragent { set; get; }

    [Display(AutoGenerateField = true, Name = "Дилер", GroupName = "Основные данные", Order = 6)]
    public Kontragent Diler { set; get; }

    [Display(AutoGenerateField = true, Name = "Склад", GroupName = "Основные данные", Order = 7)]
    public References.Warehouse Warehouse { set; get; }
    [Display(AutoGenerateField = true, Name = "Примечание", GroupName = "Основные данные", Order = 8)]
    public string Note { set; get; }

    [Display(AutoGenerateField = true, Name = "Создатель", GroupName = "Основные данные", Order = 9)]
    public string Creator { set; get; }

    [Display(AutoGenerateField = true, Name = "Сумма (поставщик)", GroupName = "Документ", Order = 10)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ProviderSumma { set; get; }
    [Display(AutoGenerateField = true,  Name = "Сумма (клиент)", GroupName = "Документ", Order = 11)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ClientSumma { set; get; }

    [Display(AutoGenerateField = true,  Name = "Отгружено (поставщик)", GroupName = "Документ", Order = 12)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ProviderShipped { set; get; }

    [Display(AutoGenerateField = true,  Name = "Отгружено (клиент)", GroupName = "Документ", Order = 13)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ClientShipped { set; get; }

    [Display(AutoGenerateField = true,  Name = "Кол-во (поставщик)", GroupName = "Документ", Order = 14)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ProviderQuantity { set; get; }

    [Display(AutoGenerateField = true,  Name = "Кол-во (клиент)", GroupName = "Документ", Order = 15)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ClientQuantity { set; get; }

    [Display(AutoGenerateField = true,  Name = "Кол-во отгр. (поставщик)", GroupName = "Документ", Order = 16)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ProviderShippedQuantity { set; get; }

    [Display(AutoGenerateField = true,  Name = "Кол-во отгр. (клиент)", GroupName = "Документ", Order = 17)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ClientShippedQuantity { set; get; }


    [Display(AutoGenerateField = true)] public bool IsInclude { set; get; } = false;

    #region Ручная установка

    [Display(AutoGenerateField = true, Name = "Сумма (поставщик)", GroupName = "Учтено в проекте", Order = 18)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ManualProviderSumma { set; get; }
    [Display(AutoGenerateField = true, Name = "Сумма (клиент)", GroupName = "Учтено в проекте", Order = 19)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ManualClientSumma { set; get; }

    [Display(AutoGenerateField = true, Name = "Кол-во (поставщик)", GroupName = "Учтено в проекте", Order = 22)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ManualProviderQuantity { set; get; }

    [Display(AutoGenerateField = true, Name = "Кол-во (клиент)", GroupName = "Учтено в проекте", Order = 23)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ManualClientQuantity { set; get; }

    [Display(AutoGenerateField = true)]
    public bool IsManualChanged =>
        (ClientQuantity - ManualClientQuantity) + (ProviderQuantity - ManualProviderQuantity) > 0;

    #endregion
}
