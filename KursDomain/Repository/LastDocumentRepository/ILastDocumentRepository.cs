using Data;
using KursDomain.Repository.Base;
using System;
using System.Threading.Tasks;
using KursDomain.Documents.CommonReferences;


namespace KursDomain.Repository.LastDocumentRepository;

public interface ILastDocumentRepository :  IKursGenericRepository<LastDocument, Guid>
{
    Task SaveLastOpenInfoAsync(DocumentType docType, Guid? docId, decimal? docDC,
        string creator, string lastChanger,
        string desc);
}
