﻿using System;
using System.Reflection;

namespace ServiceStack.Text.Common;

public class DeserializeTypeUtils
{
    public static ParseStringDelegate GetParseMethod(Type type) => v => GetParseStringSpanMethod(type)(v.AsSpan());

    public static ParseStringSpanDelegate GetParseStringSpanMethod(Type type)
    {
        var typeConstructor = GetTypeStringConstructor(type);
        if (typeConstructor != null)
        {
            return value => value.IsEmpty
                ? null
                : typeConstructor.Invoke(new object[] { value.ToString() });
        }

        return null;
    }

    /// <summary>
    /// Get the type(string) constructor if exists
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static ConstructorInfo GetTypeStringConstructor(Type type)
    {
        foreach (var ci in type.GetConstructors())
        {
            var paramInfos = ci.GetParameters();
            var matchFound = paramInfos.Length == 1 && paramInfos[0].ParameterType == typeof(string);
            if (matchFound)
            {
                return ci;
            }
        }
        return null;
    }

}
