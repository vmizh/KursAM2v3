using System.ComponentModel.DataAnnotations;

namespace KursDomain.IDocuments.WarehouseOrder;

public enum RecepientType
{
    [Display(Name = "Склад")] Warehouse = 0,
    [Display(Name = "Контрагент")] Kontragent = 1
}
