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
using KursDomain.References;
using KursDomain.Repository;

namespace KursAM2.Repositories.InvoicesRepositories
{
    public interface IInvoiceProviderRepository
    {
        InvoiceProvider GetByGuidId(Guid id);

        InvoiceProvider GetFullCopy(InvoiceProvider doc);
        InvoiceProvider GetRequisiteCopy(InvoiceProvider doc);

        void DeleteTD26CurrencyConvert();
        InvoiceProvider GetByDocCode(decimal dc);

        InvoiceProvider GetFullCopy(decimal dc);
        InvoiceProvider GetRequisiteCopy(decimal dc);

        List<IInvoiceProvider> GetByDocCodes(List<decimal> dcs);
        List<IInvoiceProvider> GetAllByDates(DateTime dateStart, DateTime dateEnd, decimal? crsDC = null);

        List<InvoiceProviderShort> GetAllForNakladDistribute(Currency crs, DateTime? dateStart,
            DateTime? dateEnd);

        List<InvoiceProviderShort> GetNakladInvoices(DateTime? dateStart,
            DateTime? dateEnd);


        InvoiceProviderRowShort GetInvoiceRow(Guid id);

        InvoiceProviderRowCurrencyConvertViewModel GetTransferRow(Guid id);

        InvoiceProviderShort GetInvoiceHead(Guid id);

        //InvoiceProviderDialogs Dialogs { set; get; }
        void Delete(SD_26 entity);
        string GetInfoByRowId(Guid newRowInvoiceRowId);
        void UpdateProjectsInfo(decimal dc, IEnumerable<Guid> projectIds, string desc);

        bool IsInvoiceHasCurrencyConvert(decimal dc);
    }

    public class InvoiceProviderRepository : GenericKursDBRepository<InvoiceProvider>, IInvoiceProviderRepository
    {
        public InvoiceProviderRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public InvoiceProviderRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public InvoiceProvider GetByGuidId(Guid id)
        {
            return new InvoiceProvider(Context.SD_26
                .Include(_ => _.TD_26)
                .Include("TD_26.TD_24")
                .Include(_ => _.SD_43)
                .Include(_ => _.SD_179)
                .Include(_ => _.SD_77)
                .Include(_ => _.SD_189)
                .Include(_ => _.SD_40)
                .Include("TD_26.SD_83")
                .Include("TD_26.SD_175")
                .Include("TD_26.SD_43")
                .Include("TD_26.SD_165")
                .Include("TD_26.SD_175")
                .Include("TD_26.SD_1751")
                .Include("TD_26.SD_26")
                .Include("TD_26.SD_261")
                .Include("TD_26.SD_301")
                .Include("TD_26.SD_303")
                .FirstOrDefault(_ => _.Id == id), new UnitOfWork<ALFAMEDIAEntities>());
        }

        public InvoiceProvider GetFullCopy(InvoiceProvider doc)
        {
            var newId = Guid.NewGuid();
            Context.Entry(doc.Entity).State = EntityState.Added;
            doc.DocCode = -1;
            doc.CREATOR = GlobalOptions.UserInfo.NickName;
            doc.DocDate = DateTime.Today;
            doc.Id = newId;
            var i = 1;
            foreach (var r in doc.Rows.Cast<InvoiceProviderRow>())
            {
                Context.Entry(r.Entity).State = EntityState.Added;
                r.DocCode = -1;
                r.Code = i;
                r.Id = Guid.NewGuid();
                r.DocId = newId;
                r.CurrencyConvertRows.Clear();
                r.Entity.TD_26_CurrencyConvert.Clear();
                i++;
            }

            return doc;
        }

        public InvoiceProvider GetRequisiteCopy(InvoiceProvider doc)
        {
            var newId = Guid.NewGuid();
            Context.Entry(doc.Entity).State = EntityState.Added;
            doc.DocCode = -1;
            doc.CREATOR = GlobalOptions.UserInfo.NickName;
            doc.DocDate = DateTime.Today;
            doc.Id = newId;
            doc.Rows.Clear();
            doc.Entity.TD_26.Clear();
            return doc;
        }

        public void DeleteTD26CurrencyConvert()
        {
            var ids = new List<Guid>(Context.TD_26_CurrencyConvert
                .Where(_ => _.DOC_CODE == null)
                .Select(_ => _.Id).ToList());
            foreach (var id in ids)
            {
                var old = Context.TD_26_CurrencyConvert.FirstOrDefault(_ => _.Id == id);
                if (old != null)
                    Context.TD_26_CurrencyConvert.Remove(old);
            }
        }

