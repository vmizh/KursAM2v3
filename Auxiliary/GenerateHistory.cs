using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Data;
using Data.Repository;
using Helper;

namespace Auxiliary
{
    public class GenerateHistory
    {
        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        public void SaveHistory()
        {


            //DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.InvoiceProvider), null,
            //    Document.DocCode, null, (string)Document.ToJson());
        }
    }
}