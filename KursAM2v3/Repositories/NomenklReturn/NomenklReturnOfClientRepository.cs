using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data;
using KursDomain;
using KursDomain.Documents.NomenklReturn;
using KursDomain.IDocuments.NomenklReturn;

namespace KursAM2.Repositories.NomenklReturn
{
    public class NomenklReturnOfClientRepository : INomenklReturnOfClientRepository
    {
        public void AddOrUpdate(INomenklReturnOfClient entity)
        {
            if (entity is null || entity.Id == Guid.Empty) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var old = ctx.NomenklReturnOfClient.FirstOrDefault(_ => _.Id == entity.Id);
                if (old != null)
                {
                    // ReSharper disable once RedundantAssignment
                    old = App.Mapper.Map<NomenklReturnOfClient>(entity);
                }
                else
                {
                    ctx.NomenklReturnOfClient.Add(entity.Entity);
                }

                ctx.SaveChanges();
            }
            
        }

        public void Delete(Guid id)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var d = ctx.NomenklReturnOfClient.FirstOrDefault(_ => _.Id == id);
                if (d is not null)
                    ctx.NomenklReturnOfClient.Remove(d);
            }
        }

        public INomenklReturnOfClient Get(Guid id)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var entity = ctx.NomenklReturnOfClient.Include(_ => _.NomenklReturnOfClientRow).FirstOrDefault(_ => _.Id == id);
                return entity is null ? null : new NomenklReturnOfClientViewModel(entity);
            }
        }

        public IEnumerable<INomenklReturnOfClient> GetAll()
        {
            var ret = new List<INomenklReturnOfClient>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.NomenklReturnOfClient.Include(_ => _.NomenklReturnOfClientRow).ToList();
                ret.AddRange(data.Select(ent => new NomenklReturnOfClientViewModel(ent)));
            }

            return ret;
        }

        public IEnumerable<INomenklReturnOfClient> GetForPeriod(DateTime start, DateTime end)
        {
            var ret = new List<INomenklReturnOfClient>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.NomenklReturnOfClient.Include(_ => _.NomenklReturnOfClientRow)
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
                        InvoiceClientDC = d.InvoiceClientDC,
                        KontragentDC = d.KontragentDC,
                        Note = d.Note,
                        SummaClient = d.NomenklReturnOfClientRow.Sum(_ => _.Price * _.Quantity),
                        SummaWarehouse = d.NomenklReturnOfClientRow.Sum(_ => _.Cost * _.Quantity)
                    };
                    ret.Add(newItem);
                }
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
                KontragentDC = old.KontragentDC
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
                KontragentDC = old.KontragentDC,
                InvoiceClientDC = old.InvoiceClientDC,
            };
            foreach (var row in old.Rows)
            {
                ret.Rows.Add(new NomenklReturnOfClientRowViewModel(new NomenklReturnOfClientRow()
                {
                    Id = new Guid(),
                    DocId = ret.Id,
                    Cost = row.Cost,
                    InvoiceRowId = row.InvoiceRowId,
                    NomenklDC = row.NomenklDC,
                    Note = row.Note,
                    Price = row.Price,
                    Quantity = row.Quantity,
                }, ret));
                
            }
            return ret;
        }
    }
}
