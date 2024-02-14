using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using KursDomain.Repository.Base;
using KursDomain.RepositoryHelper;

namespace KursDomain.Repository.WarehouseRepository;

public interface IWarehouseRepository : IKursGenericRepository<SD_27, decimal>
{
    Task<List<NomenklStoreRemainItem>> GetNomenklsOnWarehouseAsync(DateTime date, decimal warehouseDC);
    List<NomenklStoreRemainItem> GetNomenklsOnWarehouse(DateTime date, decimal warehouseDC);
}
