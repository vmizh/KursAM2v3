using System;
using KursDomain.Wrapper.Base;
using Prism.Events;

namespace KursAM2.Event
{
    public class AFterSaveBaseWrapperEvent<T> : PubSubEvent<AFterSaveBaseWrapperEventArgs<T>>
    {
    }

    public class AFterSaveBaseWrapperEventArgs<T>
    {
        public Guid Id { get; set; }
        public decimal DocCode { get; set; }
        public BaseWrapper<T> wrapper { get; set; }
    }
}
