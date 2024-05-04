using KursDomain.Documents.CommonReferences;
using KursDomain.Wrapper.Base;
using Prism.Events;
using System;

namespace KursDomain.Event
{
    public class AFterOpenBaseWrapperEvent<T> : PubSubEvent<AFterOpenBaseWrapperEventArgs<T>>
    {
    }

    public class AFterOpenBaseWrapperEventArgs<T>
    {
        public Guid Id { get; set; }
        public decimal DocCode { get; set; }
        public DocumentType DocumentType { get; set; } = DocumentType.None;
        public BaseWrapper<T> wrapper { get; set; }
    }

   
}
