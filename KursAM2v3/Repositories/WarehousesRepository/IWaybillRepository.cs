using System;
using System.Data.Entity;
using System.Linq;
using Data;
using KursDomain;
using static DevExpress.Utils.Drawing.Helpers.NativeMethods;

namespace KursAM2.Repositories.WarehousesRepository
{
    public interface IWaybillRepository
    {
        SD_24 GetByDC(decimal dc);
        SD_24 GetById(Guid id);
        SD_24 GetByRowId(Guid rowId);

        IQueryable<SD_24> GetByPeriod(DateTime start, DateTime end);
        IQueryable<SD_24> GetForInvoiceClient(decimal invoiceDC);
        IQueryable<SD_24> GetForWarehouse(decimal warehouseDC, DateTime? start = null, DateTime? end = null);
        IQueryable<SD_24> GetForKontragent(decimal kontragentDC, DateTime? start = null, DateTime? end = null);
        IQueryable<SD_24> GetSearchForPattern(string strSearch, DateTime? start = null, DateTime? end = null);

        string GetInfoByDocDC(decimal dc);
        string GetInfoByDocId(Guid id);
        string GetInfoByRowId(Guid rowId);

    }

    public class WaybillRepository : IWaybillRepository
    {
        private const decimal WaybillTypeDC = 2010000012;
        private readonly ALFAMEDIAEntities context;

        public WaybillRepository(ALFAMEDIAEntities context)
        {
            this.context = context;
        }

        public SD_24 GetByDC(decimal dc)
        {
            return context.SD_24.Include(_ => _.TD_24).Where(_ => _.DD_TYPE_DC == WaybillTypeDC).AsNoTracking()
                .FirstOrDefault(_ => _.DOC_CODE == dc);
        }

        public SD_24 GetById(Guid id)
        {
            return context.SD_24.Include(_ => _.TD_24).Where(_ => _.DD_TYPE_DC == WaybillTypeDC).AsNoTracking()
                .FirstOrDefault(_ => _.Id == id);
        }

        public SD_24 GetByRowId(Guid rowId)
        {
            return context.SD_24.Include(_ => _.TD_24).Where(_ => _.DD_TYPE_DC == WaybillTypeDC).AsNoTracking()
                .FirstOrDefault(_ => _.TD_24.Any(t => t.Id == rowId));
        }

        public IQueryable<SD_24> GetByPeriod(DateTime start, DateTime end)
        {
            return context.SD_24.Include(_ => _.TD_24).AsNoTracking()
                .Where(_ => _.DD_TYPE_DC == WaybillTypeDC && _.DD_DATE >= start && _.DD_DATE <= end);
        }

        public IQueryable<SD_24> GetForInvoiceClient(decimal invoiceDC)
        {
            var listDCs = context.TD_24.Include(_ => _.SD_24)
                .Where(_ => _.SD_24.DD_TYPE_DC == WaybillTypeDC && _.DDT_SFACT_DC == invoiceDC).Select(s => s.DOC_CODE)
                .ToList();
            return context.SD_24.Include(_ => _.TD_24).AsNoTracking().Where(_ => listDCs.Contains(_.DOC_CODE));
        }

        public IQueryable<SD_24> GetForWarehouse(decimal warehouseDC, DateTime? start = null, DateTime? end = null)
        {
            if (start is not null)
            {
                if (end is null)
                    return context.SD_24.Include(_ => _.TD_24).AsNoTracking()
                        .Where(_ => _.DD_TYPE_DC == WaybillTypeDC && _.DD_SKLAD_OTPR_DC == warehouseDC &&
                                    _.DD_DATE >= start);
                return context.SD_24.Include(_ => _.TD_24).AsNoTracking().Where(_ =>
                    _.DD_SKLAD_OTPR_DC == warehouseDC
                    && _.DD_DATE >= start && _.DD_DATE <= end);
            }

            if (end is null)
                return context.SD_24.Include(_ => _.TD_24).AsNoTracking()
                    .Where(_ => _.DD_TYPE_DC == WaybillTypeDC && _.DD_SKLAD_OTPR_DC == warehouseDC);
            return context.SD_24.Include(_ => _.TD_24).AsNoTracking()
                .Where(_ => _.DD_SKLAD_OTPR_DC == warehouseDC && _.DD_DATE <= end);
        }

        public IQueryable<SD_24> GetForKontragent(decimal kontragentDC, DateTime? start = null, DateTime? end = null)
        {
            if (start is not null)
            {
                if (end is null)
                    return context.SD_24.Include(_ => _.TD_24).AsNoTracking()
                        .Where(_ => _.DD_TYPE_DC == WaybillTypeDC && _.DD_KONTR_POL_DC == kontragentDC &&
                                    _.DD_DATE >= start);
                return context.SD_24.Include(_ => _.TD_24).AsNoTracking().Where(_ =>
                    _.DD_KONTR_POL_DC == kontragentDC
                    && _.DD_DATE >= start && _.DD_DATE <= end);
            }

            if (end is null)
                return context.SD_24.Include(_ => _.TD_24).AsNoTracking()
                    .Where(_ => _.DD_TYPE_DC == WaybillTypeDC && _.DD_KONTR_POL_DC == kontragentDC);
            return context.SD_24.Include(_ => _.TD_24).AsNoTracking()
                .Where(_ => _.DD_TYPE_DC == WaybillTypeDC && _.DD_KONTR_POL_DC == kontragentDC && _.DD_DATE <= end);
        }

