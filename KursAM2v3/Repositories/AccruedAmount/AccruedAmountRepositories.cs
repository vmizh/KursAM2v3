using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Repository;

namespace KursAM2.Repositories.AccruedAmount
{
    public interface IAccruedAmountForClientRepository
    {
        List<AccruedAmountForClient> GetByDate(DateTime startDate, DateTime endDate);
    }

    public interface IAccruedAmountOfSupplierRepository
    {
        List<AccruedAmountOfSupplier> GetByDate(DateTime startDate, DateTime endDate);
    }

    public class AccruedAmountOfSupplierRepository : GenericKursDBRepository<AccruedAmountOfSupplier>,
        IAccruedAmountOfSupplierRepository
    {
        public AccruedAmountOfSupplierRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public AccruedAmountOfSupplierRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public List<AccruedAmountOfSupplier> GetByDate(DateTime startDate, DateTime endDate)
        {
            return Context.AccruedAmountOfSupplier.Where(_ => _.DocDate >= startDate && _.DocDate <= endDate).ToList();
        }
    }


    public class AccruedAmountForClientRepository : GenericKursDBRepository<AccruedAmountForClient>,
        IAccruedAmountForClientRepository
    {
        public AccruedAmountForClientRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public AccruedAmountForClientRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public List<AccruedAmountForClient> GetByDate(DateTime startDate, DateTime endDate)
        {
            return Context.AccruedAmountForClient.Where(_ => _.DocDate >= startDate && _.DocDate <= endDate).ToList();
        }
    }
}