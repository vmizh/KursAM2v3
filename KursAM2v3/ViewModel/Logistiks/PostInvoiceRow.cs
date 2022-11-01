using Data;
using KursDomain.Documents.Invoices;

namespace KursAM2.ViewModel.Logistiks
{
    public class PostInvoiceRow : InvoiceProviderRow
    {
        public PostInvoiceRow()
        {
        }

        public PostInvoiceRow(TD_26 entity) : base(entity)
        {
        }
    }
}
