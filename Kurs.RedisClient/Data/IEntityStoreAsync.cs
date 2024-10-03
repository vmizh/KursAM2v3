using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.Data;

public interface IEntityStoreAsync
{
    Task<T> GetByIdAsync<T>(object id, CancellationToken token = default);

    Task<IList<T>> GetByIdsAsync<T>(ICollection ids, CancellationToken token = default);

    Task<T> StoreAsync<T>(T entity, CancellationToken token = default);

    Task StoreAllAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken token = default);

    Task DeleteAsync<T>(T entity, CancellationToken token = default);

    Task DeleteByIdAsync<T>(object id, CancellationToken token = default);

    Task DeleteByIdsAsync<T>(ICollection ids, CancellationToken token = default);

    Task DeleteAllAsync<TEntity>(CancellationToken token = default);
}

/// <summary>
/// For providers that want a cleaner API with a little more perf
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEntityStoreAsync<T>
{
    Task<T> GetByIdAsync(object id, CancellationToken token = default);

    Task<IList<T>> GetByIdsAsync(IEnumerable ids, CancellationToken token = default);

    Task<IList<T>> GetAllAsync(CancellationToken token = default);

    Task<T> StoreAsync(T entity, CancellationToken token = default);

    Task StoreAllAsync(IEnumerable<T> entities, CancellationToken token = default);

    Task DeleteAsync(T entity, CancellationToken token = default);

    Task DeleteByIdAsync(object id, CancellationToken token = default);

    Task DeleteByIdsAsync(IEnumerable ids, CancellationToken token = default);

    Task DeleteAllAsync(CancellationToken token = default);
}
