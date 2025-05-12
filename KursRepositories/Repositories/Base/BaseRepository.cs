using System.Data.Entity;
using Data;
using KursDomain;
using StackExchange.Redis;

namespace KursRepositories.Repositories.Base
{
    public abstract class BaseRepository : IBaseRepository
    {
        protected DbContextTransaction myTransaction;
        protected ISubscriber mySubscriber;
        protected ConnectionMultiplexer redis;
        public ALFAMEDIAEntities Context { get; set; } = GlobalOptions.GetEntities();

        public virtual void BeginTransaction()
        {
            myTransaction = Context.Database.BeginTransaction();
        }

        public virtual void CommitTransaction()
        {
            if (myTransaction != null)
                myTransaction.Commit();
        }

        public virtual void RollbackTransaction()
        {
            if (myTransaction != null)
                myTransaction.Rollback();
        }

        public virtual void SaveChanges()
        {
            if (Context.ChangeTracker.HasChanges())
                Context.SaveChanges();
        }
    }
}
