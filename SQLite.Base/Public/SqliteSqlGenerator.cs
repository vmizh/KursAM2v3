﻿using System.Data.Entity.Core.Metadata.Edm;
using SQLite.Base.Internal.Builder;
using SQLite.Base.Internal.Statement;

namespace SQLite.Base.Public
{
    /// <summary>
    /// Generates the SQL statement to create a database, based on a <see cref="EdmModel"/>.
    /// </summary>
    public class SqliteSqlGenerator : ISqlGenerator
    {
        /// <summary>
        /// Generates the SQL statement, based on the <see cref="EdmModel"/>.
        /// </summary>
        public string Generate(EdmModel storeModel)
        {
            IStatementBuilder<CreateDatabaseStatement> statementBuilder = new CreateDatabaseStatementBuilder(storeModel);
            IStatement statement = statementBuilder.BuildStatement();
            return statement.CreateStatement();
        }
    }
}
