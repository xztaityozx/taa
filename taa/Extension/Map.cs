using System;
using System.Collections.Generic;

namespace taa.Extension {
    /// <summary>
    /// Dictionary<TKey,TValue>のラッパー
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class Map<TKey,TValue> :Dictionary<TKey,TValue> {

        private readonly TValue defaultValue;

        public Map(TValue d) :base() {
            defaultValue = d;
        }

        public Map() : this(default) {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new TValue this[TKey key] {
            get => TryGetValue(key, out var v) ? v : defaultValue;
            set => base[key] = value;
        }
    }

    public static class MapExtension {
        /// <summary>
        /// コレクションからMapを作成します
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="keySelector"></param>
        /// <param name="valueSelector"></param>
        /// <returns></returns>
        public static Map<TKey, TValue> ToMap<T, TKey, TValue>(this IEnumerable<T> @this, 
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector)
        {
            var rt = new Map<TKey, TValue>();

            foreach (var x in @this) {
                rt[keySelector(x)] = valueSelector(x);
            }

            return rt;
        }
    }
}
