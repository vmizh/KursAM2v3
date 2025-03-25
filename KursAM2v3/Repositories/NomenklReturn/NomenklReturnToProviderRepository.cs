using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data;
using KursAM2.ViewModel.Logistiks.NomenklReturn.Helper;
using KursDomain;
using KursDomain.Documents.NomenklReturn;
using KursDomain.IDocuments.NomenklReturn;
using KursDomain.References;

namespace KursAM2.Repositories.NomenklReturn
{
    public class NomenklReturnToProviderRepository : INomenklReturnToProviderRepository
    {
        private readonly ALFAMEDIAEntities Context;
        private DbContextTransaction transaction;

        public NomenklReturnToProviderRepository(ALFAMEDIAEntities context)
        {
            Context = context;
        }

        public void AddOrUpdate(INomenklReturnToProvider entity, List<Guid> deletedIds)
        {
            if (entity is null || entity.Id == Guid.Empty) return;
            if (deletedIds != null && deletedIds.Any())
                foreach (var dId in deletedIds)
                {
                    var oldRow = Context.NomenklReturnToProviderRow.FirstOrDefault(_ => _.Id == dId);
                    if (oldRow != null) Context.NomenklReturnToProviderRow.Remove(oldRow);
                }

            var old = Context.NomenklReturnToProvider.FirstOrDefault(_ => _.Id == entity.Id);
            if (old != null)
                // ReSharper disable once RedundantAssignment
                old = App.Mapper.Map<NomenklReturnToProvider>(entity.Entity);
            else
                Context.NomenklReturnToProvider.Add(entity.Entity);

            Context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var d = Context.NomenklReturnToProvider.Include(_ => _.NomenklReturnToProviderRow)
                .FirstOrDefault(_ => _.Id == id);
            if (d is not null)
            {
                Context.NomenklReturnToProviderRow.RemoveRange(d.NomenklReturnToProviderRow);
                Context.NomenklReturnToProvider.Remove(d);
            }

            Context.SaveChanges();
        }

        public INomenklReturnToProvider Get(Guid id)
        {
            var entity = Context.NomenklReturnToProvider.Include(_ => _.NomenklReturnToProviderRow)
                .FirstOrDefault(_ => _.Id == id);
            return entity is null ? null : new NomenklReturnToProviderViewModel(entity);
        }

        public IEnumerable<INomenklReturnToProvider> GetAll()
        {
            var ret = new List<INomenklReturnToProvider>();

            var data = Context.NomenklReturnToProvider.Include(_ => _.NomenklReturnToProviderRow).ToList();
            ret.AddRange(data.Select(ent => new NomenklReturnToProviderViewModel(ent)));


            return ret;
        }

        public IEnumerable<INomenklReturnToProvider> GetForPeriod(DateTime start, DateTime end)
        {
            var ret = new List<INomenklReturnToProvider>();

            var data = Context.NomenklReturnToProvider.AsNoTracking().Include(_ => _.NomenklReturnToProviderRow)
                .Where(_ => _.DocDate >= start && _.DocDate <= end);
            foreach (var d in data)
            {
                var newItem = new NomenklReturnToProviderSearch
                {
                    Id = d.Id,
                    DocDate = d.DocDate,
                    DocExtNum = d.DocExtNum,
                    DocNum = d.DocNum,
                    Entity = d,
                    Note = d.Note,
                    Kontregent = GlobalOptions.ReferencesCache.GetKontragent(d.KontregentDC) as Kontragent,
                    Warehouse = GlobalOptions.ReferencesCache.GetWarehouse(d.WarehouseDC) as Warehouse,
                    SummaClient = d.NomenklReturnToProviderRow.Sum(_ => _.Price * _.Quantity),
                    SummaWarehouse = d.NomenklReturnToProviderRow.Sum(_ => (_.Cost ?? 0) * _.Quantity),
                    Creator = d.Creator
                };
                ret.Add(newItem);
            }


            return ret;
        }

        public NomenklReturnToProviderSearch GetSeacrh(Guid id)
        {
            var d = Context.NomenklReturnToProvider.Include(_ => _.NomenklReturnToProviderRow)
                .FirstOrDefault(_ => _.Id == id);
            if (d is null) return null;
            var ret = new NomenklReturnToProviderSearch
            {
                Id = d.Id,
                DocDate = d.DocDate,
                DocExtNum = d.DocExtNum,
                DocNum = d.DocNum,
                Entity = d,
                Note = d.Note,
                Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KontregentDC) as Kontragent,
                Warehouse = GlobalOptions.ReferencesCache.GetWarehouse(d.WarehouseDC) as Warehouse,
                SummaClient = d.NomenklReturnToProviderRow.Sum(_ => _.Price * _.Quantity),
                SummaWarehouse = d.NomenklReturnToProviderRow.Sum(_ => (_.Cost ?? 0) * _.Quantity),
                Creator = d.Creator
            };
            return ret;
        }

        public INomenklReturnToProvider CrateNewEmpty()
        {
            return new NomenklReturnToProviderViewModel
            {
                Id = Guid.NewGuid(),
                DocDate = DateTime.Today,
                Creator = GlobalOptions.UserInfo.NickName,
                DocNum = -1
            };
        }

        public INomenklReturnToProvider CrateNewRequisite(Guid id)
        {
            var old = Get(id);
            var ret = new NomenklReturnToProviderViewModel(new NomenklReturnToProvider())
            {
                Id = Guid.NewGuid(),
                DocDate = DateTime.Today,
                Creator = GlobalOptions.UserInfo.NickName,
                Note = old.Note,
                Kontragent = old.Kontragent,
                Warehouse = old.Warehouse
            };
            return ret;
        }

