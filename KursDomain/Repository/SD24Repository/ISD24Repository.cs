using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using KursDomain.IDocuments;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.SD24Repository;

public interface ISD24Repository : IKursGenericRepository<SD_24, decimal>
{
    Task<IEnumerable<SD_24>> SearchAsync(MaterialDocumentTypeEnum materialDocumentType);
    Task<IEnumerable<SD_24>> SearchAsync(List<MaterialDocumentTypeEnum> materialDocumentTypes);
    Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd);

    Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd,
        MaterialDocumentTypeEnum materialDocumentType);

    Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd,
        List<MaterialDocumentTypeEnum> materialDocumentTypes);

    Task<IEnumerable<SD_24>> SearchAsync(MaterialDocumentTypeEnum materialDocumentType, string pattern);
    Task<IEnumerable<SD_24>> SearchAsync(List<MaterialDocumentTypeEnum> materialDocumentTypes, string pattern);
    Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd, string pattern);

    Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd,
        MaterialDocumentTypeEnum materialDocumentType, string pattern);

    Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd,
        List<MaterialDocumentTypeEnum> materialDocumentTypes, string pattern);

    Task<SD_24> GetDocumentAsync(decimal dc);
    Task<SD_24> GetDocumentAsync(Guid id);

    SD_24 GetDocument(decimal dc);
    SD_24 GetDocument(Guid id);

    SD_24 CreateNew();
    SD_24 CreateCopy(SD_24 ent);
    SD_24 CreateCopy(Guid id);
    SD_24 CreateRequisiteCopy(SD_24 ent);
    SD_24 CreateRequisiteCopy(Guid id);
    SD_24 CreateRequisiteCopy(decimal dc);



}
