using System;
using System.Threading.Tasks;
using Core.WindowsManager;
using Data;
using Helper;
using KursDomain.Documents.CommonReferences;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.DocHistoryRepository;

public class DocHistoryRepository : KursGenericRepository<DocHistory, ALFAMEDIAEntities, Guid>, IDocHistoryRepository
{
    public DocHistoryRepository(ALFAMEDIAEntities context) : base(context)
    {
    }

  
}
