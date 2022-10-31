using System.ComponentModel.DataAnnotations;

namespace KursDomain.IDocuments.WarehouseOrder;

public enum SenderType
{
    [Display(Name = "Склад")] Warehouse = 0,
    [Display(Name = "Контрагент")] Kontragent = 1
}
