using System;

namespace Core.ViewModel.Base
{
    public interface IDocumentOpenType
    {
        Guid? Id { set; get; }
        decimal? DocCode { set; get; }
        DocumentCreateTypeEnum OpenType { set; get; }
    }

    public class DocumentOpenType : IDocumentOpenType
    {
        public Guid? Id { get; set; }
        public decimal? DocCode { get; set; }
        public DocumentCreateTypeEnum OpenType { get; set; }
    }
}