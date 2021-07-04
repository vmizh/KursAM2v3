using System;
using System.Collections.Generic;
using System.Linq;
using Core.EntityViewModel.Invoices;
using Core.Invoices.EntityViewModel;
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
                .FirstOrDefault(_ => _.DOC_CODE == dc),new UnitOfWork<ALFAMEDIAEntities>());
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
            throw new NotImplementedException();
        }
    }
}