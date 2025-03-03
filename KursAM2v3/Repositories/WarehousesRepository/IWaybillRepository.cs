using System;
using System.Linq;
using Data;

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
}
