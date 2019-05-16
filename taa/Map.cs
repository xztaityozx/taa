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
}