        public IQueryable<SD_24> GetSearchForPattern(string strSearch, DateTime? start = null, DateTime? end = null)
        {
            var sql = $@"SELECT
        DISTINCT s24.DOC_CODE
    FROM
        td_24 t24 (NOLOCK)
    INNER JOIN
      sd_24 s24 (NOLOCK)
        ON t24.DOC_CODE = s24.DOC_CODE
    INNER JOIN sd_83 s83 (nolock) ON t24.DDT_NOMENKL_DC = s83.DOC_CODE
    LEFT OUTER JOIN
      sd_43 s43_pol (NOLOCK)
        ON s24.DD_KONTR_POL_DC = s43_pol.DOC_CODE
    LEFT OUTER JOIN
      sd_43 s43_otpr (NOLOCK)
        ON s24.DD_KONTR_OTPR_DC = s43_otpr.DOC_CODE
    LEFT OUTER JOIN sd_27 s27_pol (nolock) ON s24.DD_SKLAD_POL_DC = s27_pol.DOC_CODE
    LEFT OUTER JOIN sd_27 s27_otpr (nolock) ON s24.DD_SKLAD_OTPR_DC = s27_otpr.DOC_CODE

WHERE DD_TYPE_DC = 2010000012 AND CAST(s24.DD_IN_NUM AS VARCHAR)+isnull(s24.DD_EXT_NUM,'') + isnull(s24.DD_KOMU_PEREDANO,'') + isnull(s24.DD_OT_KOGO_POLUCHENO,'') +
+ s83.NOM_NOMENKL + s83.NOM_NAME + isnull(s83.NOM_FULL_NAME,'') + isnull(s83.NOM_POLNOE_IMIA,'')+isnull(s83.NOM_NOTES,'')
+ isnull(s43_pol.NAME,'') + isnull(s43_pol.INN,'') + isnull(s43_pol.NOTES,'') + isnull(s43_pol.NAME_FULL,'') 
+ isnull(s43_otpr.NAME,'') + isnull(s43_otpr.INN,'') + isnull(s43_otpr.NOTES,'') + isnull(s43_otpr.NAME_FULL,'')
+ isnull(s27_pol.SKL_NAME,'') + isnull(s27_otpr.SKL_NAME,'') LIKE '%{strSearch}%'";
            var listDCs = context.Database.SqlQuery<decimal>(sql).ToList();
            return context.SD_24.Include(_ => _.TD_24).AsNoTracking().Where(_ => listDCs.Contains(_.DOC_CODE));
            //return context.SD_24.Include(_ => _.TD_24).AsNoTracking().Where($"".Contains(strSearch));
        }

        public string GetInfoByDocDC(decimal dc)
        {
            var doc = context.SD_24.Include(_ => _.TD_24).Where(_ => _.DD_TYPE_DC == WaybillTypeDC).AsNoTracking()
                .FirstOrDefault(_ => _.DOC_CODE == dc);
            if (doc == null) return string.Empty;
            var num = string.IsNullOrWhiteSpace(doc.DD_EXT_NUM)
                ? doc.DD_IN_NUM.ToString()
                : $"{doc.DD_IN_NUM}/{doc.DD_EXT_NUM}";
            return $"Расх.накл №{num} от {doc.DD_DATE.ToShortDateString()} склад: {GlobalOptions.ReferencesCache.GetWarehouse(doc.DD_SKLAD_OTPR_DC)} создватель: {doc.CREATOR}";
        }

        public string GetInfoByDocId(Guid id)
        {
            var doc = context.SD_24.Include(_ => _.TD_24).Where(_ => _.DD_TYPE_DC == WaybillTypeDC).AsNoTracking()
                .FirstOrDefault(_ => _.Id == id);
            if (doc == null) return string.Empty;
            var num = string.IsNullOrWhiteSpace(doc.DD_EXT_NUM)
                ? doc.DD_IN_NUM.ToString()
                : $"{doc.DD_IN_NUM}/{doc.DD_EXT_NUM}";
            return $"Расх.накл №{num} от {doc.DD_DATE.ToShortDateString()} склад: {GlobalOptions.ReferencesCache.GetWarehouse(doc.DD_SKLAD_OTPR_DC)} создватель: {doc.CREATOR}";

        }

        public string GetInfoByRowId(Guid rowId)
        {
            var id = context.TD_24.FirstOrDefault(_ => _.Id == rowId)?.DocId;
            if(id is null) return string.Empty;
            var doc = context.SD_24.Include(_ => _.TD_24).Where(_ => _.DD_TYPE_DC == WaybillTypeDC).AsNoTracking()
                .FirstOrDefault(_ => _.Id == id);
            if (doc == null) return string.Empty;
            var num = string.IsNullOrWhiteSpace(doc.DD_EXT_NUM)
                ? doc.DD_IN_NUM.ToString()
                : $"{doc.DD_IN_NUM}/{doc.DD_EXT_NUM}";
            return $"Расх.накл №{num} от {doc.DD_DATE.ToShortDateString()} склад: {GlobalOptions.ReferencesCache.GetWarehouse(doc.DD_SKLAD_OTPR_DC)} создватель: {doc.CREATOR}";
        }
    }
}
