using System;

namespace ServiceStack.Redis.Generic;

/// <summary>
/// Redis transaction for typed client
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRedisTypedTransaction<T> : IRedisTypedQueueableOperation<T>, IDisposable
{
    bool Commit();
    void Rollback();
}
