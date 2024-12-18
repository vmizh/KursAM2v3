using System;
using System.Collections.Generic;
using Data;
using KursDomain.IDocuments.NomenklReturn;

namespace KursAM2.Repositories.NomenklReturn
{
    public interface INomenklReturnToProviderRepository
    {
        public void AddOrUpdate(INomenklReturnToProvider entity);
        public void Delete(Guid id);
        public NomenklReturnOfClient Get(Guid id);
        public IEnumerable<INomenklReturnToProvider> GetAll();
        public IEnumerable<INomenklReturnToProvider> GetForPeriod(DateTime start, DateTime end);
    }
}
