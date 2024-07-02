using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using KursDomain.Documents.CommonReferences;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.DocHistoryRepository;

public interface IDocHistoryRepository : IKursGenericRepository<DocHistory, Guid>
{
    Dictionary<decimal, Tuple<string, DateTime>> GetLastChanges(IEnumerable<decimal> dc_list);
}
