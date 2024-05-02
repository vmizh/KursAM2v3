using System;
using System.Collections.Generic;
using System.Diagnostics;
using KursDomain.ICommon;
using KursDomain.IDocuments.Finance;
using KursDomain.IDocuments.WarehouseOrder;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.Documents.Warehouse;

[DebuggerDisplay(
    "{DocCode,nq} {Code,nq} Номенклатура: {Nomenkl,nq} {Quantity,nq} ")]
public class IncomingWarehouseOrderRow : IDescription, IRowDC, IRowId, IIncomingWarehouseOrderRow, 
    IEqualityComparer<IncomingWarehouseOrderRow>
{
    public decimal DocCode { get; set; }
    public int Code { get; set; }
    public Guid Id { get; set; }
    public Guid DocId { get; set; }
    public string NomenklNumber => Nomenkl?.NomenklNumber;
    public string Notes { get; set; }
    public string Description => $"{NomenklNumber} {Nomenkl} {Quantity:n2}";
    public INomenkl Nomenkl { get; set; }
    public decimal Quantity { get; set; }
    public bool IsTaxExecuted { get; set; }
    public ISDRSchet SDRSchet { get; set; }
    public IInvoiceProviderRow InvoiceProviderRow { get; set; }
    public IWarehouseOrderOutRow IssueOrderRow { get; set; }
    

    public override string ToString()
    {
        return Description;
    }

    public bool Equals(IncomingWarehouseOrderRow x, IncomingWarehouseOrderRow y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DocCode == y.DocCode && x.Code == y.Code;
    }

    public int GetHashCode(IncomingWarehouseOrderRow obj)
    {
        unchecked
        {
            return (obj.DocCode.GetHashCode() * 397) ^ obj.Code;
        }
    }

    
}
