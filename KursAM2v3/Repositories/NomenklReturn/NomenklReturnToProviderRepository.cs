using System;
using System.Collections.Generic;
using Data;
using KursDomain.IDocuments.NomenklReturn;

namespace KursAM2.Repositories.NomenklReturn
{
    public class NomenklReturnToProviderRepository : INomenklReturnToProviderRepository
    {
        public void AddOrUpdate(INomenklReturnToProvider entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public NomenklReturnOfClient Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<INomenklReturnToProvider> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<INomenklReturnToProvider> GetForPeriod(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }
    }
}
