using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DevExpress.Xpf.Core.Native;
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
                var old = ctx.NomenklReturnOfClientRow.FirstOrDefault(_ => _.Id == entity.Id);
                if (old != null)
                {
                    //old.Cost = 
                }
                else
                {
                    
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
            throw new NotImplementedException();
        }

        public IEnumerable<INomenklReturnOfClient> GetAll()
        {
            throw new NotImplementedException();
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
    }
}
