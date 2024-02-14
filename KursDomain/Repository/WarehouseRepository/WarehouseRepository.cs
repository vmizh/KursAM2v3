using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Data;
using KursDomain.Repository.Base;
using KursDomain.RepositoryHelper;

namespace KursDomain.Repository.WarehouseRepository;

public class WarehouseRepository : KursGenericRepository<SD_27, ALFAMEDIAEntities, decimal>,
    IWarehouseRepository
{
    public WarehouseRepository(ALFAMEDIAEntities context) : base(context)
    {
    }

    public async Task<List<NomenklStoreRemainItem>> GetNomenklsOnWarehouseAsync(DateTime date, decimal warehouseDC)
    {
        var data = await GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>("GetNomenklRemains @date",  
            new SqlParameter("@date", date)).ToListAsync(); 
        return data.Where(_ => _.StoreDC == warehouseDC).ToList();

    }

    public List<NomenklStoreRemainItem> GetNomenklsOnWarehouse(DateTime date, decimal warehouseDC)
    {
        var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>("GetNomenklRemains @date",  
            new SqlParameter("@date", date)).ToList(); 
        return data.Where(_ => _.StoreDC == warehouseDC).ToList();
    }
}
