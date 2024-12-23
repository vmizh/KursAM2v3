using System;
using System.Collections.ObjectModel;
using Data;
using KursDomain.Documents.NomenklReturn;
using KursDomain.References;

namespace KursDomain.IDocuments.NomenklReturn;

public interface INomenklReturnOfClient
{
    public Guid Id { get; set; }
    public int DocNum { get; set; }
    public string DocExtNum { get; set; }
    public DateTime DocDate { get; set; }
    public decimal KontragentDC { get; set; }
    public decimal? InvoiceClientDC { get; set; }
    public Currency Currency { get; }
    public decimal SummaWarehouse { get; set; }
    public decimal SummaClient { get; set; }
    public string Note { get; set; }
    public string Creator { set; get; }

    public ObservableCollection<NomenklReturnOfClientRowViewModel> Rows { set; get; }

    public NomenklReturnOfClient Entity { set; get; }
}
