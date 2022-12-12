using System;
using System.Collections.Generic;
using System.Diagnostics;
using KursDomain.ICommon;
using KursDomain.IDocuments.Finance;
using KursDomain.IDocuments.WarehouseOrder;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;

namespace KursDomain.Documents.Warehouse;

/// <summary>
///     Приходный складской ордер
/// </summary>
[DebuggerDisplay(
    "{DocCode,nq} {OrderNumber,nq} Отправитель: {SenderKontragent,nq} {SenderWarehouse,nq} " +
    " от {OrderDate.ToShortDateString(),nq}")]
public class IncomingWarehouseOrder : IDocCode, IDocGuid, IDescription, IIncomingWarehouseOrder, 
    IEqualityComparer<IncomingWarehouseOrder>
{
    public string Name { get; set; }
    public string Notes { get; set; }

    public string Description
    {
        get
        {
            var num = "№:" + (string.IsNullOrWhiteSpace(OuterNumber)
                ? OrderNumber.ToString()
                : $"{OrderNumber}/{OuterNumber}");
            var sender = SenderWarehouse == null ? SenderKontragent.ToString() : SenderWarehouse.ToString();
            return
                $"Приходный складской ордер {num} от {OrderDate.ToShortDateString()} Получатель {sender} Создал {Creator}";
        }
    }

    public decimal DocCode { get; set; }
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public string OuterNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string Creator { get; set; }
    public SenderType SenderType { get; set; }
    public IKontragent SenderKontragent { get; set; }
    public IWarehouse SenderWarehouse { get; set; }
    public IInvoiceProvider InvoiceProvider { get; set; }
    public IWarehouse Warehouse { get; set; }
    public IEnumerable<IIncomingWarehouseOrderRow> Rows { get; set; } = new List<IIncomingWarehouseOrderRow>();

    public bool Equals(IncomingWarehouseOrder x, IncomingWarehouseOrder y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DocCode == y.DocCode;
    }

    public int GetHashCode(IncomingWarehouseOrder obj)
    {
        return obj.DocCode.GetHashCode();
    }

    
}
