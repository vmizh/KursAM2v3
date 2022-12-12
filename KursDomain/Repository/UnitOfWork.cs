using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace KursDomain.Repository
{
    public interface IUnitOfWork<out TContext>
        where TContext : DbContext
    {
        TContext Context { get; }
        void CreateTransaction();
        void Commit();
        void Rollback();
        void Save();
    }

    public sealed class UnitOfWork<TContext> : IUnitOfWork<TContext>, IDisposable
        where TContext : DbContext, new()
    {
        #region Properties

        public TContext Context { get; }

        #endregion

        #region Fields

        private bool disposed;
        private string errorMessage = string.Empty;
        private DbContextTransaction objTran;
        private Dictionary<string, object> repositories;

        #endregion

        #region Constructors

        public UnitOfWork()
        {
            Context = new TContext();
        }

        public UnitOfWork(TContext ctx)
        {
            Context = ctx;
        }

        public void Dispose()
        {
            Dispose(true);
            // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        public void CreateTransaction()
        {
            objTran = Context.Database.BeginTransaction();
        }

        public void Commit()
        {
            objTran.Commit();
        }

        public void Rollback()
        {
            if (objTran?.UnderlyingTransaction.Connection == null)
                return;
            objTran.Rollback();
            objTran.Dispose();
        }

        public void Save()
        {
            try
            {
                Context.SaveChanges();
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

        private void Dispose(bool disposing)
        {
            if (!disposed)
                if (disposing)
                    Context.Dispose();
            disposed = true;
        }

        public KursDomain.Repository.GenericKursDBRepository<T> GenericRepository<T>() where T : class
        {
            if (repositories == null)
                repositories = new Dictionary<string, object>();
            var type = typeof(T).Name;
            if (repositories.ContainsKey(type)) return (KursDomain.Repository.GenericKursDBRepository<T>)repositories[type];
            var repositoryType = typeof(KursDomain.Repository.GenericKursDBRepository<T>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)),
                Context);
            repositories.Add(type, repositoryInstance);
            return (KursDomain.Repository.GenericKursDBRepository<T>)repositories[type];
        }

        #endregion
    }
}
