using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using Data;

namespace Core.Repository.Base
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(object id);
        void Insert(T obj);
        void Update(T obj);
        void Delete(T obj);
    }

    public class GenericKursSystemRepository<T> : IGenericRepository<T>, IDisposable where T : class
    {
        private IDbSet<T> entities;
        protected string errorMessage = string.Empty;
        protected bool isDisposed;

        public GenericKursSystemRepository(IUnitOfWork<KursSystemEntities> unitOfWork)
            : this(unitOfWork.Context)
        {
        }

        public GenericKursSystemRepository(KursSystemEntities context)
        {
            isDisposed = false;
            Context = context;
        }

        public KursSystemEntities Context { get; set; }

        public virtual IQueryable<T> Table => Entities;

        // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
        protected virtual IDbSet<T> Entities => entities ?? (entities = Context.Set<T>());

        public void Dispose()
        {
            Context?.Dispose();
            isDisposed = true;
        }

        public virtual IEnumerable<T> GetAll()
        {
            return Entities.ToList();
        }

        public virtual T GetById(object id)
        {
            return Entities.Find(id);
        }

        public virtual void Insert(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                Entities.Add(entity);
                if (Context == null || isDisposed)
                    Context = new KursSystemEntities();
                //Context.SaveChanges(); commented out call to SaveChanges as Context save changes will be 
                //called with Unit of work
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                foreach (var validationError in validationErrors.ValidationErrors)
                    errorMessage += $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}" +
                                    Environment.NewLine;
                throw new Exception(errorMessage, dbEx);
            }
        }

        public virtual void Update(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                if (Context == null || isDisposed)
                    Context = new KursSystemEntities();
                SetEntryModified(entity);
                //Context.SaveChanges(); commented out call to SaveChanges as Context save changes will be called with Unit of work
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                foreach (var validationError in validationErrors.ValidationErrors)
                    errorMessage += Environment.NewLine +
                                    $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}";
                throw new Exception(errorMessage, dbEx);
            }
        }

        public virtual void Delete(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                if (Context == null || isDisposed)
                    Context = new KursSystemEntities();
                Entities.Remove(entity);
                //Context.SaveChanges(); commented out call to SaveChanges as Context save changes will be called with Unit of work
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                foreach (var validationError in validationErrors.ValidationErrors)
                    errorMessage += Environment.NewLine +
                                    $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}";
                throw new Exception(errorMessage, dbEx);
            }
        }

        // ReSharper disable once ParameterHidesMember
        public void BulkInsert(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null) throw new ArgumentNullException(nameof(entities));
                Context.Configuration.AutoDetectChangesEnabled = false;
                Context.Set<T>().AddRange(entities);
                Context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                foreach (var validationError in validationErrors.ValidationErrors)
                    errorMessage += $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}"
                                    + Environment.NewLine;
                throw new Exception(errorMessage, dbEx);
            }
        }

        public virtual void SetEntryModified(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }
    }

    public class GenericKursRepository<T> : IGenericRepository<T>, IDisposable where T : class
    {
        private IDbSet<T> entities;
        protected string errorMessage = string.Empty;
        protected bool isDisposed;

        public GenericKursRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork)
            : this(unitOfWork.Context)
        {
            Context = unitOfWork.Context;
        }

        public GenericKursRepository(ALFAMEDIAEntities context)
        {
            isDisposed = false;
            Context = context;
        }

        public ALFAMEDIAEntities Context { get; set; }

        public virtual IQueryable<T> Table => Entities;

        protected virtual IDbSet<T> Entities => entities ?? (entities = Context.Set<T>());

        public void Dispose()
        {
            Context?.Dispose();
            isDisposed = true;
        }

        public virtual IEnumerable<T> GetAll()
        {
            return Entities.ToList();
        }

        public virtual T GetById(object id)
        {
            return Entities.Find(id);
        }

        public virtual void Insert(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                Entities.Add(entity);
                if (Context == null || isDisposed)
                    Context = new ALFAMEDIAEntities();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        errorMessage += $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}" +
                                        Environment.NewLine;
                throw new Exception(errorMessage, dbEx);
            }
        }

        public virtual void Update(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                if (Context == null || isDisposed)
                    Context = new ALFAMEDIAEntities();
                SetEntryModified(entity);
                //Context.SaveChanges(); commented out call to SaveChanges as Context save changes will be called with Unit of work
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        errorMessage += Environment.NewLine +
                                        $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}";
                throw new Exception(errorMessage, dbEx);
            }
        }

        public virtual void Delete(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                if (Context == null || isDisposed)
                    Context = new ALFAMEDIAEntities();
                Entities.Remove(entity);
                //Context.SaveChanges(); commented out call to SaveChanges as Context save changes will be called with Unit of work
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        errorMessage += Environment.NewLine +
                                        $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}";
                throw new Exception(errorMessage, dbEx);
            }
        }

        // ReSharper disable once ParameterHidesMember
        public void BulkInsert(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null) throw new ArgumentNullException(nameof(entities));
                Context.Configuration.AutoDetectChangesEnabled = false;
                Context.Set<T>().AddRange(entities);
                Context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        errorMessage += $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}"
                                        + Environment.NewLine;
                throw new Exception(errorMessage, dbEx);
            }
        }

        public virtual void SetEntryModified(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }
    }
}