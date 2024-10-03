using System.Collections.Generic;
using ServiceStack.Model;

namespace ServiceStack.Redis;

public interface IRedisSet
    : ICollection<string>, IHasStringId
{
    List<string> GetRangeFromSortedSet(int startingFrom, int endingAt);
    HashSet<string> GetAll();
    string Pop();
    void Move(string value, IRedisSet toSet);
    HashSet<string> Intersect(params IRedisSet[] withSets);
    void StoreIntersect(params IRedisSet[] withSets);
    HashSet<string> Union(params IRedisSet[] withSets);
    void StoreUnion(params IRedisSet[] withSets);
    HashSet<string> Diff(IRedisSet[] withSets);
    void StoreDiff(IRedisSet fromSet, params IRedisSet[] withSets);
    string GetRandomEntry();
}
