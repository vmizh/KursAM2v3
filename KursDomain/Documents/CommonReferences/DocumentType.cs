using System.ComponentModel.DataAnnotations;

namespace KursDomain.Documents.CommonReferences;

public enum DocumentType
{
    /// <summary>
    ///     �� ��������
    /// </summary>
    [Display(Name = "�� ��������")] None = 0,

    /// <summary>
    ///     ��������� ������� �����
    /// </summary>
    [Display(Name = "��������� ������� �����")]
    CashIn = 33,

    /// <summary>
    ///     ��������� ������� �����
    /// </summary>
    [Display(Name = "��������� ������� �����")]
    CashOut = 34,

    /// <summary>
    ///     ���������� ��������
    /// </summary>
    [Display(Name = "���������� ��������")]
    Bank = 101,

    /// <summary>
    ///     ����� ������
    /// </summary>
    [Display(Name = "����� ������")] CurrencyChange = 251,

    /// <summary>
    ///     ��� ������������
    /// </summary>
    [Display(Name = "��� ������������")] MutualAccounting = 110,

    /// <summary>
    ///     ��� �����������
    /// </summary>
    [Display(Name = "��� �����������")] CurrencyConvertAccounting = 111,

    /// <summary>
    ///     ������������������ ���������
    /// </summary>
    [Display(Name = "������������������ ���������")]
    InventoryList = 359,

    /// <summary>
    ///     ��������� ��������� �����
    /// </summary>
    [Display(Name = "��������� ��������� �����")]
    StoreOrderIn = 357,
    /// <summary>
    ///     ��������� ��������� �����
    /// </summary>
    [Display(Name = "��������� ��������� �����")]
    StoreOrderOut = 358,

    /// <summary>
    ///     ����-������� ��������
    /// </summary>
    [Display(Name = "����-������� ��������")]
    InvoiceClient = 84,

    /// <summary>
    ///     ����-������� �����������
    /// </summary>
    [Display(Name = "����-������� �����������")]
    InvoiceProvider = 26,

    /// <summary>
    ///     ��������� ���������
    /// </summary>
    [Display(Name = "��������� ���������")]
    Waybill = 368,

    /// <summary>
    ///     ��������� ���������� ���������� �����
    /// </summary>
    [Display(Name = "��������� ���������� ���������� �����")]
    PayRollVedomost = 903,

    /// <summary>
    ///     ��� �������� ���������� �����������
    /// </summary>
    [Display(Name = "��� �������� ���������� �����������")]
    NomenklTransfer = 10001,

    /// <summary>
    ///     ���������� ��������
    /// </summary>
    [Display(Name = "���������� ��������")]
    ProjectsReference = 10002,

    /// <summary>
    ///     ������� ��� ��������
    /// </summary>
    [Display(Name = "������� ��� ��������")]
    DogovorClient = 9,

    /// <summary>
    ///     ������� �� �����������
    /// </summary>
    [Display(Name = "������� �� �����������")]
    DogovorOfSupplier = 112,

    /// <summary>
    ///     ������� �� �������� ������
    /// </summary>
    [Display(Name = "������� �� �������� ������")]
    SaleForCash = 259,

    /// <summary>
    ///     ��� ������
    /// </summary>
    [Display(Name = "��� ������")] ActReconciliation = 430,

    /// <summary>
    ///     ��� ��������
    /// </summary>
    [Display(Name = "��� ��������")] AktSpisaniya = 72,

    /// <summary>
    ///     ������������� ���������� �� �����������
    /// </summary>
    [Display(Name = "������������� ���������� ��� ��������")]
    AccruedAmountForClient = 1004,

    /// <summary>
    ///     ���� ���������� ��� ������������ ������������
    /// </summary>
    [Display(Name = "������ �������")] AccruedAmountOfSupplier = 74,

    //��������� ���������� ����������
    [Display(Name = "��������� ���������� ����������")]
    StockHolderAccrual = 79,

    [Display(Name = "������������� ��������� ��������")]
    Naklad = 1005,

    [Display(Name = "������� �� ������")]
    TransferOutBalans = 1006


}
