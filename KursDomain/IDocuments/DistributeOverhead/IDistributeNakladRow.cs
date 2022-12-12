using System;
using DevExpress.Xpf.Bars;
using KursDomain.IDocuments.Finance;
using IInvoiceProviderRow = KursDomain.IDocuments.Finance.IInvoiceProviderRow;

namespace KursDomain.IDocuments.DistributeOverhead;

public interface IDistributeNakladRow
{
    Guid Id { set; get; }
    Guid DocId { set; get; }
    IInvoiceProviderRow InvoiceRow { set; get; }
    ISFProviderNomenklCurrencyConvert Convert { set; get; }

    string Note { set; get; }
    decimal Quantity { set; get; }
    decimal Price { set; get; }
    decimal Summa { set; get; }
    decimal DistributeSumma { set; get; }
    decimal DistributePrice { set; get; }
}
