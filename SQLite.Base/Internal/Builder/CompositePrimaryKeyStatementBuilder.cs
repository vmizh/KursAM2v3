﻿using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using SQLite.Base.Internal.Statement;

namespace SQLite.Base.Internal.Builder
{
    internal class CompositePrimaryKeyStatementBuilder : IStatementBuilder<CompositePrimaryKeyStatement>
    {
        private readonly IEnumerable<EdmMember> keyMembers;

        public CompositePrimaryKeyStatementBuilder(IEnumerable<EdmMember> keyMembers)
        {
            this.keyMembers = keyMembers;
        }

        public CompositePrimaryKeyStatement BuildStatement()
        {
            return new CompositePrimaryKeyStatement(keyMembers.Select(km => km.Name));
        }
    }
}
