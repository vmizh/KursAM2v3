using KursDomain;
using System;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Data;

namespace KursAM2.Repositories.WarehousesRepository
{
    public interface IPrihodOrderRepository
    {
        string GetInfoByRowId(Guid rowId);
    }

    public class PrihodOrderRepository : IPrihodOrderRepository
    {
        private const decimal PrihodOrderTypeDC = 2010000001;
        private readonly ALFAMEDIAEntities context;

        public PrihodOrderRepository(ALFAMEDIAEntities context)
        {
            this.context = context;
        }
        public string GetInfoByRowId(Guid rowId)
        {
            var id = context.TD_24.FirstOrDefault(_ => _.Id == rowId)?.DocId;
            if(id is null) return string.Empty;
            var doc = context.SD_24.Include(_ => _.TD_24).Where(_ => _.DD_TYPE_DC == PrihodOrderTypeDC).AsNoTracking()
                .FirstOrDefault(_ => _.Id == id);
            if (doc == null) return string.Empty;
            var num = string.IsNullOrWhiteSpace(doc.DD_EXT_NUM)
                ? doc.DD_IN_NUM.ToString()
                : $"{doc.DD_IN_NUM}/{doc.DD_EXT_NUM}";
            return $"Прих. складской ордер №{num} от {doc.DD_DATE.ToShortDateString()} склад: {GlobalOptions.ReferencesCache.GetWarehouse(doc.DD_SKLAD_POL_DC)} создватель: {doc.CREATOR}";

        }
    }
}
