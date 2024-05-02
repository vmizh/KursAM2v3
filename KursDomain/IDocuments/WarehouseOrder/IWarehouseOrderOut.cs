using System;
using System.Collections.Generic;
using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;

namespace KursDomain.IDocuments.WarehouseOrder;

/// <summary>
///     Расходный складской ордер
/// </summary>
public interface IWarehouseOrderOut
{
    int OrderNumber { get; set; }
    string OuterNumber { get; set; }
    DateTime OrderDate { get; set; }
    string Creator { get; set; }
    SenderType SenderType { get; set; }
    IKontragent RecepientKontragent { get; set; }
    IWarehouse RecepientWarehouse { get; set; }
    IInvoiceProvider InvoiceClient { set; get; }
    IWarehouse Warehouse { set; get; }

    IEnumerable<IWarehouseOrderOutRow> Rows { get; set; }
}
