using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class ListExtensions
{
    public static bool ContainsAllItems<T>(this IEnumerable<T> a, IEnumerable<T> b)
    {
        return !b.Except(a).Any();
    }
}
