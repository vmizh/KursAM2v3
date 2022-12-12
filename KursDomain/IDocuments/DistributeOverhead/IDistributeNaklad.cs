using System;
using System.Collections.Generic;
using KursDomain.IReferences;

namespace KursDomain.IDocuments.DistributeOverhead;

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
