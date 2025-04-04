﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Caching;

namespace ServiceStack.Redis;

public interface IRedisClientsManagerAsync : IAsyncDisposable
{
    /// <summary>
    /// Resolver strategy for resolving hosts and creating clients
    /// </summary>
    IRedisResolver RedisResolver { get; }
        
    /// <summary>
    /// Returns a Read/Write client (The default) using the hosts defined in ReadWriteHosts
    /// </summary>
    /// <returns></returns>
    ValueTask<IRedisClientAsync> GetClientAsync(CancellationToken token = default);

    /// <summary>
    /// Returns a ReadOnly client using the hosts defined in ReadOnlyHosts.
    /// </summary>
    /// <returns></returns>
    ValueTask<IRedisClientAsync> GetReadOnlyClientAsync(CancellationToken token = default);

    /// <summary>
    /// Returns a Read/Write ICacheClient (The default) using the hosts defined in ReadWriteHosts
    /// </summary>
    /// <returns></returns>
    ValueTask<ICacheClientAsync> GetCacheClientAsync(CancellationToken token = default);

    /// <summary>
    /// Returns a ReadOnly ICacheClient using the hosts defined in ReadOnlyHosts.
    /// </summary>
    /// <returns></returns>
    ValueTask<ICacheClientAsync> GetReadOnlyCacheClientAsync(CancellationToken token = default);
}