        public InvoiceProvider GetByDocCode(decimal dc)
        {
            DetachObjects();
            return new InvoiceProvider(Context.SD_26
                .FirstOrDefault(_ => _.DOC_CODE == dc), new UnitOfWork<ALFAMEDIAEntities>());
        }

        public InvoiceProvider GetFullCopy(decimal dc)
        {
            var doc = Context.SD_26.Include(_ => _.TD_26).AsNoTracking().FirstOrDefault(_ => _.DOC_CODE == dc);
            if (doc == null) return null;
            var newId = Guid.NewGuid();
            var ret = new InvoiceProvider(doc)
            {
                Id = newId,
                DocCode = -1,
                Note = null,
                SF_POSTAV_NUM = null,
                DocDate = DateTime.Today,
                SF_REGISTR_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name,
                myState = RowStatus.NewRow,
                IsAccepted = false,
                IsNDSInPrice = true,
                NakladDistributedSumma = 0,
            };

            var newCode = 1;
            foreach (var row in ret.Rows.Cast<InvoiceProviderRow>())
            {
                row.DocCode = -1;
                row.Id = Guid.NewGuid();
                row.DocId = newId;
                row.Code = newCode;
                row.Note = null;
                row.myState = RowStatus.NewRow;
                newCode++;
            }

            return ret;
        }

        public InvoiceProvider GetRequisiteCopy(decimal dc)
        {
            var doc = Context.SD_26.Include(_ => _.TD_26).AsNoTracking().FirstOrDefault(_ => _.DOC_CODE == dc);
            if (doc == null) return null;
            var newId = Guid.NewGuid();
            var ret = new InvoiceProvider(doc)
            {
                Id = newId,
                DocCode = -1,
                Note = null,
                SF_POSTAV_NUM = null,
                DocDate = DateTime.Today,
                SF_REGISTR_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name,
                myState = RowStatus.NewRow,
                IsAccepted = false,
                IsNDSInPrice = true,
                NakladDistributedSumma = 0,
            };
            return ret;
        }

        public List<IInvoiceProvider> GetByDocCodes(List<decimal> dcs)
        {
            var data = Context.InvoicePostQuery.Where(_ => dcs.Contains(_.DocCode)).ToList();
            return data.Select(_ => _.DocCode).Distinct()
                .Select(dc => new InvoiceProviderBase(data.Where(_ => _.DocCode == dc)))
                .Cast<IInvoiceProvider>().ToList();
        }

        public List<IInvoiceProvider> GetAllByDates(DateTime dateStart, DateTime dateEnd, decimal? crsDC = null)
        {
            var data = Context.InvoicePostQuery.Where(_ => _.Date >= dateStart && _.Date <= dateEnd)
                .OrderByDescending(_ => _.Date).ToList();
            return data.Select(_ => _.DocCode).Distinct()
                .Select(dc => new InvoiceProviderBase(data.Where(_ => _.DocCode == dc))).Cast<IInvoiceProvider>()
                .ToList();
        }

