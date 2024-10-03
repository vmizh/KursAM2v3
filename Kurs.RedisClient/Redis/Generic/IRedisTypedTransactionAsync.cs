using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.Redis.Generic;

/// <summary>
/// Redis transaction for typed client
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRedisTypedTransactionAsync<T> : IRedisTypedQueueableOperationAsync<T>, IAsyncDisposable
{
    ValueTask<bool> CommitAsync(CancellationToken token = default);
    ValueTask RollbackAsync(CancellationToken token = default);
}
