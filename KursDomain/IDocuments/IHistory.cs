using System;

namespace KursDomain.IDocuments;

public interface IHistory
{
    string DocType { set; get; }
    Guid? DocId { set; get; }
    decimal? DocDC { set; get; } 
    int? Code { set; get; }
    string Json { set; get; }
}

public class HistoryInfo : IHistory
{
    public string DocType { get; set; }
    public Guid? DocId { get; set; }
    public decimal? DocDC { get; set; }
    public int? Code { get; set; }
    public string Json { get; set; }
}
