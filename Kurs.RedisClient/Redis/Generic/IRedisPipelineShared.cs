using System;
using ServiceStack.Redis.Pipeline;

namespace ServiceStack.Redis.Generic;

/// <summary>
/// Pipeline interface shared by typed and non-typed pipelines
/// </summary>
public interface IRedisPipelineShared : IDisposable, IRedisQueueCompletableOperation
{
    void Flush();
    bool Replay();
}
