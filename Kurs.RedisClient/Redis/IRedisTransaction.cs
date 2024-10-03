using System;
using ServiceStack.Redis.Pipeline;

namespace ServiceStack.Redis;

/// <summary>
/// Interface to redis transaction
/// </summary>
public interface IRedisTransaction
    : IRedisTransactionBase, IRedisQueueableOperation, IDisposable
{
    bool Commit();
    void Rollback();
}
