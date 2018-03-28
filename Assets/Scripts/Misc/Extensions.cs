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

    public static Transform[] GetChildrenArray(this Transform source)
    {
        var result = new Transform[source.childCount];
        var i = 0;
        var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext())
        {
            result[i] = enumerator.Current as Transform;
            i++;
        }

        return result;
    }

    public static void SetLayerRecursive(this GameObject source, int layer)
    {
        source.layer = layer;

        foreach (Transform c in source.transform)
        {
            c.gameObject.SetLayerRecursive(layer);
        }
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        T tmp = a;
        a = b;
        b = tmp;
    }
}