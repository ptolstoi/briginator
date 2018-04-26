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

    public static V GetOrDefault<K, V>(this Dictionary<K, V> self, K key, V defaultValue = default(V))
    {
        return self.ContainsKey(key) ? self[key] : defaultValue;
    }

    public static V GetOrDefault<K, V>(this Dictionary<K, V> self, K key, Func<V> lazyDefaultValue)
    {
        return self.ContainsKey(key) ? self[key] : lazyDefaultValue();
    }

    public static T EnsureComponent<T>(this GameObject self) where T : Component
    {
        var comp = self.GetComponent<T>();
        if (comp == null)
        {
            comp = self.AddComponent<T>();
        }
        return comp;
    }

    public static Vector3 NoiseVector(this Vector3 v)
    {
        v.y += (Mathf.PerlinNoise(v.x * 1.5f, v.z) - 0.5f);
        return v;
    }

    public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);
    public static Vector3 WithZ(this Vector2 v, float z) => new Vector3(v.x, v.y, z);

    public static Color PopOrDefault(this Stack<Color> self)
    {
        if (self.Count != 0)
        {
            return self.Pop();
        }

        return UnityEngine.Random.ColorHSV();
    }
}