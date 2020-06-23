using System;
using System.Linq;
using Core.Repository.Base;
using Data;

namespace KursRepozit.Repositories
{
    public interface IDataSourcesRepository : IGenericRepository<DataSources>
    {
        DataSources GetById(Guid id);
        DataSources GetByName(string name);
    }

    public class DataSourcesKursSystemRepository : GenericKursSystemRepository<DataSources>, IDataSourcesRepository
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

