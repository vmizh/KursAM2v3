using System;
using System.Data.Entity;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Diagnostics.CodeAnalysis;
using SQLite.Base.Entity;

namespace SQLite.Base
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class LocalDbContext : DbContext
    {

        private static SQLiteConnection CreateConnection()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var builder = (SQLiteConnectionStringBuilder)SQLiteProviderFactory.Instance.CreateConnectionStringBuilder();
            if (builder == null) return null;
            builder.DataSource = $"{appData}\\KursAM2v3\\DB\\Local.Db";
            builder.FailIfMissing = false;
            builder.ForeignKeys = true;
            return new SQLiteConnection(builder.ToString());
        }

        public LocalDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Configure();
        }
        public LocalDbContext()
            : base(CreateConnection(), false)
        {
            Configure();
        }

        //public LocalDbContext(DbConnection connection, bool contextOwnsConnection)
        //    : base(connection, contextOwnsConnection)
        //{
        //    Configure();
        //}

        private void Configure()
        {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;
        }

        public class ModelConfiguration
        {
            public static void Configure(DbModelBuilder modelBuilder)
            {
                ConfigureDefault(modelBuilder);
            }

            private static void ConfigureDefault(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<KontragentCash>();
                modelBuilder.Entity<Employee>();
                modelBuilder.Entity<LoginStart>();
            }
        }

        public DbSet<KontragentCash> KontragentCashes { set; get; }
        public DbSet<Employee> Employees { set; get; }
        public DbSet<LoginStart> LoginStarts { set; get; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ModelConfiguration.Configure(modelBuilder);
            var initializer = new LocalDBInitializer(modelBuilder);
            Database.SetInitializer(initializer);
        }
        
    }
}