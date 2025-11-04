using System;
using System.Collections.Generic;
using System.Linq;

namespace KursDomain.References.RedisCache;

public static class RedisCacheExtensions
{
    public static void AddCacheKey(this List<CachKey> caches, CachKey cacheKey)
    {
        var dcOlds = caches.Where(_ => _.DocCode == cacheKey.DocCode).ToList();
        var idOlds = caches.Where(_ => _.Id == cacheKey.Id).ToList();
        foreach (var item in dcOlds.Where(item => dcOlds.Count > 0))
        {
            caches.Remove(item);
        }
        foreach (var item in idOlds.Where(item => idOlds.Count > 0))
        {
            caches.Remove(item);
        }
        caches.Add(cacheKey);
    }

    public static void AddCacheKey(this HashSet<CachKey> caches, CachKey cacheKey)
    {
        var dcOlds = caches.Where(_ => _.DocCode == cacheKey.DocCode).ToList();
        var idOlds = caches.Where(_ => _.Id == cacheKey.Id).ToList();
        foreach (var item in dcOlds.Where(item => dcOlds.Count > 0))
        {
            caches.Remove(item);
        }
        foreach (var item in idOlds.Where(item => idOlds.Count > 0))
        {
            caches.Remove(item);
        }
        caches.Add(cacheKey);
    }
}
