using System;
using ServiceStack.Caching;

namespace ServiceStack.Redis;

public interface IRedisClientsManager : IDisposable
{
    /// <summary>
    /// Resolver strategy for resolving hosts and creating clients
    /// </summary>
    IRedisResolver RedisResolver { get; }
        
    /// <summary>
    /// Returns a Read/Write client (The default) using the hosts defined in ReadWriteHosts
    /// </summary>
    /// <returns></returns>
    IRedisClient GetClient();

    /// <summary>
    /// Returns a ReadOnly client using the hosts defined in ReadOnlyHosts.
    /// </summary>
    /// <returns></returns>
    IRedisClient GetReadOnlyClient();

    /// <summary>
    /// Returns a Read/Write ICacheClient (The default) using the hosts defined in ReadWriteHosts
    /// </summary>
    /// <returns></returns>
    ICacheClient GetCacheClient();

    /// <summary>
    /// Returns a ReadOnly ICacheClient using the hosts defined in ReadOnlyHosts.
    /// </summary>
    /// <returns></returns>
    ICacheClient GetReadOnlyCacheClient();
}
