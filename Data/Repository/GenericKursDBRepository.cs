using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;

namespace Data.Repository
{
    public class GenericKursDBRepository<T> : IGenericRepository<T>, IDisposable where T : class
    {
        private IDbSet<T> entities;
        private string errorMessage = string.Empty;
        private bool isDisposed;

        public GenericKursDBRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork)
            : this(unitOfWork.Context)
        {
            Context = unitOfWork.Context;
        }

        public GenericKursDBRepository(ALFAMEDIAEntities context)
        {
            isDisposed = false;
            Context = context;
        }

        public ALFAMEDIAEntities Context { get; set; }

        public IQueryable<T> Table => Entities;
        private IDbSet<T> Entities => entities ?? (entities = Context.Set<T>());

        public void Dispose()
        {
            Context?.Dispose();
            isDisposed = true;
        }

        public IEnumerable<T> GetAll()
        {
            return Entities.ToList();
        }

        public T GetById(object id)
        {
            return Entities.Find(id);
        }

        public void Insert(T entity)
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

        public void Update(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                if (Context == null || isDisposed)
                    Context = new ALFAMEDIAEntities();
                SetEntryModified(entity);
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

        public void Delete(T entity)
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

        public void Refresh(T entity)
        {
            if (entity == null) return;
            Context.Entry(entity).Reload();
        }

        public void DetachObjects()
        {
            foreach (var en in Context.ChangeTracker.Entries()) en.State = EntityState.Detached;
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

        public void SetEntryModified(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }
    }
}