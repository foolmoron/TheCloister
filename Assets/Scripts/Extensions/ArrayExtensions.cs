using System;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ArrayExtensions {

    public static T Random<T>(this T[] array) {
        return array[Mathf.FloorToInt(UnityEngine.Random.value * array.Length)];
    }

    public static List<T> WithAdd<T>(this List<T> list, T item) {
        list.Add(item);
        return list;
    }

    public static List<T> WithAddRange<T>(this List<T> list, IEnumerable<T> items) {
        list.AddRange(items);
        return list;
    }

    public static int Count<T>(this T[] array, Func<T, bool> countPredicate) {
        var count = 0;
        for (int i = 0; i < array.Length; i++) {
            if (countPredicate(array[i]))
                count++;
        }
        return count;
    }

    public static void ForEach<T>(this T[] array, Action<T> action) {
        for (int i = 0; i < array.Length; i++) {
            action(array[i]);
        }
    }

    public static int IndexOf<T>(this T[] array, T item) {
        for (int i = 0; i < array.Length; i++) {
            if (array[i].Equals(item))
                return i;
        }
        return -1;
    }

    public static bool Contains<T>(this T[] array, T item) {
        for (int i = 0; i < array.Length; i++) {
            if (array[i].Equals(item))
                return true;
        }
        return false;
    }

    public static bool Contains<TArray, TItem>(this TArray[] array, TItem item, Func<TArray, TItem, bool> comparator) {
        for (int i = 0; i < array.Length; i++) {
            if (comparator(array[i], item))
                return true;
        }
        return false;
    }

    public static bool ContainsIgnoreCase(this string[] array, string item) {
        for (int i = 0; i < array.Length; i++) {
            if (String.Compare(array[i], item, StringComparison.OrdinalIgnoreCase) == 0)
                return true;
        }
        return false;
    }

    public static T[] Shuffle<T>(this T[] array) {
        // TODO: fisher yates
        return array;
    }
}
