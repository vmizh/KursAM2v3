using KursDomain.Documents.CommonReferences;
using KursDomain.Wrapper.Base;
using Prism.Events;
using System;

namespace KursDomain.Event
{
    public class AfterUpdateBaseWrapperEvent<T> : PubSubEvent<AfterUpdateBaseWrapperEventArgs<T>>
    {
    }

    public class AfterUpdateBaseWrapperEventArgs<T>
    {
        public Guid Id { get; set; }
        public decimal DocCode { get; set; }
        public DocumentType DocumentType { get; set; } = DocumentType.None;
        public object wrapper { get; set; }
        public string FieldName { get; set; }


    }
}
