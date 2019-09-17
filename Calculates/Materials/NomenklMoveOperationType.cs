namespace Calculates.Materials
{
    public enum NomenklMoveOperationType
    {
        /// <summary>
        ///     ������ �� �������� ����������� �����������
        /// </summary>
        ReceiptKontragent = 0,

        /// <summary>
        ///     ������ �� �������� �������������� �����������
        /// </summary>
        ReceiptOutBalansKontragent = 1,

        /// <summary>
        ///     ������ �� ������������������ ���������
        /// </summary>
        ReceiptInventory = 2,

        /// <summary>
        ///     ������ �� ���� ������� ���������
        /// </summary>
        ReceiptAssemblyProduct = 3,

        /// <summary>
        ///     ������ �� ���� �������� ��������� (����������������)
        /// </summary>
        ReceiptDisassemblyProduct = 4,

        /// <summary>
        ///     ������ �� ��������� ����������� �����������
        /// </summary>
        ReceiptInnerMove = 5,

        /// <summary>
        ///     ������ �� ������� �� �������� ������
        /// </summary>
        ReceiptSpotSale = 6,

        /// <summary>
        ///     �������� ������ ����������� ��������
        /// </summary>
        ExpenseKontragent = 100,

        /// <summary>
        ///     �������� �������������� �����������
        /// </summary>
        ExpenseOutBalansKontragent = 101,

        /// <summary>
        ///     �������� �� ������������������ ���������
        /// </summary>
        ExpenseInventory = 102,

        /// <summary>
        ///     �������� ������ �� ���� ������� ������� ���������
        /// </summary>
        ExpenseAssemblyProduct = 103,

        /// <summary>
        ///     �������� �� ���� ���������������� ���������
        /// </summary>
        ExpenseDisassemblyProduct = 104,

        /// <summary>
        ///     ������ �� ��������� �� ���������� �����������
        /// </summary>
        ExpenseInnerMove = 105,

        /// <summary>
        ///     �������� �� ���� ��������
        /// </summary>
        ExpenseWriteOff = 106,

        /// <summary>
        ///     �������� �� ������� �� �������� ������
        /// </summary>
        ExpenseSpotSale = 106,
        /// <summary>
        /// ������� ������
        /// </summary>
        Transfer = 107
    }
}