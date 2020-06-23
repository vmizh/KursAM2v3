using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core.EntityViewModel;
using Core.Repository.Base;
using Data;
using KursAM2.Dialogs;

namespace KursAM2.Repositories.InvoicesRepositories
{
    public interface IInvoiceProviderRepository
    {
        InvoiceProvider GetById(Guid id);
        InvoiceProvider GetByDocCode(decimal dc);
        List<InvoiceProvider> GetAllByDates(DateTime dateStart, DateTime dateEnd);

        List<InvoiceProviderShort> GetAllForNakladDistribute(Currency crs, DateTime? dateStart,
            DateTime? dateEnd);
        InvoiceProviderDialogs Dialogs { set; get; }
    }

    public class InvoiceProviderRepository : GenericKursRepository<InvoiceProvider>, IInvoiceProviderRepository
    {
        public InvoiceProviderRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            Dialogs = new InvoiceProviderDialogs(this);
        }

        public InvoiceProviderRepository(ALFAMEDIAEntities context) : base(context)
        {
            Dialogs = new InvoiceProviderDialogs(this);
        }

        public InvoiceProvider GetById(Guid id)
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
                .FirstOrDefault(_ => _.Id == id));
        }

        public InvoiceProvider GetByDocCode(decimal dc)
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
                .FirstOrDefault(_ => _.DOC_CODE == dc));
        }

        public List<InvoiceProvider> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            var ret = new List<InvoiceProvider>();
            var data = Context.SD_26
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
                .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd).ToList();
            foreach (var d in data) ret.Add(new InvoiceProvider(d));
            return ret;
        }

        public List<InvoiceProviderShort> GetAllForNakladDistribute(Currency crs, DateTime? dateStart,
            DateTime? dateEnd)
        {
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

            foreach (var d in data) ret.Add(new InvoiceProviderShort(d));

            return ret;
        }

        public InvoiceProviderDialogs Dialogs { get; set; }
    }
}