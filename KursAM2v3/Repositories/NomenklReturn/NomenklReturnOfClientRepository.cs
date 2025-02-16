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
    public class NomenklReturnOfClientRepository : INomenklReturnOfClientRepository
    {
        private readonly ALFAMEDIAEntities Context;
        private DbContextTransaction transaction;

        public NomenklReturnOfClientRepository(ALFAMEDIAEntities context)
        {
            Context = context;
        }

        public void AddOrUpdate(INomenklReturnOfClient entity, List<Guid> deletedIds)
        {
            if (entity is null || entity.Id == Guid.Empty) return;
            if (deletedIds != null && deletedIds.Any())
            {
                foreach (var dId in deletedIds)
                {
                    var oldRow = Context.NomenklReturnOfClientRow.FirstOrDefault(_ => _.Id == dId);
                    if (oldRow != null)
                    {
                        Context.NomenklReturnOfClientRow.Remove(oldRow);
                    }
                }
            }
            var old = Context.NomenklReturnOfClient.FirstOrDefault(_ => _.Id == entity.Id);
            if (old != null)
                // ReSharper disable once RedundantAssignment
                old = App.Mapper.Map<NomenklReturnOfClient>(entity);
            else
                Context.NomenklReturnOfClient.Add(entity.Entity);

            Context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var d = Context.NomenklReturnOfClient.FirstOrDefault(_ => _.Id == id);
            if (d is not null)
                Context.NomenklReturnOfClient.Remove(d);
            Context.SaveChanges();
        }

        public INomenklReturnOfClient Get(Guid id)
        {
            var entity = Context.NomenklReturnOfClient.Include(_ => _.NomenklReturnOfClientRow)
                .FirstOrDefault(_ => _.Id == id);
            return entity is null ? null : new NomenklReturnOfClientViewModel(entity);
        }

        public NomenklReturnOfClientSearch GetSeacrh(Guid id)
        {
            var d = Context.NomenklReturnOfClient.Include(_ => _.NomenklReturnOfClientRow)
                .FirstOrDefault(_ => _.Id == id);
            if (d is null) return null;
            var ret = new NomenklReturnOfClientSearch
            {
                Id = d.Id,
                DocDate = d.DocDate,
                DocExtNum = d.DocExtNum,
                DocNum = d.DocNum,
                Entity = d,
                Note = d.Note,
                Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent,
                Warehouse = GlobalOptions.ReferencesCache.GetWarehouse(d.WarehouseDC) as Warehouse,
                SummaClient = d.NomenklReturnOfClientRow.Sum(_ => _.Price * _.Quantity),
                SummaWarehouse = d.NomenklReturnOfClientRow.Sum(_ => _.Cost * _.Quantity),
                Creator = d.Creator
            };
            return ret;
        }

        public IEnumerable<INomenklReturnOfClient> GetAll()
        {
            var ret = new List<INomenklReturnOfClient>();

            var data = Context.NomenklReturnOfClient.Include(_ => _.NomenklReturnOfClientRow).ToList();
            ret.AddRange(data.Select(ent => new NomenklReturnOfClientViewModel(ent)));


            return ret;
        }

        public IEnumerable<INomenklReturnOfClient> GetForPeriod(DateTime start, DateTime end)
        {
            var ret = new List<INomenklReturnOfClient>();

            var data = Context.NomenklReturnOfClient.AsNoTracking().Include(_ => _.NomenklReturnOfClientRow)
                .Where(_ => _.DocDate >= start && _.DocDate <= end);
            foreach (var d in data)
            {
                var newItem = new NomenklReturnOfClientSearch
                {
                    Id = d.Id,
                    DocDate = d.DocDate,
                    DocExtNum = d.DocExtNum,
                    DocNum = d.DocNum,
                    Entity = d,
                    Note = d.Note,
                    Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent,
                    Warehouse = GlobalOptions.ReferencesCache.GetWarehouse(d.WarehouseDC) as Warehouse,
                    SummaClient = d.NomenklReturnOfClientRow.Sum(_ => _.Price * _.Quantity),
                    SummaWarehouse = d.NomenklReturnOfClientRow.Sum(_ => _.Cost * _.Quantity),
                    Creator = d.Creator
                };
                ret.Add(newItem);
            }


            return ret;
        }

        public INomenklReturnOfClient CrateNewEmpty()
        {
            return new NomenklReturnOfClientViewModel
            {
                Id = Guid.NewGuid(),
                DocDate = DateTime.Today,
                Creator = GlobalOptions.UserInfo.NickName,
                DocNum = -1
            };
        }

        public INomenklReturnOfClient CrateNewRequisite(Guid id)
        {
            var old = Get(id);
            var ret = new NomenklReturnOfClientViewModel(new NomenklReturnOfClient())
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

        public INomenklReturnOfClient CrateNewCopy(Guid id)
        {
            var old = Get(id);
            var ret = new NomenklReturnOfClientViewModel(new NomenklReturnOfClient())
            {
                Id = Guid.NewGuid(),
                DocDate = DateTime.Today,
                Creator = GlobalOptions.UserInfo.NickName,
                Note = old.Note,
                Kontragent = old.Kontragent,
                Warehouse = old.Warehouse
            };
            foreach (var row in old.Rows)
                ret.Rows.Add(new NomenklReturnOfClientRowViewModel(new NomenklReturnOfClientRow
                {
                    Id = new Guid(),
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

        public List<RashodNakladRow> GetRashodNakladRows(decimal kontrDC)
        {
            var data = Context.TD_24.Include(_ => _.SD_24)
                .Include(_ => _.TD_84).Include(_ => _.TD_84.SD_84).Where(_ => _.SD_24.DD_KONTR_POL_DC == kontrDC)
                .OrderBy(_ => _.SD_24.DD_DATE).ToList();

            return data.Select(d => new RashodNakladRow
                {
                    DocCode = d.DOC_CODE,
                    Code = d.CODE,
                    Id = d.Id,
                    WayBillDate = d.SD_24.DD_DATE,
                    WayBillCreator = d.SD_24.CREATOR,
                    WayBillNumber = $"{d.SD_24.DD_IN_NUM}" + (string.IsNullOrWhiteSpace(d.SD_24.DD_EXT_NUM)
                        ? null
                        : $"/{d.SD_24.DD_EXT_NUM}"),
                    WaybillNote = $"{d.DDT_NOTE} {d.SD_24.DD_NOTES}",
                    InvoiceDC = d.TD_84?.DOC_CODE,
                    InvoiceCode = d.TD_84?.CODE,
                    InvoiceRowId = d.TD_84?.Id,
                    InvoiceCreator = d.TD_84?.SD_84.CREATOR,
                    InvoiceDate = d.TD_84?.SD_84.SF_DATE,
                    InvoiceNote = $"{d.TD_84?.SFT_TEXT} {d.TD_84?.SD_84.SF_NOTE}",
                    InvoiceNumber = $"{d.TD_84?.SD_84.SF_IN_NUM}" + (string.IsNullOrWhiteSpace(d.TD_84?.SD_84.SF_OUT_NUM)
                        ? null
                        : $"/{d.TD_84.SD_84.SF_OUT_NUM}"),
                    Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(d.DDT_NOMENKL_DC) as Nomenkl,
                    Price = d.TD_84?.SFT_ED_CENA ?? 0,
                    Quantity = d.DDT_KOL_RASHOD,
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
                return ctx.NomenklReturnOfClient.Any() ? ctx.NomenklReturnOfClient.Max(_ => _.DocNum) + 1 : 1;
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
