using Core;
using Data;
using KursDomain.Repository;
using KursDomain;

namespace Auxiliary;

public class GenerateHistory
{
    public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
        // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
        new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

    public void SaveHistory()
    {
        //DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.InvoiceProvider), null,
        //    Document.DocCode, null, (string)Document.ToJson());
    }
}
