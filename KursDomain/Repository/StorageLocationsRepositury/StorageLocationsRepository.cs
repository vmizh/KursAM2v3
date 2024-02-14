using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.StorageLocationsRepositury;

public class StorageLocationsRepository : KursGenericRepository<StorageLocations, ALFAMEDIAEntities, Guid>,
    IStorageLocationsRepositiry
{
    public StorageLocationsRepository(ALFAMEDIAEntities context) : base(context)
    {
    }

    public IEnumerable<StorageLocations> GetAll()
    {
        return Context.Set<StorageLocations>().ToList();
    }
}
