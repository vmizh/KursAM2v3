using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace SQLite.Base.Public
{
    public interface IDatabaseCreator
    {
        void Create(Database db, DbModel model);
    }
}