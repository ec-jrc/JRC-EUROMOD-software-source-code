using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public static class ExtensionMethods_Dictionary
    {
        /// <summary> add to Dictionary with making sure that the key does not yet exist </summary>
        /// <returns> false: if key already contained in Dictionary, true if successfully added </returns>
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.ContainsKey(key)) return false;
            dic.Add(key, value);
            return true;
        }

        /// <summary> AddRange for a Dictionary </summary>
        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dic, Dictionary<TKey, TValue> dicToAdd,
                                                  bool ignoreDoubleKey = false)
        {
            foreach (var x in dicToAdd)
            {
                if (ignoreDoubleKey && dic.ContainsKey(x.Key)) continue;
                dic.Add(x.Key, x.Value);
            }
        }

        /// <summary> get the whole entry of a dictionary by key (not just the value) - useful if for example comparer ignores case </summary>
        public static KeyValuePair<TKey, TValue> GetEntry<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.FirstOrDefault(p => dictionary.Comparer.Equals(p.Key, key));
        }

        /// <summary> get the key of a dictionary by key - useful if for example comparer ignores case </summary>
        public static TKey GetOriginalKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            KeyValuePair<TKey, TValue> entry = dictionary.GetEntry<TKey, TValue>(key);
            return entry.Equals(default(KeyValuePair<TKey, TValue>)) ? default(TKey) : entry.Key;
        }

        public static bool CaseInsensitiveContainsKey<T>(this Dictionary<string, T> dictionary, string key)
        {
            var e = from d in dictionary where d.Key.ToLower() == key.ToLower() select d;
            return (e != null && e.Count() > 0);
        }

        public static KeyValuePair<string, T> CaseInsensitiveGet<T>(this Dictionary<string, T> dictionary, string key)
        {
            return (from d in dictionary where d.Key.ToLower() == key.ToLower() select d).FirstOrDefault();
        }

        public static bool RenameKey<T>(this Dictionary<string, T> dictionary, string oldKey, string newKey)
        {
            if (!dictionary.ContainsKey(oldKey)) return false; // note: does not take care about position (removes from pos x and adds at end)
            T value = dictionary[oldKey]; dictionary.Remove(oldKey); dictionary.Add(newKey, value);
            return true;
        }

        public static bool ReplaceValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue newValue)
        {
            if (!dictionary.ContainsKey(key)) return false; // note: does not take care about position (removes from pos x and adds at end)
            dictionary.AddOrReplace<TKey, TValue>(key, newValue);
            return true;
        }

        public static void AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.Remove(key); dictionary.Add(key, value);
        }

        public static string GetOrEmpty(this Dictionary<string, string> dictionary, string key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : string.Empty;
        }
    }
}
