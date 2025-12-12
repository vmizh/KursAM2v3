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
    [Display(AutoGenerateField = true, Name = "Тип док-та", Order = 1)]
    public string DocType => DocumentType.GetDisplayAttributesFrom(DocumentType.GetType()).Name;

    [Display(AutoGenerateField = true,  Name = "Внут.№", Order = 2)]
    public int? InnerNumber { set; get; }

    [Display(AutoGenerateField = true,  Name = "Внеш.№", Order = 3)]
    public string ExtNumber { set; get; }

    [Display(AutoGenerateField = true,  Name = "Дата", Order = 4)]
    public DateTime DocDate { set; get; }

    [Display(AutoGenerateField = true,  Name = "Контрагент", Order = 5)]
    public Kontragent Kontragent { set; get; }

    [Display(AutoGenerateField = true,  Name = "Дилер", Order = 6)]
    public Kontragent Diler { set; get; }

    [Display(AutoGenerateField = true,  Name = "Склад", Order = 7)]
    public References.Warehouse Warehouse { set; get; }
    [Display(AutoGenerateField = true,  Name = "Примечание", Order = 8)]
    public string Note { set; get; }

    [Display(AutoGenerateField = true,  Name = "Создатель", Order = 9)]
    public string Creator { set; get; }

    [Display(AutoGenerateField = true,  Name = "Сумма (поставщик)", Order = 10)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ProviderSumma { set; get; }
    [Display(AutoGenerateField = true,  Name = "Сумма (клиент)", Order = 10)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ClientSumma { set; get; }

    [Display(AutoGenerateField = true,  Name = "Отгружено (поставщик)", Order = 10)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ProviderShipped { set; get; }

    [Display(AutoGenerateField = true,  Name = "Отгружено (клиент)", Order = 10)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ClientShipped { set; get; }

    [Display(AutoGenerateField = true,  Name = "Кол-во (поставщик)", Order = 10)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ProviderQuantity { set; get; }

    [Display(AutoGenerateField = true,  Name = "Кол-во (клиент)", Order = 10)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ClientQuantity { set; get; }

    [Display(AutoGenerateField = true,  Name = "Кол-во отгр. (поставщик)", Order = 10)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ProviderShippedQuantity { set; get; }

    [Display(AutoGenerateField = true,  Name = "Кол-во отгр. (клиент)", Order = 10)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal ClientShippedQuantity { set; get; }

    [Display(AutoGenerateField = true)] public bool IsInclude { set; get; } = false;
}
