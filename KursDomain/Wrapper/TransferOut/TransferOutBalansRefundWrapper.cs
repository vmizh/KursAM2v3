using System;
using KursDomain.ICommon;
using KursDomain.IDocuments.TransferOut;
using KursDomain.References;
using KursDomain.Wrapper.Base;
using Prism.Events;

namespace KursDomain.Wrapper.TransferOut;

public class TransferOutBalansRefundWrapper : BaseWrapper<ITransferOutBalansRefundRows>, ITransferOutBalansRefundRows,
    IEquatable<TransferOutBalansRefundWrapper>
{
    public TransferOutBalansRefundWrapper(ITransferOutBalansRefundRows model, IEventAggregator eventAggregator,
        IMessageDialogService messageDialogService) : base(model, eventAggregator, messageDialogService)
    {
    }

    public bool Equals(TransferOutBalansRefundWrapper other)
    {
        throw new NotImplementedException();
    }

    public Guid DocId { get; set; }
    public References.Nomenkl Nomenkl { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public ITransferOutBalansRefund TransferOutBalansRefund { get; set; }
}
