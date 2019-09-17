﻿using System.Data.Entity;
using SQLite.Base.Internal.Utility;

namespace SQLite.Base.Public.DbIninitializers
{
    /// <summary>
    /// An implementation of <see cref="IDatabaseInitializer{TContext}"/> that will always recreate and optionally re-seed the 
    /// database the first time that a context is used in the app domain. To seed the database, create a derived class and override the Seed method.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public class SqliteDropCreateDatabaseAlways<TContext> : SqliteInitializerBase<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDropCreateDatabaseAlways{TContext}"/> class.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        public SqliteDropCreateDatabaseAlways(DbModelBuilder modelBuilder)
            : base(modelBuilder)
        { }

        /// <summary>
        /// Initialize the database for the given context.
        /// Generates the SQLite-DDL from the model and executs it against the database.
        /// All actions are be executed in transactions.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void InitializeDatabase(TContext context)
        {
            string databseFilePath = GetDatabasePathFromContext(context);

            bool exists = InMemoryAwareFile.Exists(databseFilePath);
            if (exists)
            {
                InMemoryAwareFile.Delete(databseFilePath);
            }

            base.InitializeDatabase(context);
        }
    }
}
