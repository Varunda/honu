using System;
using System.Collections.Generic;

namespace watchtower.Code.ExtensionMethods {

    public static class EnumerableExtensionMethods {

        public static Dictionary<TKey, TSource> ToDictionaryDistinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : notnull {
            Dictionary<TKey, TSource> dict = new();

            foreach (TSource elem in source) {
                TKey key = keySelector(elem);
                if (dict.ContainsKey(key) == false) {
                    dict.Add(key, elem);
                }
            }

            return dict;
        }

    }
}
