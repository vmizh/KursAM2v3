using System;
using System.Collections.Generic;
using System.Linq;
using Core.EntityViewModel.Invoices;
using Data;
using Data.Repository;

namespace KursAM2.Repositories.InvoicesRepositories
{
    public interface IInvoiceClientRepository
    {
        InvoiceClient GetById(Guid id);
        InvoiceClient GetByDocCode(decimal dc);
        InvoiceClient GetFullCopy(InvoiceClient doc);
        InvoiceClient GetRequisiteCopy(InvoiceClient doc);
        InvoiceClient GetFullCopy(decimal dc);
        InvoiceClient GetRequisiteCopy(decimal dc);
        void Delete(SD_84 entity);
        List<InvoiceClient> GetAllByDates(DateTime dateStart, DateTime dateEnd);
    }

    public class InvoiceClientRepository : GenericKursDBRepository<InvoiceClient>, IInvoiceClientRepository
    {
        public InvoiceClientRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public InvoiceClientRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public InvoiceClient GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public InvoiceClient GetByDocCode(decimal dc)
        {
            DetachObjects();
            return new InvoiceClient(Context.SD_84
                .FirstOrDefault(_ => _.DOC_CODE == dc), new UnitOfWork<ALFAMEDIAEntities>());
        }

        public InvoiceClient GetFullCopy(InvoiceClient doc)
        {
            throw new NotImplementedException();
        }

        public InvoiceClient GetRequisiteCopy(InvoiceClient doc)
        {
            throw new NotImplementedException();
        }

        public InvoiceClient GetFullCopy(decimal dc)
        {
            throw new NotImplementedException();
        }

        public InvoiceClient GetRequisiteCopy(decimal dc)
        {
            throw new NotImplementedException();
        }

        public void Delete(SD_84 entity)
        {
            throw new NotImplementedException();
        }

        public List<InvoiceClient> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            DetachObjects();
            var ret = new List<InvoiceClient>();
            var data = Context.SD_84
                .Where(_ => _.SF_DATE >= dateStart && _.SF_DATE <= dateEnd)
                .OrderByDescending(_ => _.SF_DATE).ToList();
            foreach (var d in data)
                ret.Add(new InvoiceClient(d, new UnitOfWork<ALFAMEDIAEntities>()));
            return ret;
        }
    }
}