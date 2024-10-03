using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Redis.Pipeline;

namespace ServiceStack.Redis;

/// <summary>
/// Interface to redis transaction
/// </summary>
public interface IRedisTransactionAsync
    : IRedisTransactionBaseAsync, IRedisQueueableOperationAsync, IAsyncDisposable
{
    ValueTask<bool> CommitAsync(CancellationToken token = default);
    ValueTask RollbackAsync(CancellationToken token = default);
}
