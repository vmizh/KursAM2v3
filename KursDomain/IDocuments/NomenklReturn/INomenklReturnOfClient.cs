using System;
using System.Collections.ObjectModel;
using Data;

namespace KursDomain.IDocuments.NomenklReturn;

public interface INomenklReturnOfClient
{
    public Guid Id { get; set; }
    public int DocNum { get; set; }
    public string DocExtNum { get; set; }
    public DateTime DocDate { get; set; }
    public decimal KontragentDC { get; set; }
    public decimal? InvoiceClientDC { get; set; }
    public string Note { get; set; }

    public ObservableCollection<INomenklReturnOfClientRow> Rows { set; get; }

    public NomenklReturnOfClient Entity { set; get; }
}
