using System.ComponentModel.DataAnnotations;

namespace KursDomain.IDocuments;

public enum MaterialDocumentTypeEnum
{
    [Display(Name="Не указан")]
    None = 0,
    [Display(Name="Приходный складской ордер")]
    WarehouseIn = 2010000001,
    [Display(Name="Расходный складской ордер")]
    WarehouseOut = 2010000003,
    [Display(Name="Лимитно-заборная карта")]
    LimitАenceСard = 2010000004,
    [Display(Name="Инвентаризационная ведомость")]
    InventoryStatement = 2010000005,
    [Display(Name="Требование на выдачу")]
    TheRequirementIssue = 2010000006,
    [Display(Name="Накладная на отпуск на сторону")]
    NakladShippingRequirements = 2010000007,
    [Display(Name="Акт приемки готовой продукции")]
    ActAcceptanceProduct = 2010000008,
    [Display(Name="Акт разукомплектации готовой продукции")]
    ActRejectanceProduct = 2010000009,
    [Display(Name="Акт списания материалов")]
    ActWritingOffMaterials = 2010000010,
    [Display(Name="Расходная накладная (без требования)")]
    WayBill = 2010000012,
    [Display(Name="Накладная на внутренее перемещение")]
    WarehouseInnerMove   = 2010000014,
    [Display(Name="Складской ордер Транзит")]
    WarehouseTranzit = 2010000016

    
}
