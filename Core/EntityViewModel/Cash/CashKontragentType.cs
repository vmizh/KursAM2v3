using System.ComponentModel.DataAnnotations;

namespace Core.EntityViewModel.Cash
{
    public enum CashKontragentType
    {
        [Display(Name = "Контрагент")] 
        Kontragent = 1,
        [Display(Name = "Сотрудник")] 
        Employee = 2,
        [Display(Name = "Касса")] 
        Cash = 3,
        [Display(Name = "Банк")] 
        Bank = 4,
        [Display(Name = "Не выбран")] 
        NotChoice = 5
    }
}