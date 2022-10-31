using KursDomain.ICommon;

namespace KursDomain.Documents.Warehouse;

public class IssueWarehouseOrderRow : IDocumentState
{
    public RowStatus RowStatus { get; set; }
}
