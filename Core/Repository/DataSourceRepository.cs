using System;
using System.Linq;
using Data;
using KursRepozit.Repository.Base;

namespace KursRepozit.Repository
{
    public interface IDataSourcesRepository : IGenericRepository<DataSources>
    {
        DataSources GetById(Guid id);
        DataSources GetByName(string name);
    }

    public class DataSourcesRepository : GenericRepository<DataSources>, IDataSourcesRepository
    {
        public DataSourcesRepository(IUnitOfWork<KursSystemEntities> unitOfWork)
            : base(unitOfWork)
        {
        }

        public DataSourcesRepository(KursSystemEntities context)
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

