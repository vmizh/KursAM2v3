using System.ComponentModel.DataAnnotations;

namespace Core.EntityViewModel.Cash
{
    public enum CashKontragentType
    {
        [Display(Name = "����������")] 
        Kontragent = 1,
        [Display(Name = "���������")] 
        Employee = 2,
        [Display(Name = "�����")] 
        Cash = 3,
        [Display(Name = "����")] 
        Bank = 4,
        [Display(Name = "�� ������")] 
        NotChoice = 5
    }
}