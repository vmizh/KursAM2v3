using System;
using KursDomain.Documents.CommonReferences;

namespace KursDomain.IDocuments;

public interface ILastDocument
{
    DocumentType DocType { set; get; }
    Guid? DocId { set; get; } 
    decimal? DocDC { set; get; }
    string Creator { set; get; } 
    string LastChanger { set; get; }
    string Desc { set; get; }
}

public class LastDocumentInfo : ILastDocument
{
    public DocumentType DocType { get; set; }
    public Guid? DocId { get; set; }
    public decimal? DocDC { get; set; }
    public string Creator { get; set; }
    public string LastChanger { get; set; }
    public string Desc { get; set; }
}
