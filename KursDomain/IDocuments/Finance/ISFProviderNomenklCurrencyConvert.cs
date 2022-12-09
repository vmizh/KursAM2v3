using System;
using System.Collections.Generic;
using KursDomain.IDocuments.DistributeOverhead;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.IDocuments.Finance;

public interface ISFProviderNomenklCurrencyConvert
{
    Guid Id { get; set; }
    INomenkl NomenklFrom { set; get; }
    INomenkl NomenklTo { set; get;}
    DateTime Date { get; set; }
    decimal Quantity { get; set; }
    decimal Rate { get; set; }
    decimal FromPrice { get; set; }
    decimal FromPriceWithNaklad { get; set; }
    decimal Price { get; set; }
    decimal PriceWithNaklad { get; set; }
    decimal FromSumma { get; }
    decimal FromSummaWithNaklad { get;  }
    decimal Summa { get;  }
    decimal SummaWithNaklad { get;  }
    decimal SFDocCode { get; set; }
    int SFCode { get; set; }
    IWarehouse Warehouse { get; set; }
    IEnumerable<IDistributeNakladRow> NakladRows { set; get; }
    string Description { get; }

    IInvoiceProviderRow InvoiceProviderRow { set; get; }
}
