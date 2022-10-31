using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.IDocuments.WarehouseOrder;

/// <summary>
///     строка расходного складского ордера
/// </summary>
public interface IIssueWarehouseOrderRow
{
    INomenkl Nomenkl { get; set; }
    decimal Quantity { set; get; }
    bool IsTaxExecuted { set; get; }
    ISDRSchet SDRSchet { set; get; }
    IInvoiceClientRow InvoiceClientRow { set; get; }
    IIncomingWarehouseOrderRow IncomingWarehouseOrderRow { set; get; }
}