        public List<InvoiceProviderShort> GetAllForNakladDistribute(Currency crs, DateTime? dateStart,
            DateTime? dateEnd)
        {
            DetachObjects();
            List<SD_26> data;
            var ret = new List<InvoiceProviderShort>();
            if (dateStart == null && dateEnd == null)
                // ReSharper disable once RedundantAssignment
                data = Context.SD_26
                    .Include(_ => _.TD_26)
                    .Include("TD_26.TD_24")
                    .Include(_ => _.SD_43)
                    .Include(_ => _.SD_179)
                    .Include(_ => _.SD_77)
                    .Include(_ => _.SD_189)
                    .Include(_ => _.SD_40)
                    .Include("TD_26.SD_83")
                    .Include("TD_26.SD_175")
                    .Include("TD_26.SD_43")
                    .Include("TD_26.SD_165")
                    .Include("TD_26.SD_175")
                    .Include("TD_26.SD_1751")
                    .Include("TD_26.SD_26")
                    .Include("TD_26.SD_261")
                    .Include("TD_26.SD_301")
                    .Include("TD_26.SD_303")
                    .Where(_ => _.SF_CRS_DC == crs.DocCode && _.SF_ACCEPTED == 1 && _.TD_26.Select(x => x.TD_24).Any())
                    .ToList();

            if (dateStart == null && dateEnd != null)
                // ReSharper disable once RedundantAssignment
                data = Context.SD_26
                    .Include(_ => _.TD_26)
                    .Include("TD_26.TD_24")
                    .Include(_ => _.SD_43)
                    .Include(_ => _.SD_179)
                    .Include(_ => _.SD_77)
                    .Include(_ => _.SD_189)
                    .Include(_ => _.SD_40)
                    .Include("TD_26.SD_83")
                    .Include("TD_26.SD_175")
                    .Include("TD_26.SD_43")
                    .Include("TD_26.SD_165")
                    .Include("TD_26.SD_175")
                    .Include("TD_26.SD_1751")
                    .Include("TD_26.SD_26")
                    .Include("TD_26.SD_261")
                    .Include("TD_26.SD_301")
                    .Include("TD_26.SD_303")
                    .Where(_ => _.SF_POSTAV_DATE <= dateStart && _.SF_CRS_DC == crs.DocCode
                                                              && _.SF_ACCEPTED == 1
                                                              && _.TD_26.Select(x => x.TD_24).Any())
                    .ToList();
            if (dateStart != null && dateEnd != null)
            {
                data = Context.SD_26
                    .Include(_ => _.TD_26)
                    .Include("TD_26.TD_24")
                    .Include(_ => _.SD_43)
                    .Include(_ => _.SD_179)
                    .Include(_ => _.SD_77)
                    .Include(_ => _.SD_189)
                    .Include(_ => _.SD_40)
                    .Include("TD_26.SD_83")
                    .Include("TD_26.SD_175")
                    .Include("TD_26.SD_43")
                    .Include("TD_26.SD_165")
                    .Include("TD_26.SD_175")
                    .Include("TD_26.SD_1751")
                    .Include("TD_26.SD_26")
                    .Include("TD_26.SD_261")
                    .Include("TD_26.SD_301")
                    .Include("TD_26.SD_303")
                    .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd
                                                              && _.SF_CRS_DC == crs.DocCode && _.SF_ACCEPTED == 1
                                                              && _.TD_26.Select(x => x.TD_24).Any())
                    .ToList();

                var invdc = Context.TD_26_CurrencyConvert
                    .Include(_ => _.TD_26)
                    .Include(_ => _.TD_26.SD_26)
                    .Where(_ => _.TD_26.SD_26.SF_POSTAV_DATE >= dateStart &&
                                _.TD_26.SD_26.SF_POSTAV_DATE <= dateEnd).ToList();
                foreach (var dc in invdc)
                {
                    var doc = Context.SD_26
                        .Include(_ => _.TD_26)
                        .Include("TD_26.TD_24")
                        .Include("TD_26.TD_26_CurrencyConvert")
                        .Include(_ => _.SD_43)
                        .Include(_ => _.SD_179)
                        .Include(_ => _.SD_77)
                        .Include(_ => _.SD_189)
                        .Include(_ => _.SD_40)
                        .Include("TD_26.SD_83")
                        .Include("TD_26.SD_175")
                        .Include("TD_26.SD_43")
                        .Include("TD_26.SD_165")
                        .Include("TD_26.SD_175")
                        .Include("TD_26.SD_1751")
                        .Include("TD_26.SD_26")
                        .Include("TD_26.SD_261")
                        .Include("TD_26.SD_301")
                        .Include("TD_26.SD_303")
                        .FirstOrDefault(_ => _.DOC_CODE == dc.DOC_CODE);
                    if (doc != null) ret.Add(new InvoiceProviderShort(doc, new UnitOfWork<ALFAMEDIAEntities>()));
                }

                foreach (var d in data) ret.Add(new InvoiceProviderShort(d, new UnitOfWork<ALFAMEDIAEntities>()));
            }

            return ret;
        }

