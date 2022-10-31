using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.EntityViewModel.Invoices;
using Data;
using Data.Repository;
using KursAM2.ViewModel.Finance.Invoices.Base;

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
        List<IInvoiceClient> GetAllByDates(DateTime dateStart, DateTime dateEnd);
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
                .FirstOrDefault(_ => _.DOC_CODE == dc), new UnitOfWork<ALFAMEDIAEntities>(),true);
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

        public List<IInvoiceClient> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            //var data = Context.SD_84
            //    .Where(_ => _.SF_DATE >= dateStart && _.SF_DATE <= dateEnd)
            //    .OrderByDescending(_ => _.SF_DATE).ToList();
            var data = Context.InvoiceClientQuery.Where(_ => _.DocDate >= dateStart && _.DocDate <= dateEnd)
                .OrderByDescending(_ => _.DocDate).ToList();
            return data.Select(_ => _.DocCode).Distinct()
                .Select(dc => new InvoiceClientBase(data.Where(_ => _.DocCode == dc))).Cast<IInvoiceClient>().ToList();
        }
    }
}
