using System.ComponentModel.DataAnnotations;

namespace KursDomain.Documents.Cash
{
    public enum CashCurrencyExchangeKontragentType
    {
        [Display(Name = "����������")] Kontragent = 1,
        [Display(Name = "���������")] Employee = 2,
        [Display(Name = "�� ������")] NotChoice = 5
    }
}
