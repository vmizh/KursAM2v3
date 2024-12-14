using System;
using Data;

namespace KursDomain.IDocuments.NomenklReturn;

public interface INomenklReturnOfClientRow
{
    public Guid Id { get; set; }
    public Guid DocId { get; set; }
    public decimal NomenklDC { get; set; }
    public Guid? InvoiceRowId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public string Note { get; set; }

    public NomenklReturnOfClientRow Entity { set; get; }
}
