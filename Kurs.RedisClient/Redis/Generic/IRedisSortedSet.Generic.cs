﻿using System.Collections.Generic;
using ServiceStack.Model;

namespace ServiceStack.Redis.Generic;

public interface IRedisSortedSet<T> : ICollection<T>, IHasStringId
{
    void Add(T item, double score);
    T PopItemWithHighestScore();
    T PopItemWithLowestScore();
    double IncrementItem(T item, double incrementBy);
    int IndexOf(T item);
    long IndexOfDescending(T item);
    List<T> GetAll();
    List<T> GetAllDescending();
    List<T> GetRange(int fromRank, int toRank);
    List<T> GetRangeByLowestScore(double fromScore, double toScore);
    List<T> GetRangeByLowestScore(double fromScore, double toScore, int? skip, int? take);
    List<T> GetRangeByHighestScore(double fromScore, double toScore);
    List<T> GetRangeByHighestScore(double fromScore, double toScore, int? skip, int? take);
    long RemoveRange(int minRank, int maxRank);
    long RemoveRangeByScore(double fromScore, double toScore);
    double GetItemScore(T item);
    long PopulateWithIntersectOf(params IRedisSortedSet<T>[] setIds);
    long PopulateWithIntersectOf(IRedisSortedSet<T>[] setIds, string[] args);
    long PopulateWithUnionOf(params IRedisSortedSet<T>[] setIds);
    long PopulateWithUnionOf(IRedisSortedSet<T>[] setIds, string[] args);
}
