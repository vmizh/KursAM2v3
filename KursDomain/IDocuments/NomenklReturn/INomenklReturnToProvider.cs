using System;
using System.Collections.ObjectModel;
using Data;
using KursDomain.Documents.NomenklReturn;
using KursDomain.References;

namespace KursDomain.IDocuments.NomenklReturn;

public interface INomenklReturnToProvider
{
    public Guid Id { get; set; }
    public int DocNum { get; set; }
    public string DocExtNum { get; set; }
    public DateTime DocDate { get; set; }
    public Kontragent Kontragent { get; set; }
    public Warehouse Warehouse { set; get; }
    public Currency Currency { get; }
    public decimal SummaWarehouse { get; set; }
    public decimal SummaClient { get; set; }
    public decimal? PrihOrderDC { get; set; }
    public decimal? InvoiceProviderDC { get; set; }
    public string Note { get; set; }
    public ObservableCollection<NomenklReturnToProviderRowViewModel> Rows { set; get; }
    public NomenklReturnToProvider Entity { set; get; }

}
