using System;
using System.Collections.Generic;
using ServiceStack.Model;

namespace ServiceStack.Redis;

public interface IRedisList
    : IList<string>, IHasStringId
{
    List<string> GetAll();
    List<string> GetRange(int startingFrom, int endingAt);
    List<string> GetRangeFromSortedList(int startingFrom, int endingAt);
    void RemoveAll();
    void Trim(int keepStartingFrom, int keepEndingAt);
    long RemoveValue(string value);
    long RemoveValue(string value, int noOfMatches);

    void Prepend(string value);
    void Append(string value);
    string RemoveStart();
    string BlockingRemoveStart(TimeSpan? timeOut);
    string RemoveEnd();

    void Enqueue(string value);
    string Dequeue();
    string BlockingDequeue(TimeSpan? timeOut);

    void Push(string value);
    string Pop();
    string BlockingPop(TimeSpan? timeOut);
    string PopAndPush(IRedisList toList);
}
