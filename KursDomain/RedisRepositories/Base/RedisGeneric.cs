using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using KursDomain.ICommon;
using ServiceStack.Redis;

namespace KursDomain.RedisRepositories.Base;

public class RedisGeneric<T> : IRedisGeneric<T>
{
    private readonly RedisManagerPool redisManager =
        new RedisManagerPool(ConfigurationManager.AppSettings["redis.connection"]);

    public void UpdateListGuid<T>(IEnumerable<T> list, DateTime? nowFix) where T : IDocGuid
    {
        throw new NotImplementedException();
    }

    public void AddOrUpdateGuid<T>(T item) where T : IDocGuid
    {
        throw new NotImplementedException();
    }

    public void DropAllGuid<T>() where T : IDocGuid
    {
        throw new NotImplementedException();
    }

    public void DropGuid<T>(Guid id) where T : IDocGuid
    {
        throw new NotImplementedException();
    }

    public T GetItemGuid<T>(Guid dc) where T : IDocGuid
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> GetAllGuid<T>() where T : IDocGuid
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> GetListGuid<T>(IEnumerable<Guid> ids) where T : IDocGuid
    {
        throw new NotImplementedException();
    }

    public void UpdateList<T>(IEnumerable<T> list, DateTime? nowFix) where T : IDocCode
    {
        throw new NotImplementedException();
    }

    public void AddOrUpdate<T>(T item) where T : IDocCode
    {
        throw new NotImplementedException();
    }

    public void DropAll<T>() where T : IDocCode
    {
        throw new NotImplementedException();
    }

    public void Drop<T>(decimal dc) where T : IDocCode
    {
        throw new NotImplementedException();
    }

    public T GetItem<T>(decimal dc) where T : IDocCode
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> GetAll<T>() where T : IDocCode
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> GetList<T>(IEnumerable<decimal> dcs) where T : IDocCode
    {
        if (dcs is null) return Enumerable.Empty<T>();
        var idList = dcs.ToList();
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll().Where(_ => idList.Contains(_.DocCode));
            return list;
        }
    }
}
