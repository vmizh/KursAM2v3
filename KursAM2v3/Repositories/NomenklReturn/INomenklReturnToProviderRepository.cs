using System;
using System.Collections.Generic;
using Data;
using KursAM2.ViewModel.Logistiks.NomenklReturn.Helper;
using KursDomain.Documents.NomenklReturn;
using KursDomain.IDocuments.NomenklReturn;

namespace KursAM2.Repositories.NomenklReturn
{
    public interface INomenklReturnToProviderRepository
    {
        public void AddOrUpdate(INomenklReturnToProvider entity, List<Guid> deletedIds);
        public void Delete(Guid id);
        public INomenklReturnToProvider Get(Guid id);
        public IEnumerable<INomenklReturnToProvider> GetAll();
        public IEnumerable<INomenklReturnToProvider> GetForPeriod(DateTime start, DateTime end);
        public NomenklReturnToProviderSearch GetSeacrh(Guid id);

        public INomenklReturnToProvider CrateNewEmpty();
        public INomenklReturnToProvider CrateNewRequisite(Guid id);
        public INomenklReturnToProvider CrateNewCopy(Guid id);

        public List<PrhodOrderRow> GetPrihodOrderdRows(decimal kontrDC);

        public decimal GetNomenklLastPrice(decimal nomDC, DateTime date);
        public IDictionary<decimal, decimal> GetNomenklLastPrices(List<decimal> nomDCs, DateTime date);

        public void BeginTransaction();
        public void CommitTransaction();
        public void RollbackTransaction();
        public int GetNewNumber();

        public void UndoChanges();
    }
}
