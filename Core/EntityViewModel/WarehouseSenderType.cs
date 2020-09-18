using System.ComponentModel.DataAnnotations;

namespace Core.EntityViewModel
{
    public enum WarehouseSenderType
    {
        [Display(Name = "Контрагент")] Kontragent = 0,
        [Display(Name = "Склад")] Store = 1
    }
}