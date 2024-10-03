using System.Collections.Generic;
using ServiceStack.Model;

namespace ServiceStack.Redis.Generic;

public interface IRedisHash<TKey, TValue> : IDictionary<TKey, TValue>, IHasStringId
{
    Dictionary<TKey, TValue> GetAll();
}
