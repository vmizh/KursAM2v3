using System.Data.Entity;
using System;
using Data;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.StorageLocationsRepositury;

public class StorageLocationsRepository :  KursGenericRepository<StorageLocations, DbContext, Guid>, IStorageLocationsRepositiry
{
    public StorageLocationsRepository(DbContext context) : base(context)
    {
    }


   
}
