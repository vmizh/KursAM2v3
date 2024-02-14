using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using KursDomain.ICommon;

namespace KursDomain.Repository.Base;

public interface IKursGenericRepository<T,I>
{
    Task<T> GetByIdAsync(I id);
    Task<IEnumerable<T>> GetAllAsync(bool isUntrack = false);
    Task SaveAsync();
    bool HasChanges();
    void Add(T model);
    void Remove(T model);
    RowStatus GetRowStatus(T model);
    void BeginTransaction();
    void CommitTransaction();
    void ContextRollback();
    void Rollback();

    EntityState GetEntityState(object  entity);
    List<DbEntityEntry> GetEntites();
}
