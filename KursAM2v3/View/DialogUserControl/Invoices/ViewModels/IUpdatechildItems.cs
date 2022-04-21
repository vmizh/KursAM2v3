namespace KursAM2.View.DialogUserControl.Invoices.ViewModels
{
    /// <summary>
    /// Методы обновления значений зависимых записей
    /// </summary>
    public interface IUpdatechildItems
    {
        void UpdateSelectedItems();
        void UpdatePositionItem();
        void UpdateInvoiceItem();
    }
}