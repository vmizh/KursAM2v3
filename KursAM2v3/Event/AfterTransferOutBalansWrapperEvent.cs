using Prism.Events;
using System;
using KursDomain.Wrapper.TransferOut;

namespace KursAM2.Event
{
    public class AfterTransferOutBalansWrapperEvent : PubSubEvent<AfterTransferOutBalansWrapperEventArgs>
    {
    }

    public class  AfterTransferOutBalansWrapperEventArgs
    {
        public Guid Id { get; set; }
        public decimal DocCode { get; set; }
        public EnumAfterSaveOperation Operation { set; get; }
        public TransferOutBalansWrapper Document { get; set; }
    }
}
