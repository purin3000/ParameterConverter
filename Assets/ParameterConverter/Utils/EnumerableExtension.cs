using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ParameterConverter.Editor.Utils {
  public static class EnumerableExtension {
    public static IEnumerable<(int, T)> Indexed<T>(this IEnumerable list) {
      var ret = new List<(int, T)>();
      var count = 0;
      foreach (var obj in list) ret.Add((count++, (T)obj));
      return ret;
    }

    public static IEnumerable<(int, ValueType)> Indexed<ValueType>(this IEnumerable<ValueType> list) {
      var count = 0;
      return list.Select(val => (count++, val));
    }
  }
}