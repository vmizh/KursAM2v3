using System.Data.Entity;
using SQLite.Base.Entity;
using SQLite.Base.Public.DbIninitializers;

namespace SQLite.Base
{
    public class LocalDBInitializer : SqliteDropCreateDatabaseWhenModelChanges<LocalDbContext>
    {

        public LocalDBInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder, typeof(CustomHistory))
        { }

        protected override void Seed(LocalDbContext context)
        {
            // Here you can seed your core data if you have any.
        }
    }
}