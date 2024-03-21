using System.Collections.Generic;

public static class IEnumerableExt
{
    public static void ForEach<T>(this IEnumerable<T> items, System.Action<T, int> handler)
    {
        int i = -1;
        foreach (T item in items) handler(item, ++i);
    }
}