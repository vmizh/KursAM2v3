using System.ComponentModel.DataAnnotations;

namespace KursDomain.Documents.NomenklManagement;

public enum WarehouseSenderType
{
    [Display(Name = "Контрагент")] Kontragent = 0,
    [Display(Name = "Склад")] Store = 1
}
