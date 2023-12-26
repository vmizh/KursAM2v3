using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursDomain.Repository.Base;

public interface  IBaseRepository<T, I> 
{
    KursContext.KursContext Context { get; set; }
    T Get(I id);
    ICollection<T> GetAll();

    void Save(T obj);
    void Delete(I id);
    void Delete(T obj);

    void Save(IEnumerable<T> objects);
    void Delete(IEnumerable<I> ids);
    void Delete(IEnumerable<T> objects);
}


public interface  IBaseRepositoryAsync<T, I> 
{
    KursContext.KursContext Context { get; set; }
    Task<T> GetAsync(I id);
    Task<ICollection<T>> GetAllAsync();

    Task SaveAsync(T obj);
    Task DeleteAsync(I id);
    Task DeleteAsync(T obj);

    Task SaveAsync(IEnumerable<T> objects);
    Task DeleteAsync(IEnumerable<I> ids);
    Task DeleteAsync(IEnumerable<T> objects);
}
