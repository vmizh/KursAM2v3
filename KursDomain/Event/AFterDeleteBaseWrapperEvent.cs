using Prism.Events;
using System;

namespace KursDomain.Event;

public class AFterDeleteBaseWrapperEvent<T> : PubSubEvent<AFterDeleteBaseWrapperEventArgs<T>>
{
}

public class AFterDeleteBaseWrapperEventArgs<T>
{
    public Guid Id { get; set; }
    public decimal DocCode { get; set; }
}
