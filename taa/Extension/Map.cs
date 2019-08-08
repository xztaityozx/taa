using System;
using System.Collections.Generic;
using System.Text;

namespace taa {
    /// <summary>
    /// Dictionary<TKey,TValue>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class Map<TKey,TValue> :Dictionary<TKey,TValue> {

        private readonly TValue defaultValue;

        public Map(TValue d) :base() {
            defaultValue = d;
        }

        public Map() : this(default) {}

        public new TValue this[TKey key] {
            get => TryGetValue(key, out var v) ? v : defaultValue;
            set => base[key] = value;
        }
    }

    public static class MapExtension {
        public static Map<TKey, TValue> ToMap<T, TKey, TValue>(this IEnumerable<T> @this, Func<T, TKey> keySelector,
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