        public INomenklReturnToProvider CrateNewCopy(Guid id)
        {
            var old = Get(id);
            var ret = new NomenklReturnToProviderViewModel(new NomenklReturnToProvider())
            {
                Id = Guid.NewGuid(),
                DocDate = DateTime.Today,
                Creator = GlobalOptions.UserInfo.NickName,
                Note = old.Note,
                Kontragent = old.Kontragent,
                Warehouse = old.Warehouse
            };
            foreach (var row in old.Rows)
                ret.Rows.Add(new NomenklReturnToProviderRowViewModel(new NomenklReturnToProviderRow
                {
                    Id = Guid.NewGuid(),
                    DocId = ret.Id,
                    Cost = row.Cost,
                    InvoiceRowId = row.InvoiceRowId,
                    NomenklDC = row.NomenklDC,
                    Note = row.Note,
                    Price = row.Price,
                    Quantity = row.Quantity
                }, ret));

            return ret;
        }

        public List<PrhodOrderRow> GetPrihodOrderdRows(decimal kontrDC)
        {
            var data = Context.TD_24.Include(_ => _.SD_24)
                .Include(_ => _.TD_26).Include(_ => _.TD_26.SD_26).Include(td24 => td24.TD_84.SD_84).Where(_ => _.SD_24.DD_KONTR_OTPR_DC == kontrDC)
                .OrderBy(_ => _.SD_24.DD_DATE).ToList();
            if (data.Count == 0) return new List<PrhodOrderRow>();
            var retRows = Context.NomenklReturnToProviderRow.ToList();
            foreach (var rowId in retRows.Select(_ => _.PrihOrderId).Distinct())
            {
                var prih = data.First(_ => _.Id == rowId);
                {
                    var q = retRows.Where(_ => _.PrihOrderId == rowId).Sum(_ => _.Quantity);
                    if (prih.DDT_KOL_PRIHOD == q)
                        data.Remove(prih);
                    else prih.DDT_KOL_PRIHOD -= q;
                }
            }

            return data.Select(d => new PrhodOrderRow
                {
                    DocCode = d.DOC_CODE,
                    Code = d.CODE,
                    Id = d.Id,
                    PrihOrderDate = d.SD_24.DD_DATE,
                    PrihOrderCreator = d.SD_24.CREATOR,
                    PrihOrderNumber = $"{d.SD_24.DD_IN_NUM}" + (string.IsNullOrWhiteSpace(d.SD_24.DD_EXT_NUM)
                        ? null
                        : $"/{d.SD_24.DD_EXT_NUM}"),
                    PrihOrderNote = $"{d.DDT_NOTE} {d.SD_24.DD_NOTES}",
                    InvoiceDC = d.TD_26?.DOC_CODE,
                    InvoiceCode = d.TD_26?.CODE,
                    InvoiceRowId = d.TD_26?.Id,
                    InvoiceCreator = d.TD_26?.SD_26.CREATOR,
                    InvoiceDate = d.TD_26?.SD_26.SF_POSTAV_DATE,
                    InvoiceNote = $"{d.TD_26?.SFT_TEXT} {d.TD_26?.SD_26.SF_NOTES}",
                    InvoiceNumber = $"{d.TD_26?.SD_26.SF_IN_NUM}" + (string.IsNullOrWhiteSpace(d.TD_26?.SD_26.SF_POSTAV_NUM)
                        ? null
                        : $"/{d.TD_26.SD_26.SF_POSTAV_NUM}"),
                    Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(d.DDT_NOMENKL_DC) as Nomenkl,
                    Price = d.TD_26?.SFT_ED_CENA ?? 0,
                    Quantity = d.DDT_KOL_PRIHOD,
                    NomenklDC = d.DDT_NOMENKL_DC
                })
                .ToList();
        }
        

        public decimal GetNomenklLastPrice(decimal nomDC, DateTime date)
        {
            var last = Context.NOM_PRICE.LastOrDefault(_ => _.DATE <= date);
            return last?.PRICE_WO_NAKLAD ?? 0;
        }

        public IDictionary<decimal, decimal> GetNomenklLastPrices(List<decimal> nomDCs, DateTime date)
        {
            if (nomDCs == null || !nomDCs.Any()) return new Dictionary<decimal, decimal>();

            var ret = new Dictionary<decimal, decimal>();
            foreach (var dc in nomDCs)
            {
                var items = Context.NOM_PRICE.Where(_ => _.NOM_DC == dc && _.DATE <= date && _.PRICE_WO_NAKLAD > 0)
                    .OrderBy(_ => _.DATE).ToList();
                if (!items.Any())
                {
                    ret.Add(dc, 0);
                    continue;
                }

                ret.Add(dc, items.Last().PRICE_WO_NAKLAD);
            }

            return ret;
        }

        public void BeginTransaction()
        {
            transaction = Context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if(transaction is not null)
                transaction.Commit();
        }

        public void RollbackTransaction()
        {
            if(transaction is not null)
                transaction.Rollback();
        }

        public int GetNewNumber()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                return ctx.NomenklReturnToProvider.Any() ? ctx.NomenklReturnToProvider.Max(_ => _.DocNum) + 1 : 1;
            }
        }

        public void UndoChanges()
        {
            var changedEntries = Context.ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Unchanged).ToList();
            foreach (var entry in changedEntries)
            {
                switch(entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }
    }
}
