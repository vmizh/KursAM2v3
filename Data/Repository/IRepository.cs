using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Data.Repository
{
    public interface IRepository<TModel> where TModel : class
    {
        // Get records by it's primary key
        TModel Get(int id);
        TModel Get(decimal id);
        TModel Get(Guid id);

        // Get all records
        List<TModel> GetAll();

        // Get all records matching a lambda expression
        IQueryable<TModel> Find(Expression<Func<TModel, bool>> predicate);

        // Get the a single matching record or null
        TModel SingleOrDefault(Expression<Func<TModel, bool>> predicate);

        // Add single record
        void Add(TModel entity);

        // Add multiple records
        void AddRange(List<TModel> entities);

        // Remove records
        void Remove(TModel entity);

        // remove multiple records
        void RemoveRange(List<TModel> entities);
        void Update(TModel entity);
        void Update(TModel entity, params string[] propsToUpdate);
    }
}