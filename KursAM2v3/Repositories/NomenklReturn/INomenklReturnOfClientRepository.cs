using System;
using System.Collections.Generic;
using Data;
using KursAM2.ViewModel.Logistiks.NomenklReturn.Helper;
using KursDomain.Documents.NomenklReturn;
using KursDomain.IDocuments.NomenklReturn;

namespace KursAM2.Repositories.NomenklReturn
{
    public interface INomenklReturnOfClientRepository
    {
        public void AddOrUpdate(INomenklReturnOfClient entity, List<Guid> deletedIds);
        public void Delete(Guid id);
        public INomenklReturnOfClient Get(Guid id);
        public NomenklReturnOfClientSearch GetSeacrh(Guid id);
        public IEnumerable<INomenklReturnOfClient> GetAll();
        public IEnumerable<INomenklReturnOfClient> GetForPeriod(DateTime start, DateTime end);

        public INomenklReturnOfClient CrateNewEmpty();
        public INomenklReturnOfClient CrateNewRequisite(Guid id);
        public INomenklReturnOfClient CrateNewCopy(Guid id);

        public List<RashodNakladRow> GetRashodNakladRows(decimal kontrDC);

        public decimal GetNomenklLastPrice(decimal nomDC, DateTime date);
        public IDictionary<decimal, decimal> GetNomenklLastPrices(List<decimal> nomDCs, DateTime date);

        public void BeginTransaction();
        public void CommitTransaction();
        public void RollbackTransaction();
        public int GetNewNumber();

        public void UndoChanges();
    
    }
}
