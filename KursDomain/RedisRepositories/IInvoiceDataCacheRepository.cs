using KursDomain.References.RedisCache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursAM2.RedisRepositories
{
    public interface IInvoiceDataCacheRepository
    {
        IEnumerable<InvoicePaymentShipment> GetInvoiceClientPartialPayment(DateTime? dateStart = null,
            DateTime? dateEnd = null);

        IEnumerable<InvoicePaymentShipment> GetInvoiceClientPartialShipment(DateTime? dateStart = null,
            DateTime? dateEnd = null);

        IEnumerable<InvoicePaymentShipment> GetInvoiceProviderPartialPayment(DateTime? dateStart = null,
            DateTime? dateEnd = null);

        IEnumerable<InvoicePaymentShipment> GetInvoiceProviderPartialShipment(DateTime? dateStart = null,
            DateTime? dateEnd = null);

        Task UpdateInvoiceClientPaymentShipmentAsync(decimal invoiceDC);
        Task UpdateInvoiceProviderPaymentShipmentAsync(decimal invoiceDC);
        Task UpdateInvoiceClientPaymentShipmentAsync(IEnumerable<decimal> invoiceDCList);
        Task UpdateInvoiceProviderPaymentShipmentAsync(IEnumerable<decimal> invoiceDCList);

        Task ResetInvoiceProviderPaymentShipmentAsync();
        Task ResetInvoiceClientPaymentShipmentAsync();
    }
}
