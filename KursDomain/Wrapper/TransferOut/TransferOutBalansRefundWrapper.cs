using System;
using KursDomain.IDocuments.TransferOut;
using KursDomain.References;
using KursDomain.Wrapper.Base;

namespace KursDomain.Wrapper.TransferOut;

public class TransferOutBalansRefundWrapper : BaseWrapper<ITransferOutBalansRefundRows>, ITransferOutBalansRefundRows,
    IEquatable<TransferOutBalansRefundWrapper>
{
    public TransferOutBalansRefundWrapper(ITransferOutBalansRefundRows model) : base(model)
    {
    }

    public bool Equals(TransferOutBalansRefundWrapper other)
    {
        throw new NotImplementedException();
    }

    public Guid DocId { get; set; }
    public Nomenkl Nomenkl { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public ITransferOutBalansRefund TransferOutBalansRefund { get; set; }
}
