using Prism.Events;

namespace KursDomain.Event;

public class SaveLastDocumentEvent<ILastDocument> : PubSubEvent<SaveLastDocumentEventArgs<ILastDocument>>
{
}

public class SaveLastDocumentEventArgs<ILastDocument>
{
    public ILastDocument info { get; set; }
}

public class DeleteLastDocumentEvent<ILastDocument> : PubSubEvent<DeleteLastDocumentEventArgs<ILastDocument>>
{
}

public class DeleteLastDocumentEventArgs<ILastDocument>
{
    public ILastDocument info { get; set; }
}
