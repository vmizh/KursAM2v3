using Prism.Events;
using System;

namespace KursDomain.Event;

public class AfterAddNewBaseWrapperEvent<T> : PubSubEvent<AfterAddNewBaseWrapperEventArgs<T>>
{
}

public class AfterAddNewBaseWrapperEventArgs<T>
{
    public Guid Id { get; set; }
    public decimal DocCode { get; set; }
}
