using KursDomain.ICommon;
using System.Collections.Generic;
using System;

namespace KursDomain.RedisRepositories.Base;

public interface IRedisGeneric<T>
{
    void UpdateListGuid<T>(IEnumerable<T> list, DateTime? nowFix) where T : IDocGuid;
    void AddOrUpdateGuid<T>(T item) where T : IDocGuid;
    void DropAllGuid<T>() where T : IDocGuid;
    void DropGuid<T>(Guid id) where T : IDocGuid;
    T GetItemGuid<T>(Guid dc) where T : IDocGuid;
    IEnumerable<T> GetAllGuid<T>() where T : IDocGuid;

    IEnumerable<T> GetListGuid<T>(IEnumerable<Guid> ids) where T : IDocGuid;
   
    void UpdateList<T>(IEnumerable<T> list, DateTime? nowFix) where T : IDocCode;
    void AddOrUpdate<T>(T item) where T : IDocCode;
    void DropAll<T>() where T : IDocCode;
    void Drop<T>(decimal dc) where T : IDocCode;
    T GetItem<T>(decimal dc) where T : IDocCode;
    IEnumerable<T> GetAll<T>() where T : IDocCode;

    IEnumerable<T> GetList<T>(IEnumerable<decimal> dcs) where T : IDocCode;
}
