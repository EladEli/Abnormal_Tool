using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abnormal_UI
{

    public static class EnumExtension
    {
        public static bool IsDefined<TEnum>(TEnum value)
        {
            return Enum.IsDefined(typeof(TEnum), value);
        }

        public static IEnumerable<object> GetFlagNames(this Enum enumFlags)
        {
            return enumFlags.ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static TEnum Parse<TEnum>(string value)
        {
            Contract.Requires(value != null);

            return (TEnum)Enum.Parse(typeof(TEnum), value);
        }
    }

    public static class EnumerableExtension
    {
        #region Methods

        public static bool None<TItem>(this IEnumerable<TItem> enumerable, Func<TItem, bool> predicate = null)
        {
            Contract.Requires(enumerable != null);

            return !(predicate == null ? enumerable.Any() : enumerable.Any(predicate));
        }

        public static IEnumerable<TItem> EmptyIfNull<TItem>(this IEnumerable<TItem> enumerable)
        {
            return enumerable ?? Enumerable.Empty<TItem>();
        }

        public static IEnumerable<TItem> NullIfEmpty<TItem>(this IEnumerable<TItem> enumerable)
        {
            return enumerable == null || enumerable.Any() ? enumerable : null;
        }

        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TItem, TKey, TValue>(this IEnumerable<TItem> enumerable, Func<TItem, Task<TKey>> keySelector, Func<TItem, Task<TValue>> valueSelector)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(keySelector != null);
            Contract.Requires(valueSelector != null);

            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var item in enumerable)
            {
                dictionary[await keySelector(item)] = await valueSelector(item);
            }

            return dictionary;
        }

        public static HashSet<TItem> ToHashSet<TItem>(this IEnumerable<TItem> enumerable)
        {
            Contract.Requires(enumerable != null);

            return new HashSet<TItem>(enumerable);
        }

        public static string ToJoinString(this IEnumerable enumerable, string separator = null)
        {
            Contract.Requires(enumerable != null);

            return string.Join(separator, enumerable.Cast<object>());
        }

        public static IEnumerable<int> Repeat(this int count)
        {
            return Enumerable.Repeat(0, count);
        }

        public static void ForEach<TItem>(this IEnumerable<TItem> enumerable, Action<TItem> action)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(action != null);

            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        public static async Task ForEachAsync<TItem>(this IEnumerable<TItem> enumerable, Func<TItem, Task> action)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(action != null);

            foreach (var item in enumerable)
            {
                await action(item);
            }
        }

        public static bool EnumerableEquals<TItem>(this IEnumerable<TItem> enumerable, IEnumerable<TItem> other)
        {
            if (enumerable == null && other == null)
            {
                return true;
            }

            if (enumerable == null ^ other == null)
            {
                return false;
            }

            return enumerable.SequenceEqual(other);
        }

        public static bool EnumerableUnorderedEquals<TItem>(this IEnumerable<TItem> enumerable, IEnumerable<TItem> other)
        {
            if (enumerable == null && other == null)
            {
                return true;
            }

            if (enumerable == null ^ other == null)
            {
                return false;
            }

            return new HashSet<TItem>(enumerable).SetEquals(other);
        }

        public static int GetEnumerableHashCode<TItem>(this IEnumerable<TItem> enumerable)
        {
            if (enumerable == null || enumerable.None())
            {
                return 0;
            }

            var hashCode = 17;
            enumerable.ForEach(_ => hashCode = hashCode * 31 + _.GetHashCode());
            return hashCode;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable, Random random)
        {
            var array = enumerable.ToArray();
            for (var itemIndex = array.Length - 1; itemIndex >= 0; itemIndex--)
            {
                var swapItemIndex = random.Next(itemIndex + 1);
                yield return array[swapItemIndex];
                array[swapItemIndex] = array[itemIndex];
            }
        }

        #endregion
    }

    public static class DictionaryExtensions
    {
        #region Methods

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            Contract.Requires(dictionary != null);

            TValue value;
            return !dictionary.TryGetValue(key, out value) ? default(TValue) : value;
        }

        public static Dictionary<TKey, TNewValue> Transform<TKey, TOldValue, TNewValue>(
            this IDictionary<TKey, TOldValue> dictionary,
            Func<TOldValue, TNewValue> valueTransformer)
        {
            Contract.Requires(dictionary != null);
            Contract.Requires(valueTransformer != null);

            return dictionary.ToDictionary(_ => _.Key, _ => valueTransformer(_.Value));
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            Contract.Requires(dictionary != null);

            return dictionary.GetOrCreate(key, () => new TValue());
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueCreator)
        {
            Contract.Requires(dictionary != null);
            Contract.Requires(valueCreator != null);

            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = valueCreator();
                dictionary[key] = value;
            }

            return value;
        }

        public static void UnionWith<TKey, TValue>(
            this IDictionary<TKey, TValue> destinationDictionary,
            IDictionary<TKey, TValue> sourceDictionary,
            Func<TKey, TValue, TValue, TValue> duplicateItemSelector = null,
            Func<TKey, TValue, TValue> singleItemSelector = null)
        {
            Contract.Requires(destinationDictionary != null);
            Contract.Requires(sourceDictionary != null);

            foreach (var sourceDictionaryItem in sourceDictionary)
            {
                TValue destinationDictionaryItemValue;
                destinationDictionary[sourceDictionaryItem.Key] = destinationDictionary.TryGetValue(sourceDictionaryItem.Key, out destinationDictionaryItemValue)
                    ? (duplicateItemSelector == null
                        ? sourceDictionaryItem.Value
                        : duplicateItemSelector(sourceDictionaryItem.Key, destinationDictionaryItemValue, sourceDictionaryItem.Value))
                    : (singleItemSelector == null
                        ? sourceDictionaryItem.Value
                        : singleItemSelector(sourceDictionaryItem.Key, sourceDictionaryItem.Value));
            }
        }

        #endregion
    }
}
