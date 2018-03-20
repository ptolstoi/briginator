using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Linq.Expressions;

public static class Extensions
{

    public static Vector3 Average<TSource>(this IEnumerable<TSource> source, Func<TSource, Vector3> selector)
    {
        var result = Vector3.zero;
        var count = 0;

        foreach (var v in source)
        {
            count++;
            result += selector(v);
        }

        if (count == 0)
        {
            return Vector3.zero;
        }

        return result / count;
    }
}