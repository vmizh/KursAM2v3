using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursDomain.Repository.Base;

public interface IKursGenericRepository<T,I>
{
    Task<T> GetByIdAsync(I id);
    Task<IEnumerable<T>> GetAllAsync(bool isUntrack = false);
    Task SaveAsync();
    bool HasChanges();
    void Add(T model);
    void Remove(T model);

    void Rollback();
}
