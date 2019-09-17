using System;
using SQLite.Base.Public.Entities;

namespace SQLite.Base.Internal.Utility
{
    internal class HistoryEntityTypeValidator
    {
        public static void EnsureValidType(Type historyEntityType)
        {
            if (!typeof(IHistory).IsAssignableFrom(historyEntityType))
            {
                throw new InvalidOperationException("The Type " + historyEntityType.Name + " does not implement the IHistory interface.");
            }
            if (historyEntityType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidOperationException("The Type " + historyEntityType.Name + " does not provide an parameterless constructor.");
            }
        }
    }
}
