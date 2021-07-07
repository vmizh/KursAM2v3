using System.ComponentModel.DataAnnotations;

namespace Core.EntityViewModel.CommonReferences
{
    public enum DocumentType
    {
        [Display(Name = "�� ��������")] None = 0,

        [Display(Name = "��������� ������� �����")]
        CashIn = 33,

        [Display(Name = "��������� ������� �����")]
        CashOut = 34,

        [Display(Name = "���������� ��������")]
        Bank = 101,
        [Display(Name = "����� ������")] CurrencyChange = 251,
        [Display(Name = "��� ������������")] MutualAccounting = 110,
        [Display(Name = "��� �����������")] CurrencyConvertAccounting = 111,

        [Display(Name = "������������������ ���������")]
        InventoryList = 359,

        [Display(Name = "��������� ��������� �����")]
        StoreOrderIn = 357,

        [Display(Name = "����-������� ��������")]
        InvoiceClient = 84,

        [Display(Name = "����-������� �����������")]
        InvoiceProvider = 26,

        [Display(Name = "��������� ���������")]
        Waybill = 368,

        [Display(Name = "��������� ���������� ���������� �����")]
        PayRollVedomost = 903,

        [Display(Name = "��� �������� ���������� �����������")]
        NomenklTransfer = 10001,

        [Display(Name = "���������� ��������")]
        ProjectsReference = 10002,

        [Display(Name = "������� ��� ��������")]
        DogovorClient = 9,

        [Display(Name = "������� �� �������� �������")]
        SaleForCash = 259,

        [Display(Name = "��� ������")]
        ActReconciliation = 430,
        
        [Display(Name = "��� ��������")]
        AktSpisaniya = 1003

    }
}