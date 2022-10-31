using System.Collections.Generic;
using System.Diagnostics;
using KursDomain.ICommon;
using KursDomain.IDocuments;
using KursDomain.IDocuments.Finance;
using KursDomain.IDocuments.WarehouseOrder;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.Documents.Warehouse;

[DebuggerDisplay(
    "{DocCode,nq} {Code,nq} Номенклатура: {Nomenkl,nq} {Quantity,nq} ")]
public class IncomingWarehouseOrderRow : IDescription, IRowDC, IIncomingWarehouseOrderRow, IEqualityComparer<IRowDC>
{
    public string Notes { get; set; }
    public string Description => $"{NomenklNumber} {Nomenkl} {Quantity:n2}";

    public bool Equals(IRowDC x, IRowDC y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DocCode == y.DocCode && x.Code == y.Code;
    }

    public int GetHashCode(IRowDC obj)
    {
        unchecked
        {
            return (obj.DocCode.GetHashCode() * 397) ^ obj.Code;
        }
    }

    public INomenkl Nomenkl { get; set; }
    public string NomenklNumber => Nomenkl?.NomenklNumber;
    public decimal Quantity { get; set; }
    public bool IsTaxExecuted { get; set; }
    public ISDRSchet SDRSchet { get; set; }
    public IInvoiceProviderRow InvoiceProviderRow { get; set; }
    public IIssueWarehouseOrderRow IssueOrderRow { get; set; }
    public decimal DocCode { get; set; }
    public int Code { get; set; }

    public override string ToString()
    {
        return Description;
    }
}
