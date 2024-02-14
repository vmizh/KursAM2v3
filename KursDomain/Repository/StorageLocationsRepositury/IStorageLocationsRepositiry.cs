using System;
using System.Collections.Generic;
using Data;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.StorageLocationsRepositury;

public interface IStorageLocationsRepositiry : IKursGenericRepository<StorageLocations, Guid>
{
    IEnumerable<StorageLocations> GetAll();
}
