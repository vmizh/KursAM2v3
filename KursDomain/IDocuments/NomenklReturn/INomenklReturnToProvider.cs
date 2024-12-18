using System;
using System.Collections.ObjectModel;
using Data;
using KursDomain.References;

namespace KursDomain.IDocuments.NomenklReturn;

public interface INomenklReturnToProvider
{
    public Guid Id { get; set; }
    public int DocNum { get; set; }
    public string DocExtNum { get; set; }
    public DateTime DocDate { get; set; }
    public decimal KontregentDC { get; set; }
    public Currency Currency { get; }
    public decimal Summa { get; }
    public decimal? InvoiceProviderDC { get; set; }
    public string Note { get; set; }
    public ObservableCollection<INomenklReturnToProviderRow> Rows { set; get; }
    public NomenklReturnToProvider Entity { set; get; }

}
