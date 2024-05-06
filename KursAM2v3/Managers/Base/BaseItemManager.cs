using System;
using System.Collections.Generic;

namespace KursAM2.Managers.Base
{
    public abstract class BaseItemManager<T> : IItemManager<T>
    {
        public virtual bool Delete(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public virtual List<T> LoadList()
        {
            throw new NotImplementedException();
        }

        public virtual T Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual T Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual T New(decimal newDC =-1)
        {
            throw new NotImplementedException();
        }

        public virtual T NewCopy(T item, decimal newDC = -1)
        {
            throw new NotImplementedException();
        }
        
        public virtual T New(T parentItem = default, decimal newDC = -1)
        {
            throw new NotImplementedException();
        }

        
        public virtual bool Save(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

       
    }
}
