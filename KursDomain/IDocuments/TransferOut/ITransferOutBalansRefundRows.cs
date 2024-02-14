using System;
using KursDomain.References;

namespace KursDomain.IDocuments.TransferOut;

public interface ITransferOutBalansRefundRows
{
    public Guid Id { get; set; }
    public Guid DocId { get; set; }
    public Nomenkl Nomenkl { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public string Note { get; set; }

    public ITransferOutBalansRefund TransferOutBalansRefund { get; set; }
}
