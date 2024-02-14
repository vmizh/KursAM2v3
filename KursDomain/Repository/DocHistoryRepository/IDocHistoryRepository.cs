using System;
using System.Threading.Tasks;
using Data;
using KursDomain.Documents.CommonReferences;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.DocHistoryRepository;

public interface IDocHistoryRepository : IKursGenericRepository<DocHistory, Guid>
{
   
}
