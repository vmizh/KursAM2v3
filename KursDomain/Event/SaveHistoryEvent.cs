using Prism.Events;

namespace KursDomain.Event;

public class SaveHistoryEvent<IHistory> : PubSubEvent<SaveHistoryEventArgs<IHistory>>
{
}

public class SaveHistoryEventArgs<IHistory>
{
    public IHistory history { get; set; }
}
