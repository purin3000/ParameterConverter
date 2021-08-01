using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ParameterConverter
{
    public static class EnumerableExtension
    {
        public static IEnumerable<(int, T)> Indexed<T>(this IEnumerable list)
        {
            List<(int, T)> ret = new List<(int, T)>();
            int count = 0;
            foreach (var obj in list) {
                ret.Add((count++, (T)obj));
            }
            return ret;
        }

        public static IEnumerable<(int, ValueType)> Indexed<ValueType>(this IEnumerable<ValueType> list)
        {
            int count = 0;
            return list.Select(val => (count++, val));
        }
    }
}

