using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using KursDomain.ICommon;

namespace KursDomain.Repository.Base;

public class KursGenericRepository<TEntity, TContext, TIdentity> : IKursGenericRepository<TEntity, TIdentity>
    where TEntity : class
    where TContext : DbContext
{
    protected readonly TContext Context;
    private DbContextTransaction _tran;

    protected KursGenericRepository(TContext context)
    {
        Context = context;
    }

    public void Add(TEntity model)
    {
        Context.Set<TEntity>().Add(model);
    }

    public virtual async Task<TEntity> GetByIdAsync(TIdentity id)
    {
        return await Context.Set<TEntity>().FindAsync(id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(bool isUntrack = false)
    {
        if (isUntrack)
            return await Context.Set<TEntity>().AsNoTracking().ToListAsync();
        return await Context.Set<TEntity>().ToListAsync();
    }

    public bool HasChanges()
    {
        return Context.ChangeTracker.HasChanges();
    }

    public void Remove(TEntity model)
    {
        Context.Set<TEntity>().Remove(model);
    }

    public virtual RowStatus GetRowStatus(TEntity model)
    {
        return RowStatus.NotDefinition;
    }

    public void BeginTransaction()
    {
        _tran = Context.Database.BeginTransaction();
    }

    public void CommitTransaction()
    {
        if (_tran != null)
        {
            _tran.Commit();
            _tran = null;
        }
    }

    public void ContextRollback()
    {
        var changedEntries = Context.ChangeTracker.Entries()
            .Where(x => x.State != EntityState.Unchanged).ToList();
        foreach (var entry in changedEntries)
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = EntityState.Unchanged;
                    break;
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    break;
            }
    }

    public void Rollback()
    {
        if (_tran != null) _tran.Rollback();
        _tran = null;
    }

    public List<DbEntityEntry> GetEntites()
    {
        return Context.ChangeTracker.Entries().ToList();
    }

    public EntityState GetEntityState(object entity)
    {
        return Context.Entry(entity).State;
    }

    public async Task SaveAsync()
    {
        await Context.SaveChangesAsync();
    }

    public void Remove(IEnumerable<TEntity> modelList)
    {
        foreach (var model in modelList) Context.Set<TEntity>().Remove(model);
    }
}
