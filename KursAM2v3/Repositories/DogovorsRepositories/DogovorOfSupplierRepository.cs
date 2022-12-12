using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data;
using KursDomain.Repository;
using KursDomain.Documents.Dogovora;
using KursDomain.Documents.Invoices;

namespace KursAM2.Repositories.DogovorsRepositories
{
    public interface IDogovorOfSupplierRepository
    {

        DogovorOfSupplier Dogovor { set; get; }

        List<DogovorOfSupplier> GetByDate(DateTime startDate, DateTime endDate);

        List<LinkDocumentInfo> GetLinkDocuments();
        List<InvoicePaymentDocument> GetPayments();
    }

    public class DogovorOfSupplierRepository : GenericKursDBRepository<DogovorOfSupplier>, 
        IDogovorOfSupplierRepository
    {
        public UnitOfWork<ALFAMEDIAEntities> UnitOfWork;

        public DogovorOfSupplierRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            UnitOfWork = (UnitOfWork<ALFAMEDIAEntities>) unitOfWork;
        }

        public DogovorOfSupplierRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public DogovorOfSupplier Dogovor { get; set; }
        public List<DogovorOfSupplier> GetByDate(DateTime startDate, DateTime endDate)
        {
            return UnitOfWork.Context.DogovorOfSupplier.Include(_ => _.DogovorOfSupplierRow)
                .Where(_ => _.DocDate >= startDate && _.DocDate <= endDate).ToList();
        }

        public List<LinkDocumentInfo> GetLinkDocuments()
        {
            return null;
        }

        public List<InvoicePaymentDocument> GetPayments()
        {
            return null;
        }
    }
}
