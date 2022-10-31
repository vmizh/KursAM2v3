using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.IDocuments.WarehouseOrder;

public interface IIncomingWarehouseOrderRow
{
    INomenkl Nomenkl { get; set; }
    decimal Quantity { set; get; }
    bool IsTaxExecuted { set; get; }
    ISDRSchet SDRSchet { set; get; }
    IInvoiceProviderRow InvoiceProviderRow { set; get; }
    IIssueWarehouseOrderRow IssueOrderRow { set; get; }
}
