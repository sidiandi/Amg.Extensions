using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Amg.Collections;

public static class DictionaryExtensions
{
    public static V GetOr<K, V>(this IDictionary<K, V> dictionary, K key, Func<V> creator)
    {
        if (dictionary.TryGetValue(key, out var v))
        {
            return v;
        }
        else
        {
            var newValue = creator();
            dictionary[key] = newValue;
            return newValue;
        }
    }

    /// <summary>
    /// "Caching" function: get an element of a dictionary or creates it and adds it if it not yet exists.
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static Value GetOrAdd<Key, Value>(this IDictionary<Key, Value> dictionary, Key key, Func<Value> factory)
    {
        lock (dictionary)
        {
            if (dictionary.TryGetValue(key, out var existingValue))
            {
                return existingValue;
            }
        }
                
        var value = factory();

        lock (dictionary)
        {
            dictionary[key] = value;
        }
        return value;
    }

    /// <summary>
    /// "Caching" function: get an element of a dictionary or creates it and adds it if it not yet exists.
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static async Task<Value> GetOrAddAsync<Key, Value>(this IDictionary<Key, Value> dictionary, Key key, Func<Task<Value>> factory)
    {
        lock (dictionary)
        {
            if (dictionary.TryGetValue(key, out var existingValue))
            {
                return existingValue;
            }
        }

        var value = await factory();

        lock (dictionary)
        {
            dictionary[key] = value;
        }
        return value;
    }

    /// <summary>
    /// Merge two dictionaries.
    /// </summary>
    ///  Keys of b which are already present in a will overwrite the entry in a
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static IDictionary<Key, Value> Merge<Key, Value>(this IDictionary<Key, Value> a, IDictionary<Key, Value> b) where Key : notnull
    {
        var r = new Dictionary<Key, Value>();
        foreach (var i in a.Concat(b))
        {
            r[i.Key] = i.Value;
        }
        return r;
    }

    /// <summary>
    /// Add entries of newEntries to dictionaryToGrow
    /// </summary>
    /// <param name="dictionaryToGrow"></param>
    /// <param name="newEntries"></param>
    public static void Add(this StringDictionary dictionaryToGrow, IDictionary<string, string> newEntries)
    {
        foreach (var i in newEntries)
        {
            dictionaryToGrow[i.Key] = i.Value;
        }
    }


}
