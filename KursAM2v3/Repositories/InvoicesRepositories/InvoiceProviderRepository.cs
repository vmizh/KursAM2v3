using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Data;
using Data.Repository;

namespace KursAM2.Repositories.InvoicesRepositories
{
    public interface IInvoiceProviderRepository
    {
        InvoiceProvider GetByGuidId(Guid id);

        InvoiceProvider GetFullCopy(InvoiceProvider doc);
        InvoiceProvider GetRequisiteCopy(InvoiceProvider doc);

        void DeleteTD26CurrencyConvert();
        InvoiceProvider GetByDocCode(decimal dc);
        List<InvoiceProvider> GetAllByDates(DateTime dateStart, DateTime dateEnd);

        List<InvoiceProviderShort> GetAllForNakladDistribute(Currency crs, DateTime? dateStart,
            DateTime? dateEnd);

        List<InvoiceProviderShort> GetNakladInvoices(DateTime? dateStart,
            DateTime? dateEnd);

        InvoiceProviderRowShort GetInvoiceRow(Guid id);

        InvoiceProviderShort GetInvoiceHead(Guid id);

        //InvoiceProviderDialogs Dialogs { set; get; }
        void Delete(SD_26 entity);
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
                .FirstOrDefault(_ => _.Id == id),new UnitOfWork<ALFAMEDIAEntities>());
        }

        public InvoiceProvider GetFullCopy(InvoiceProvider doc)
        {
            var newId = Guid.NewGuid();
            Context.Entry(doc.Entity).State = EntityState.Added;
            doc.DocCode = -1;
            doc.CREATOR = GlobalOptions.UserInfo.NickName;
            doc.SF_POSTAV_DATE = DateTime.Today;
            doc.Id = newId;
            var i = 1;
            foreach (var r in doc.Rows)
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
            doc.SF_POSTAV_DATE = DateTime.Today;
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
                .FirstOrDefault(_ => _.DOC_CODE == dc),new UnitOfWork<ALFAMEDIAEntities>());
        }

        public List<InvoiceProvider> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            DetachObjects();
            var ret = new List<InvoiceProvider>();
            var data = Context.SD_26
                .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd)
                .OrderByDescending(_ => _.SF_POSTAV_DATE).ToList();
            foreach (var d in data) 
                ret.Add(new InvoiceProvider(d,new UnitOfWork<ALFAMEDIAEntities>(),false));
            return ret;
        }

        public List<InvoiceProviderShort> GetAllForNakladDistribute(Currency crs, DateTime? dateStart,
            DateTime? dateEnd)
        {
            DetachObjects();
            var data = new List<SD_26>();
            var ret = new List<InvoiceProviderShort>();
            if (dateStart == null && dateEnd == null)
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
                    .Where(_ => _.SF_CRS_DC == crs.DocCode && _.SF_ACCEPTED == 1)
                    .ToList();

            if (dateStart == null && dateEnd != null)
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
                                                              && _.SF_ACCEPTED == 1)
                    .ToList();
            if (dateStart != null && dateEnd != null)
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
                                                              && _.SF_CRS_DC == crs.DocCode && _.SF_ACCEPTED == 1)
                    .ToList();

            foreach (var d in data) ret.Add(new InvoiceProviderShort(d,new UnitOfWork<ALFAMEDIAEntities>()));

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
                    //.Include(_ => _.TD_26)
                    //.Include("TD_26.TD_24")
                    //.Include(_ => _.SD_43)
                    //.Include(_ => _.SD_179)
                    //.Include(_ => _.SD_77)
                    //.Include(_ => _.SD_189)
                    //.Include(_ => _.SD_40)
                    //.Include("TD_26.SD_83")
                    //.Include("TD_26.SD_175")
                    //.Include("TD_26.SD_43")
                    //.Include("TD_26.SD_165")
                    //.Include("TD_26.SD_175")
                    //.Include("TD_26.SD_1751")
                    //.Include("TD_26.SD_26")
                    //.Include("TD_26.SD_261")
                    //.Include("TD_26.SD_301")
                    //.Include("TD_26.SD_303")
                    .Where(_ => _.SF_ACCEPTED == 1 && _.IsInvoiceNakald == true
                                                   && (_.NakladDistributedSumma ?? 0) < _.SF_KONTR_CRS_SUMMA)
                    .ToList();

            if (dateStart == null && dateEnd != null)
                data = Context.SD_26
                    //.Include(_ => _.TD_26)
                    //.Include("TD_26.TD_24")
                    //.Include(_ => _.SD_43)
                    //.Include(_ => _.SD_179)
                    //.Include(_ => _.SD_77)
                    //.Include(_ => _.SD_189)
                    //.Include(_ => _.SD_40)
                    //.Include("TD_26.SD_83")
                    //.Include("TD_26.SD_175")
                    //.Include("TD_26.SD_43")
                    //.Include("TD_26.SD_165")
                    //.Include("TD_26.SD_175")
                    //.Include("TD_26.SD_1751")
                    //.Include("TD_26.SD_26")
                    //.Include("TD_26.SD_261")
                    //.Include("TD_26.SD_301")
                    //.Include("TD_26.SD_303")
                    .Where(_ => _.SF_POSTAV_DATE >= dateStart && (_.IsInvoiceNakald ?? false)
                                                              && (_.NakladDistributedSumma ?? 0) <
                                                              _.SF_KONTR_CRS_SUMMA
                                                              && _.SF_ACCEPTED == 1)
                    .ToList();

            if (dateStart != null && dateEnd != null)
                data = Context.SD_26
                    //.Include(_ => _.TD_26)
                    //.Include("TD_26.TD_24")
                    //.Include(_ => _.SD_43)
                    //.Include(_ => _.SD_179)
                    //.Include(_ => _.SD_77)
                    //.Include(_ => _.SD_189)
                    //.Include(_ => _.SD_40)
                    //.Include("TD_26.SD_83")
                    //.Include("TD_26.SD_175")
                    //.Include("TD_26.SD_43")
                    //.Include("TD_26.SD_165")
                    //.Include("TD_26.SD_175")
                    //.Include("TD_26.SD_1751")
                    //.Include("TD_26.SD_26")
                    //.Include("TD_26.SD_261")
                    //.Include("TD_26.SD_301")
                    //.Include("TD_26.SD_303")
                    .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd
                                                              && _.IsInvoiceNakald == true
                                                              && (_.NakladDistributedSumma ?? 0) <
                                                              _.SF_KONTR_CRS_SUMMA && _.SF_ACCEPTED == 1)
                    .ToList();
            foreach (var d in data) ret.Add(new InvoiceProviderShort(d,new UnitOfWork<ALFAMEDIAEntities>()));

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
            return new InvoiceProviderShort(item,new UnitOfWork<ALFAMEDIAEntities>());
        }

        public void Delete(SD_26 entity)
        {
            Context.SD_26.Remove(entity);
            DeleteTD26CurrencyConvert();
        }
    }
}