using System;
using System.Collections.Generic;
using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.IDocuments.WarehouseOrder;

/// <summary>
///     Расходная накладная
/// </summary>
public interface IConsumableWarehouseInvoice
{
    int OrderNumber { get; set; }
    string OuterNumber { get; set; }
    DateTime OrderDate { get; set; }
    string Creator { get; set; }
    SenderType SenderType { get; set; }
    IKontragent RecepientKontragent { get; set; }
    IInvoiceProvider InvoiceClient { set; get; }
    IWarehouse Warehouse { set; get; }

    IEnumerable<IConsumableWarehouseInvoiceRow> Rows { get; set; }
}

/// <summary>
///     строк расходная накладная
/// </summary>
public interface IConsumableWarehouseInvoiceRow
{
    INomenkl Nomenkl { get; set; }
    decimal Quantity { set; get; }
    bool IsTaxExecuted { set; get; }
    ISDRSchet SDRSchet { set; get; }
    IInvoiceClientRow InvoiceProviderRow { set; get; }
}
