using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data;
using KursAM2.ViewModel.Finance.Invoices.Base;
using KursDomain;
using KursDomain.Documents.Invoices;
using KursDomain.IDocuments.Finance;
using KursDomain.Repository;

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

        string GetInfoById(Guid id);
        IDictionary<Guid,string> GetInfoByIds(List<Guid>  ids);
        string GetInfoById(decimal dc);
        IDictionary<decimal,string> GetInfoByIds(List<decimal>  ids);

        string GetInfoByRowId(Guid id);
        IDictionary<Guid,string> GetInfoByRowIds(List<Guid>  ids);
        
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
                .FirstOrDefault(_ => _.DOC_CODE == dc), new UnitOfWork<ALFAMEDIAEntities>(), true);
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
                .AsNoTracking()
                .OrderByDescending(_ => _.DocDate).ToList();
            var dd = data.FirstOrDefault(_ => _.ClientDC == 10430003379);
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

        public string GetInfoById(Guid id)
        {
            var data = Context.SD_84.SingleOrDefault(_ => _.Id == id);
            if (data is null) return string.Empty;
            var snum = string.IsNullOrWhiteSpace(data.SF_OUT_NUM)
                ? data.SF_IN_NUM.ToString()
                : $"{data.SF_IN_NUM}/{data.SF_OUT_NUM}";
            return
                $"С/фактура клиенту:{GlobalOptions.ReferencesCache.GetKontragent(data.SF_CLIENT_DC)} №{snum} от {data.SF_DATE.ToShortDateString()}";
        }

        public IDictionary<Guid,string> GetInfoByIds(List<Guid> ids)
        {
            if (ids is null || !ids.Any()) return new Dictionary<Guid,string>();
            return ids.ToDictionary(id => id, GetInfoById);
        }

        public string GetInfoById(decimal dc)
        {
            var data = Context.SD_84.SingleOrDefault(_ => _.DOC_CODE == dc);
            if (data is null) return string.Empty;
            var snum = string.IsNullOrWhiteSpace(data.SF_OUT_NUM)
                ? data.SF_IN_NUM.ToString()
                : $"{data.SF_IN_NUM}/{data.SF_OUT_NUM}";
            return $"№{snum} от {data.SF_DATE.ToShortDateString()}";
        }

        public IDictionary<decimal,string> GetInfoByIds(List<decimal> ids)
        {
            
            if (ids is null || !ids.Any()) return new Dictionary<decimal,string>();
            return ids.ToDictionary(id => id, GetInfoById);
        }

        public string GetInfoByRowId(Guid rowid)
        {
            var id = Context.TD_84.FirstOrDefault(_ => _.Id == rowid)?.DocId;
            return id is null ? string.Empty : GetInfoById(id.Value);
        }

        public IDictionary<Guid, string> GetInfoByRowIds(List<Guid> ids)
        {
            if (ids is null || !ids.Any()) return new Dictionary<Guid, string>();
            var docIds = ids.Select(rowid => Context.TD_84.SingleOrDefault(_ => _.Id == rowid)?.DocId)
                .Where(docId => docId is not null).Select(docId => (Guid)docId).ToList();
            return GetInfoByIds(docIds);
        }
    }
}
