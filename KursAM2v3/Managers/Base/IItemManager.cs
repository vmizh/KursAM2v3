using System;
using System.Collections.Generic;

namespace KursAM2.Managers.Base
{
    public interface IItemManager<T>
    {
        List<T> LoadList();
        T Load(decimal dc);
        T Load(Guid id);
        T New(decimal newDC = -1);
        T NewCopy(T item, decimal newDC = -1);

        bool Save(IEnumerable<T> items);
        bool Delete(IEnumerable<T> items);
    }
}
