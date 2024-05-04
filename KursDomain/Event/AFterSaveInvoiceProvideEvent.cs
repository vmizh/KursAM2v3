using System;
using KursDomain.Documents.Invoices;
using Prism.Events;

namespace KursDomain.Event
{
    public class AFterSaveInvoiceProvideEvent : PubSubEvent<AFterSaveInvoiceProvideEventArgs>
    {
    }

    public class AFterSaveInvoiceProvideEventArgs
    {
        public Guid Id { get; set; }
        public decimal DocCode { get; set; }
        public InvoiceProvider Invoice { get; set; }
    }
}
