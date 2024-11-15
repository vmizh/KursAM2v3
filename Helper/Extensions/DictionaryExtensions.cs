using System.Collections.Generic;

namespace Helper.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey,TItem>(this Dictionary<TKey,TItem> dict, TKey key, TItem item)
        {
            if(dict.ContainsKey(key))
                dict[key] = item;
            else dict.Add(key, item);
        }

    }
}
