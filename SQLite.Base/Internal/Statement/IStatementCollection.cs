using System.Collections.Generic;

namespace SQLite.Base.Internal.Statement
{
    public interface IStatementCollection : IStatement, ICollection<IStatement>
    {
    }
}
