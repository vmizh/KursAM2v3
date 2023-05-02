using System;
using System.Collections.Generic;
using System.Linq;
using KursDomain.Documents.NomenklManagement;
using KursDomain.IDocuments.Refund;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.Documents.Refund;

public class RefundFromClientSimple : IRefundFromClient
{
    public Guid Id { get; set; }
    public int Num { get; set; }
    public DateTime Date { get; set; }
    public IWarehouse Warehouse { get; set; }
    public decimal Summa => Rows.Sum(_ => _.FactPrice * _.Quantity);
    public IKontragent Client { get; set; }
    public string Note { get; set; }
    public string Creator { get; set; }
    public IEnumerable<IRefundFromClientRow> Rows { get; } = new List<IRefundFromClientRow>();
}

public class RefundFromClientRowSimple : IRefundFromClientRow
{
    public Guid Id { get; set; }
    public Guid DocId { get; set; }
    public INomenkl Nomenkl { get; set; }
    public decimal Quantity { get; set; }
    public WaybillRow WaybillRow { get; set; }
    public decimal FactPrice { get; set; }
    public string Note { get; set; }
    public IRefundFromClient Parent { get; set; }
}


