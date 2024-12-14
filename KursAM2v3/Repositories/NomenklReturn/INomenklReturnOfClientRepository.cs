using System;
using System.Collections.Generic;
using Data;

namespace KursAM2.Repositories.NomenklReturn
{
    public interface INomenklReturnOfClientRepository
    {
        public void AddOrUpdate(NomenklReturnOfClient entity);
        public void Delete(Guid id);
        public NomenklReturnOfClient Get(Guid id);
        public IEnumerable<NomenklReturnOfClient> GetAll();
        public IEnumerable<NomenklReturnOfClient> GetForPeriod(DateTime start, DateTime end);
    }
}
