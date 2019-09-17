using System.Data.Entity;
using System.Data.SQLite;

namespace LocalDataBase
{
    public class LocalDBContext : DbContext
    {
        public LocalDBContext()  { }

        public LocalDBContext(string connectionString) : base(new SQLiteConnection(connectionString),true)
        {

        }
        public DbSet<KontragentCash> KontragentCashes { set; get; }
    }
}