using System;
using System.Collections.Generic;

namespace Core.Utils.Extensions {
    public static class ArrayExtension {
        public static T[] RemoveAt<T>(this T[] array, int index) {
            if (index < 0 || index >= array.Length) return array;

            T[] result = new T[array.Length - 1];
            for (int i = 0, j = 0; i < array.Length; i++) {
                if (i == index) continue;

                result[j] = array[i];
                j++;
            }

            return result;
        }

        public static int IndexOf<T>(this T[] array, T t) {
            for (int i = 0; i < array.Length; i++) {
                if (array[i].Equals(t)) {
                    return i;
                }
            }

            return -1;
        }

        public static bool Contains<T>(this T[] array, T t) {
            return IndexOf(array, t) > -1;
        }

        public static T[] Filter<T>(this T[] array, Func<T, bool> filter) {
            List<T> l = new List<T>();
            for (int kIndex = 0; kIndex < array.Length; kIndex++) {
                T t = array[kIndex];
                if (filter(t)) {
                    l.Add(t);
                }
            }
            T[] r = new T[l.Count];
            for (int kIndex = 0; kIndex < l.Count; kIndex++) {
                r[kIndex] = l[kIndex];
            }

            return r;
        }

        public static float Sum(this float[] array) {
            float sum = 0;
            for (int kIndex = 0; kIndex < array.Length; kIndex++) {
                sum += array[kIndex];
            }

            return sum;
        }
    }
}