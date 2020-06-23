using System;

namespace KursRepozit.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IDataSourcesRepository DataSources { get; }
    }
}