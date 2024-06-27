using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using KursDomain;

namespace KursAM2.Repositories.LastDocumentRepository
{
    public class LastDocumentRepository : ILastDocumentRepository
    {
        private const int pageSize = 200;
        public LastDocumentRepository()
        {
            Context = GlobalOptions.GetEntities();
        }

        public LastDocumentRepository(ALFAMEDIAEntities context)
        {
            Context = context;
        }

        public ALFAMEDIAEntities Context { get; }

        public Dictionary<decimal, Tuple<string, DateTime>> GetLastChanges(IEnumerable<decimal> dc_list)
        {
            var dcList = dc_list.ToList();
           if (!dcList.Any()) return new Dictionary<decimal, Tuple<string, DateTime>>();

            Dictionary<decimal, Tuple<string, DateTime>> ret = new Dictionary<decimal, Tuple<string, DateTime>>();

            List<List<decimal>> pages = new List<List<decimal>>();
            var cnt = dcList.Count() / pageSize;
            for (int i = 0; i <= cnt; i++)
            {
                pages.Add(dcList.Skip(i * pageSize).Take(pageSize).ToList());
            }

            foreach (var page in pages)
            {
                var data = Context.DocHistory.Where(_ => _.DocDC != null
                                                         && page.Contains(_.DocDC.Value))
                    .ToList();
                // ReSharper disable once PossibleInvalidOperationException
                List<decimal> dcs = new List<decimal>(data.Select(_ => _.DocDC.Value).Distinct());
                foreach (var dc in dcs)
                {
                    var r = data.Where(_ => _.DocDC == dc).OrderByDescending(_ => _.Date).First();
                    ret.Add(dc, new Tuple<string, DateTime>(r.UserName, r.Date));
                }
            }

            return ret;

        }
    }

    public interface ILastDocumentRepository
    {
        ALFAMEDIAEntities Context { get; }

        Dictionary<decimal, Tuple<string, DateTime>> GetLastChanges(IEnumerable<decimal> dc_list);
    }
}
