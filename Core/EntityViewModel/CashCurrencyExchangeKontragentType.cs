using System.ComponentModel.DataAnnotations;

namespace Core.EntityViewModel
{
    public enum CashCurrencyExchangeKontragentType
    {
        [Display(Name = "����������")] Kontragent = 1,
        [Display(Name = "���������")] Employee = 2,
        [Display(Name = "�� ������")] NotChoice = 5
    }
}