        public List<InvoiceProviderShort> GetNakladInvoices(DateTime? dateStart,
            DateTime? dateEnd)
        {
            DetachObjects();
            var data = new List<SD_26>();
            var ret = new List<InvoiceProviderShort>();
            if (dateStart == null && dateEnd == null)
                data = Context.SD_26
                    .Where(_ => _.SF_ACCEPTED == 1 && _.IsInvoiceNakald == true
                                                   && (_.NakladDistributedSumma ?? 0) < _.SF_KONTR_CRS_SUMMA
                    )
                    .ToList();

            if (dateStart == null && dateEnd != null)
                data = Context.SD_26
                    .Where(_ => _.SF_POSTAV_DATE >= dateStart && (_.IsInvoiceNakald ?? false)
                                                              && (_.NakladDistributedSumma ?? 0) <
                                                              _.SF_KONTR_CRS_SUMMA
                                                              && _.SF_ACCEPTED == 1)
                    .ToList();

            if (dateStart != null && dateEnd != null)
                data = Context.SD_26
                    .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd
                                                              && _.IsInvoiceNakald == true
                                                              && (_.NakladDistributedSumma ?? 0) <
                                                              _.SF_KONTR_CRS_SUMMA && _.SF_ACCEPTED == 1
                                                              && _.TD_26.Select(x => x.TD_24).Any())
                    .ToList();
            foreach (var d in data) ret.Add(new InvoiceProviderShort(d, new UnitOfWork<ALFAMEDIAEntities>()));

            return ret;
        }

        public InvoiceProviderRowShort GetInvoiceRow(Guid id)
        {
            DetachObjects();
            var item = Context.TD_26.Include(_ => _.TD_24)
                .Include(_ => _.SD_83)
                .Include(_ => _.SD_175)
                .Include(_ => _.SD_43)
                .Include(_ => _.SD_165)
                .Include(_ => _.SD_175)
                .Include(_ => _.SD_1751)
                .Include(_ => _.SD_26)
                .Include(_ => _.SD_261)
                .Include(_ => _.SD_301)
                .Include(_ => _.SD_303)
                .SingleOrDefault(_ => _.Id == id);
            if (item != null) return new InvoiceProviderRowShort(item);
            return null;
        }

        public InvoiceProviderRowCurrencyConvertViewModel GetTransferRow(Guid id)
        {
            DetachObjects();
            var item = Context.TD_26_CurrencyConvert.Include(_ => _.TD_26)
                .SingleOrDefault(_ => _.Id == id);
            return new InvoiceProviderRowCurrencyConvertViewModel(item);
        }

        public InvoiceProviderShort GetInvoiceHead(Guid id)
        {
            DetachObjects();
            var item = Context.SD_26
                .Include(_ => _.TD_26)
                .Include(_ => _.SD_43)
                .Include(_ => _.SD_179)
                .Include(_ => _.SD_77)
                .Include(_ => _.SD_189)
                .Include(_ => _.SD_40)
                .SingleOrDefault(_ => _.Id == id);
            return new InvoiceProviderShort(item, new UnitOfWork<ALFAMEDIAEntities>());
        }

        public void Delete(SD_26 entity)
        {
            Context.SD_26.Remove(entity);
            DeleteTD26CurrencyConvert();
        }


        public string GetInfoByRowId(Guid rowid)
        {
            var id = Context.TD_26.FirstOrDefault(_ => _.Id == rowid)?.DocId;
            return id is null ? string.Empty : GetInfoById(id.Value);
        }

        public void UpdateProjectsInfo(decimal dc, IEnumerable<Guid> projectIds, string desc)
        {

            var doc = Context.SD_26
                .AsNoTracking()
                .FirstOrDefault(_ => _.DOC_CODE == dc);
            if (doc == null) return;

            var sql =
                $@"DELETE FROM ProjectDocuments WHERE InvoiceProviderId = '{CustomFormat.GuidToSqlString(doc.Id)}'";
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
                                     ,26
                                     ,'{desc}' 
                                     ,'' 
                                     ,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
                                     ,NULL
                                     ,'{CustomFormat.GuidToSqlString(doc.Id)}'
                                    );";
                Context.Database.ExecuteSqlCommand(sqlIns);

            }
        }

        public bool IsInvoiceHasCurrencyConvert(decimal dc)
        {
            return Context.TD_26_CurrencyConvert.Any(_ => _.DOC_CODE == dc);
        }

        public string GetInfoById(Guid id)
        {
            var data = Context.SD_26.SingleOrDefault(_ => _.Id == id);
            if (data is null) return string.Empty;
            var snum = string.IsNullOrWhiteSpace(data.SF_POSTAV_NUM)
                ? data.SF_IN_NUM.ToString()
                : $"{data.SF_IN_NUM}/{data.SF_POSTAV_NUM}";
            return
                $"С/фактура поставщика:{GlobalOptions.ReferencesCache.GetKontragent(data.SF_POST_DC)} №{snum} от {data.SF_POSTAV_DATE.ToShortDateString()}";
        }
    }
}
