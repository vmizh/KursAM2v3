using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Core.Repository.Base;
using Data;
using Data.Repository;

namespace KursRepozit.Repositories
{
    public interface IDataSourcesRepository : IGenericRepository<DataSources>
    {
        DataSources GetById(Guid id);
        DataSources GetByName(string name);
    }

    public class DataSourcesKursSystemRepository : GenericKursSystemDBRepository<DataSources>, IDataSourcesRepository
    {
        public DataSourcesKursSystemRepository(IUnitOfWork<KursSystemEntities> unitOfWork)
            : base(unitOfWork)
        {
        }

        public DataSourcesKursSystemRepository(KursSystemEntities context)
            : base(context)
        {
        }

        public DataSources GetById(Guid id)
        {
            return Context.DataSources.SingleOrDefault(_ => _.Id == id);
        }

        public DataSources GetByName(string name)
        {
            return Context.DataSources.SingleOrDefault(_ => _.Name == name);
        }
    }
}

