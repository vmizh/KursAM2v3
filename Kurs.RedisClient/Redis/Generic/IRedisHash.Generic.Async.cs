using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Model;

namespace ServiceStack.Redis.Generic;

public interface IRedisHashAsync<TKey, TValue> : IAsyncEnumerable<KeyValuePair<TKey, TValue>>, IHasStringId
{
    ValueTask<Dictionary<TKey, TValue>> GetAllAsync(CancellationToken token = default);

    ValueTask<int> CountAsync(CancellationToken token = default);
    ValueTask AddAsync(KeyValuePair<TKey, TValue> item, CancellationToken token = default);
    ValueTask AddAsync(TKey key, TValue value, CancellationToken token = default);
    ValueTask ClearAsync(CancellationToken token = default);
    ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken token = default);
    ValueTask<bool> RemoveAsync(TKey key, CancellationToken token = default);
}
