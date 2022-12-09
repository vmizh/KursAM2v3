using System;
using System.Collections.Generic;
using DevExpress.Xpf.Bars;
using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;
using IInvoiceProviderRow = KursDomain.IDocuments.Finance.IInvoiceProviderRow;

namespace KursDomain.IDocuments.DistributeOverhead;

public interface IDistributeNakladRow
{
    Guid Id { set; get; }
    Guid DocId { set; get; }
    IInvoiceProviderRow InvoiceRow { set; get; }
    ISFProviderNomenklCurrencyConvert Convert { set; get; }

    string Note { set; get; }
    decimal Quantity { set; get; }
    decimal Price { set; get; }
    decimal Summa { set; get; }
    decimal DistributeSumma { set; get; }
    decimal DistributePrice { set; get; }
}

public interface IDistributeNaklad
{
    Guid Id { get; set; }
    DateTime DocDate { get; set; }
    string DocNum { get; set; }
    string Creator { set; get; }
    ICurrency Currency { set; get; }
    string Note { set; get; }
    IEnumerable<IDistributeNakladRow> Tovars { set; get; }
    IEnumerable<IDistributeNakladInvoice> NakladInvoices { set; get; }
}

public interface IDistributeNakladInvoice
{
    DistributeNakladTypeEnum DistributeType { set; get; }
}

public interface IDistributeNakladInfo
{
    
}
