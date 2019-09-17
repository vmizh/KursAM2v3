using SQLite.Base.Internal.Statement;

namespace SQLite.Base.Internal.Builder
{
    internal interface IStatementBuilder<out TStatement>
        where TStatement : IStatement
    {
        TStatement BuildStatement();
    }
}
