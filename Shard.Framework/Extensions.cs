using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shard
{
    static class Extensions
    {
        public static TValue TryGetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TValue : class
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
                return value;

            return null;
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> activator)
           where TValue : class
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
                return value;

            return dict[key] = activator();
        }
    }
}
