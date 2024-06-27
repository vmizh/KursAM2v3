using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Data;
using KursDomain.Repository;
using KursAM2.ViewModel.Finance.Invoices.Base;
using KursDomain.Documents.Invoices;
using KursDomain.IDocuments.Finance;

namespace KursAM2.Repositories.InvoicesRepositories
{
    public interface IInvoiceClientRepository
    {
        InvoiceClientViewModel GetById(Guid id);
        InvoiceClientViewModel GetByDocCode(decimal dc);
        InvoiceClientViewModel GetFullCopy(InvoiceClientViewModel doc);
        InvoiceClientViewModel GetRequisiteCopy(InvoiceClientViewModel doc);
        InvoiceClientViewModel GetFullCopy(decimal dc);
        InvoiceClientViewModel GetRequisiteCopy(decimal dc);
        void Delete(SD_84 entity);
        List<IInvoiceClient> GetAllByDates(DateTime dateStart, DateTime dateEnd);
        List<IInvoiceClient> GetByDocCodes(List<decimal> dcs);
      
    }

    public class InvoiceClientRepository : GenericKursDBRepository<InvoiceClientViewModel>, IInvoiceClientRepository
    {
        public InvoiceClientRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public InvoiceClientRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public InvoiceClientViewModel GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public InvoiceClientViewModel GetByDocCode(decimal dc)
        {
            DetachObjects();
            return new InvoiceClientViewModel(Context.SD_84
                .FirstOrDefault(_ => _.DOC_CODE == dc), new UnitOfWork<ALFAMEDIAEntities>(),true);
        }

        public InvoiceClientViewModel GetFullCopy(InvoiceClientViewModel doc)
        {
            throw new NotImplementedException();
        }

        public InvoiceClientViewModel GetRequisiteCopy(InvoiceClientViewModel doc)
        {
            throw new NotImplementedException();
        }

        public InvoiceClientViewModel GetFullCopy(decimal dc)
        {
            throw new NotImplementedException();
        }

        public InvoiceClientViewModel GetRequisiteCopy(decimal dc)
        {
            throw new NotImplementedException();
        }

        public void Delete(SD_84 entity)
        {
            throw new NotImplementedException();
        }

        public List<IInvoiceClient> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            var data = Context.InvoiceClientQuery.Where(_ => _.DocDate >= dateStart && _.DocDate <= dateEnd)
                .OrderByDescending(_ => _.DocDate).ToList();
            return data.Select(_ => _.DocCode).Distinct()
                .Select(dc => new InvoiceClientBase(data.Where(_ => _.DocCode == dc))).Cast<IInvoiceClient>().ToList();
        }

        public List<IInvoiceClient> GetByDocCodes(List<decimal> dcs)
        {
            var data = Context.InvoiceClientQuery.Where(_ => dcs.Contains(_.DocCode)).ToList();
            return data.Select(_ => _.DocCode).Distinct()
                .Select(dc => new InvoiceClientBase(data.Where(_ => _.DocCode == dc)))
                .Cast<IInvoiceClient>().ToList();
        }

        
    }
}
