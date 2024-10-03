using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Model;

namespace ServiceStack.Redis.Generic;

public interface IRedisSetAsync<T> : IAsyncEnumerable<T>, IHasStringId
{
    ValueTask<int> CountAsync(CancellationToken token = default);
    ValueTask<List<T>> SortAsync(int startingFrom, int endingAt, CancellationToken token = default);
    ValueTask<HashSet<T>> GetAllAsync(CancellationToken token = default);
    ValueTask<T> PopRandomItemAsync(CancellationToken token = default);
    ValueTask<T> GetRandomItemAsync(CancellationToken token = default);
    ValueTask MoveToAsync(T item, IRedisSetAsync<T> toSet, CancellationToken token = default);
    ValueTask PopulateWithIntersectOfAsync(IRedisSetAsync<T>[] sets, CancellationToken token = default);
    ValueTask PopulateWithIntersectOfAsync(params IRedisSetAsync<T>[] sets); // convenience API
    ValueTask PopulateWithUnionOfAsync(IRedisSetAsync<T>[] sets, CancellationToken token = default);
    ValueTask PopulateWithUnionOfAsync(params IRedisSetAsync<T>[] sets); // convenience API
    ValueTask GetDifferencesAsync(IRedisSetAsync<T>[] withSets, CancellationToken token = default);
    ValueTask GetDifferencesAsync(params IRedisSetAsync<T>[] withSets); // convenience API
    ValueTask PopulateWithDifferencesOfAsync(IRedisSetAsync<T> fromSet, IRedisSetAsync<T>[] withSets, CancellationToken token = default);
    ValueTask PopulateWithDifferencesOfAsync(IRedisSetAsync<T> fromSet, params IRedisSetAsync<T>[] withSets); // convenience API
    ValueTask ClearAsync(CancellationToken token = default);
    ValueTask<bool> ContainsAsync(T item, CancellationToken token = default);
    ValueTask<bool> RemoveAsync(T item, CancellationToken token = default);
    ValueTask AddAsync(T item, CancellationToken token = default);
}
