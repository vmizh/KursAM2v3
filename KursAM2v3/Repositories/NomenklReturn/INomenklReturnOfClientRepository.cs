using System;
using System.Collections.Generic;
using KursDomain.IDocuments.NomenklReturn;

namespace KursAM2.Repositories.NomenklReturn
{
    public interface INomenklReturnOfClientRepository
    {
        public void AddOrUpdate(INomenklReturnOfClient entity);
        public void Delete(Guid id);
        public INomenklReturnOfClient Get(Guid id);
        public IEnumerable<INomenklReturnOfClient> GetAll();
        public IEnumerable<INomenklReturnOfClient> GetForPeriod(DateTime start, DateTime end);

        public INomenklReturnOfClient CrateNewEmpty();
        public INomenklReturnOfClient CrateNewRequisite(Guid id);
        public INomenklReturnOfClient CrateNewCopy(Guid id);
    }
}
