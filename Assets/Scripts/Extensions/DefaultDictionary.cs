using System.Collections.Generic;

public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
{
    public new TValue this[TKey key]
    {
        get
        {
            if (!TryGetValue(key, out TValue val)) Add(key, val = new TValue());
            return val;
        }
        set => base[key] = value;
    }
}