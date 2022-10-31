using System;
using System.Collections.Generic;
using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;

namespace KursDomain.IDocuments.WarehouseOrder;

/// <summary>
///     Приходный складской ордер
/// </summary>
public interface IIncomingWarehouseOrder
{
    int OrderNumber { get; set; }
    string OuterNumber { get; set; }
    DateTime OrderDate { get; set; }
    string Creator { get; set; }
    SenderType SenderType { get; set; }
    IKontragent SenderKontragent { get; set; }
    IWarehouse SenderWarehouse { get; set; }
    IInvoiceProvider InvoiceProvider { set; get; }
    IWarehouse Warehouse { set; get; }

    IEnumerable<IIncomingWarehouseOrderRow> Rows { get; set; }
}
