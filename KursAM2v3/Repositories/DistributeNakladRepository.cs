using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core.Repository.Base;
using Data;

namespace KursAM2.Repositories
{
    public interface IDistributeNakladRepository
    {
        DistributeNaklad GetById(Guid id);
        List<DistributeNaklad> GetAllByDates(DateTime dateStart, DateTime dateEnd);

    }
    public class DistributeNakladRepository : GenericKursRepository<DistributeNaklad>, IDistributeNakladRepository
    {
        public DistributeNakladRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public DistributeNakladRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public DistributeNaklad GetById(Guid id)
        {
            return Context.DistributeNaklad.Include(_ => _.DistributeNakladRow)
                .Include(_ => _.DistributeNakladRow.Select(x => x.DistributeNakladInfo))
                .SingleOrDefault(_ => _.Id == id);
        }

        public List<DistributeNaklad> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            return Context.DistributeNaklad.Include(_ => _.DistributeNakladRow)
                .Include(_ => _.DistributeNakladRow.Select(x => x.DistributeNakladInfo))
                .Where(_ => _.DocDate >= dateStart && _.DocDate <= dateEnd).ToList();
        }
    }
}