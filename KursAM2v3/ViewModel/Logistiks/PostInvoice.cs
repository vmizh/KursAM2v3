using Core.EntityViewModel;
using Data;

namespace KursAM2.ViewModel.Logistiks
{
    public class PostInvoice : InvoiceProvider
    {
        public PostInvoice()
        {
        }

        public PostInvoice(SD_26 entity)
            : base(entity)
        {
        }
    }
}