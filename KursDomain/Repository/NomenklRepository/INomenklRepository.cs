using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.NomenklRepository;

public interface INomenklRepository : IKursGenericRepository<SD_83, decimal>
{
    Task RecalcPriceAsync();
    Task RecalcPriceAsync(List<decimal> docCodes);

    void RecalcPrice (List<decimal> docCodes);

    public Task<List<NomenklQuantityInfo>> GetNomenklQuantityAsync(decimal skladDC, decimal nomDC, DateTime dateStart,
        DateTime dateEnd);

}
