using KursDomain.IDocuments;

namespace KursDomain.Services.CreateNewDocumentServices;

public interface ICreateNewDocument<T> where T : class
{
    T NewDocument();
    T NewDocument(MaterialDocumentTypeEnum docType);
    T  NewCopyDocument(T oldDocument);
    T  NewRequisiteDocument(T oldDocument);
}
