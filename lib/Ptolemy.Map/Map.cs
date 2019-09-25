using System;
using System.Collections.Generic;
using System.Linq;

namespace Ptolemy.Map
{
    public class Map<TKey, TValue> : Dictionary<TKey, TValue> {
        private readonly Func<TValue> defaultFunc;

        public Map() : base() {
            defaultFunc = () => default;
        }

        public Map(TValue d) : base() {
            defaultFunc = () => d;
        }

        public Map(Func<TValue> func) : base() {
            defaultFunc = func;
        }

        public static Map<TKey, TValue> Merge(IEnumerable<Map<TKey, TValue>> maps) {
            var rt = new Map<TKey, TValue>();

            foreach (var (key, value) in maps.SelectMany(k => k)) {
                rt[key] = value;
            }
            
            return rt;
        }

        public new TValue this[TKey key] {
            get => TryGetValue(key, out var v) ? v : defaultFunc();
            set => base[key] = value;
        }
    }

    public static class MapExtension {
        /// <summary>
        /// Convert to Map
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="keySelector"></param>
        /// <param name="valueSelector"></param>
        /// <returns></returns>
        public static Map<TKey, TValue> ToMap<T, TKey, TValue>(this IEnumerable<T> @this, Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector) {
            var rt = new Map<TKey, TValue>();

            foreach (var t in @this) {
                rt[keySelector(t)] = valueSelector(t);
            }

            return rt;
        }
    }

}
