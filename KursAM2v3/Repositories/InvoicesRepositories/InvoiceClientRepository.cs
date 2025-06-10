using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data;
using Helper;
using KursAM2.ViewModel.Finance.Invoices.Base;
using KursDomain;
using KursDomain.Documents.Invoices;
using KursDomain.ICommon;
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

        IList<SD_84> GetSearch(DateTime start, DateTime end);

        void UpdateProjectsInfo(decimal dc, IEnumerable<Guid> projectIds, string desc);



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
            var doc = Context.SD_84.Include(_ => _.TD_84).AsNoTracking().FirstOrDefault(_ => _.DOC_CODE == dc);
            if (doc == null) return null;
            var newId = Guid.NewGuid();
            var ret = new InvoiceClientViewModel(doc)
            {
                DocCode = -1,
                DocDate = DateTime.Today,
                Note = null,
                REGISTER_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name,
                InnerNumber = -1,
                OuterNumber = null,
                IsAccepted = false,
                myState = RowStatus.NewRow,
                Id = newId
            };
            foreach (var row in ret.Rows)
            {
                var newCode = 1;
                foreach (var item in ret.Rows.Cast<InvoiceClientRowViewModel>())
                {
                    item.DocCode = -1;
                    item.Id = Guid.NewGuid();
                    item.DocId = newId;
                    item.Code = newCode;
                    item.Note = " ";
                    item.Shipped = 0;
                    item.State = RowStatus.NewRow;
                    newCode++;
                }
            }
            return ret;
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

        public IList<SD_84> GetSearch(DateTime start, DateTime end)
        {
            return Context
                .SD_84
                .Include(_ => _.SD_43)
                .Include(_ => _.SD_431)
                .Include(_ => _.SD_432)
                .Include(_ => _.SD_301)
                .Include(_ => _.TD_84)
                .Include("TD_84.SD_83")
                .Include("TD_84.TD_24")
                .AsNoTracking()
                .Where(_ => _.SF_DATE >= start && _.SF_DATE <= end).ToList();
        }

        public void UpdateProjectsInfo(decimal dc, IEnumerable<Guid> projectIds, string desc)
        {

            var doc = Context.SD_84
                .AsNoTracking()
                .FirstOrDefault(_ => _.DOC_CODE == dc);
            if (doc == null) return;

            var sql =
                $@"DELETE FROM ProjectDocuments WHERE InvoiceClientId = '{CustomFormat.GuidToSqlString(doc.Id)}'";
            Context.Database.ExecuteSqlCommand(sql);
            foreach (var projId in projectIds.ToList())
            {
                var sqlIns = $@"INSERT INTO dbo.ProjectDocuments
                                    (
                                      Id,ProjectId,DocType,DocInfo,Note,BankCode,CashInDC
                                     ,CashOutDC,WarehouseOrderInDC,WaybillDC
                                     ,AccruedClientRowId,AccruedSupplierRowId,UslugaClientRowId,UslugaProviderRowId
                                     ,CurrencyConvertId,InvoiceClientId,InvoiceProviderId
                                    )
                                    VALUES
                                    (
                                      newid() -- Id - uniqueidentifier NOT NULL
                                     ,'{CustomFormat.GuidToSqlString(projId)}' 
                                     ,84 
                                     ,'{desc}' 
                                     ,'' 
                                     ,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
                                     ,'{CustomFormat.GuidToSqlString(doc.Id)}'
                                     ,NULL
                                    );";
                Context.Database.ExecuteSqlCommand(sqlIns);

            }
        }

        public void UpdateLinkProjects(decimal dc, IEnumerable<Guid> projectIds)
        {
            var cashs = Context.SD_33.Where(_ => _.SFACT_DC == dc);
            var waybills = Context.TD_24.Include(_ => _.SD_24).Where(_ => _.DDT_SFACT_DC == dc);

        }
    }
}
