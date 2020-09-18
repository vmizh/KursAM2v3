using System;
using System.Collections.Generic;
using Core.EntityViewModel;
using Core.Repository.Base;
using Data;
using Data.Repository;

namespace KursAM2.Repositories.InvoicesRepositories
{
    public interface IInvoiceClientRepository
    {
        InvoiceClient GetById(Guid id);
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

        public List<InvoiceClient> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            throw new NotImplementedException();
        }
    }
}