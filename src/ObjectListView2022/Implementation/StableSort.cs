using System;
using System.Collections;
using System.Collections.Generic;

namespace BrightIdeasSoftware.Implementation
{
    /// <summary>
    /// Provides stable in-place sorting for the collection types used by ObjectListView.
    /// </summary>
    internal static class StableSort
    {
        internal static void Sort<T>(List<T> items, IComparer<T> comparer)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }
            if (items.Count < 2)
            {
                return;
            }

            var indexedItems = new List<IndexedValue<T>>(items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                indexedItems.Add(new IndexedValue<T>(items[i], i));
            }

            indexedItems.Sort((x, y) =>
            {
                var result = comparer.Compare(x.Value, y.Value);
                return result != 0 ? result : x.Index.CompareTo(y.Index);
            });

            for (var i = 0; i < indexedItems.Count; i++)
            {
                items[i] = indexedItems[i].Value;
            }
        }

        internal static void Sort(ArrayList items, IComparer comparer)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }
            if (items.Count < 2)
            {
                return;
            }

            var indexedItems = new List<IndexedValue<object>>(items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                indexedItems.Add(new IndexedValue<object>(items[i], i));
            }

            indexedItems.Sort((x, y) =>
            {
                var result = comparer.Compare(x.Value, y.Value);
                return result != 0 ? result : x.Index.CompareTo(y.Index);
            });

            for (var i = 0; i < indexedItems.Count; i++)
            {
                items[i] = indexedItems[i].Value;
            }
        }

        private readonly struct IndexedValue<T>
        {
            internal IndexedValue(T value, int index)
            {
                Value = value;
                Index = index;
            }

            internal T Value { get; }
            internal int Index { get; }
        }
    }
}
