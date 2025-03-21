﻿namespace ServiceStack.Text.Pools;

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;

/// <summary>
/// Shared object pool for roslyn
/// 
/// Use this shared pool if only concern is reducing object allocations.
/// if perf of an object pool itself is also a concern, use ObjectPool directly.
/// 
/// For example, if you want to create a million of small objects within a second, 
/// use the ObjectPool directly. it should have much less overhead than using this.
/// </summary>
public static class SharedPools
{
    /// <summary>
    /// pool that uses default constructor with 100 elements pooled
    /// </summary>
    public static ObjectPool<T> BigDefault<T>() where T : class, new()
    {
        return DefaultBigPool<T>.Instance;
    }

    /// <summary>
    /// pool that uses default constructor with 20 elements pooled
    /// </summary>
    public static ObjectPool<T> Default<T>() where T : class, new()
    {
        return DefaultNormalPool<T>.Instance;
    }

    /// <summary>
    /// pool that uses string as key with StringComparer.OrdinalIgnoreCase as key comparer
    /// </summary>
    public static ObjectPool<Dictionary<string, T>> StringIgnoreCaseDictionary<T>()
    {
        return StringIgnoreCaseDictionaryNormalPool<T>.Instance;
    }

    /// <summary>
    /// pool that uses string as element with StringComparer.OrdinalIgnoreCase as element comparer
    /// </summary>
    public static readonly ObjectPool<HashSet<string>> StringIgnoreCaseHashSet =
        new ObjectPool<HashSet<string>>(() => new HashSet<string>(StringComparer.OrdinalIgnoreCase), 20);

    /// <summary>
    /// pool that uses string as element with StringComparer.Ordinal as element comparer
    /// </summary>
    public static readonly ObjectPool<HashSet<string>> StringHashSet =
        new ObjectPool<HashSet<string>>(() => new HashSet<string>(StringComparer.Ordinal), 20);

    /// <summary>
    /// Used to reduce the # of temporary byte[]s created to satisfy serialization and
    /// other I/O requests
    /// </summary>
    public static readonly ObjectPool<byte[]> ByteArray = new ObjectPool<byte[]>(() => new byte[ByteBufferSize], ByteBufferCount);

    /// pooled memory : 4K * 512 = 4MB
    public const int ByteBufferSize = 4 * 1024;
    private const int ByteBufferCount = 512;

    public static readonly ObjectPool<byte[]> AsyncByteArray = new ObjectPool<byte[]>(() => new byte[StreamExtensions.AsyncBufferSize], 50);

    private static class DefaultBigPool<T> where T : class, new()
    {
        public static readonly ObjectPool<T> Instance = new ObjectPool<T>(() => new T(), 100);
    }

    private static class DefaultNormalPool<T> where T : class, new()
    {
        public static readonly ObjectPool<T> Instance = new ObjectPool<T>(() => new T(), 20);
    }

    private static class StringIgnoreCaseDictionaryNormalPool<T>
    {
        public static readonly ObjectPool<Dictionary<string, T>> Instance =
            new ObjectPool<Dictionary<string, T>>(() => new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase), 20);
    }
}
