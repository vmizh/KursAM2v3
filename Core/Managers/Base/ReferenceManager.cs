using System;
using System.Collections.Generic;

namespace KursAM2.Managers.Base
{
    public abstract class ReferenceManager<T> where T : class
    {
        public abstract List<T> GetDocuments();
        public abstract List<T> GetDocuments(DateTime dateStart, DateTime dateEnd);
        public abstract List<T> GetDocuments(DateTime dateStart, DateTime dateEnd, string searchText);
    }
}