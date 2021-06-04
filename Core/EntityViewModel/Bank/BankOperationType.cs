using System.ComponentModel.DataAnnotations;

namespace Core.EntityViewModel.Bank
{
    public enum BankOperationType
    {
        [Display(Name = "����������")] Kontragent = 2,

        [Display(Name = "��������� �������� �����")]
        CashIn = 3,

        [Display(Name = "��������� �������� �����")]
        CashOut = 4,
        [Display(Name = "���� ����������")] BankIn = 5,
        [Display(Name = "���� �����������")] BankOut = 6,
        [Display(Name = "�� ������")] NotChoice = 1,
        [Display(Name = "����� ������")] CurrencyChange = 7
    }
}