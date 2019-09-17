using System.Collections.Generic;

namespace SQLite.Base.Internal.Statement.ColumnConstraint
{
    interface IColumnConstraintCollection : ICollection<IColumnConstraint>, IColumnConstraint
    {
    }
}
