using System.Collections.Generic;
using System;
using System.Collections;

namespace ServiceStack.Data;

public interface IEntityStore : IDisposable
{
    T GetById<T>(object id);

    IList<T> GetByIds<T>(ICollection ids);

    T Store<T>(T entity);

    void StoreAll<TEntity>(IEnumerable<TEntity> entities);

    void Delete<T>(T entity);

    void DeleteById<T>(object id);

    void DeleteByIds<T>(ICollection ids);

    void DeleteAll<TEntity>();
}
/// <summary>
/// For providers that want a cleaner API with a little more perf
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEntityStore<T>
{
    T GetById(object id);

    IList<T> GetByIds(IEnumerable ids);

    IList<T> GetAll();

    T Store(T entity);

    void StoreAll(IEnumerable<T> entities);

    void Delete(T entity);

    void DeleteById(object id);

    void DeleteByIds(IEnumerable ids);

    void DeleteAll();
}
