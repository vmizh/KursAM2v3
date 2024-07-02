using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.DocHistoryRepository;

public class DocHistoryRepository : KursGenericRepository<DocHistory, ALFAMEDIAEntities, Guid>, IDocHistoryRepository
{
    private const int pageSize = 200;

    public DocHistoryRepository(ALFAMEDIAEntities context) : base(context)
    {
    }


    public Dictionary<decimal, Tuple<string, DateTime>> GetLastChanges(IEnumerable<decimal> dc_list)
    {
        var dcList = dc_list.ToList();
        if (!dcList.Any()) return new Dictionary<decimal, Tuple<string, DateTime>>();

        var ret = new Dictionary<decimal, Tuple<string, DateTime>>();

        var pages = new List<List<decimal>>();
        var cnt = dcList.Count() / pageSize;
        for (var i = 0; i <= cnt; i++) pages.Add(dcList.Skip(i * pageSize).Take(pageSize).ToList());

        foreach (var page in pages)
        {
            var data = Context.DocHistory.Where(_ => _.DocDC != null
                                                     && page.Contains(_.DocDC.Value))
                .ToList();
            // ReSharper disable once PossibleInvalidOperationException
            var dcs = new List<decimal>(data.Select(_ => _.DocDC.Value).Distinct());
            foreach (var dc in dcs)
            {
                var r = data.Where(_ => _.DocDC == dc).OrderByDescending(_ => _.Date).First();
                ret.Add(dc, new Tuple<string, DateTime>(r.UserName, r.Date));
            }
        }

        return ret;
    }
}
