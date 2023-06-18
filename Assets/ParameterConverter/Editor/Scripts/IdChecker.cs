using System;
using System.Collections.Generic;
using System.Reflection;
using ParameterConverter.Editor.Internal;

namespace ParameterConverter.Editor {
  /// <summary>
  /// .idに格納する値が一意ではない場合はエラーにする
  /// </summary>
  public class IdChecker : IValueChecker {
    private readonly Dictionary<Type, HashSet<string>> history = new();

    public void Check(Type scriptableObjectType, FieldInfo fieldInfo, object val, int arrayIndex,
      CellParseInfo cellParseInfo) {
      if (fieldInfo.Name != "id" || 0 > arrayIndex) {
        return;
      }
      var str = val.ToString();
      if (!history.TryGetValue(scriptableObjectType, out var list)) {
        list = new HashSet<string>();
        history.Add(scriptableObjectType, list);
      }
      if (list.Contains(str)) {
        throw new IdValidatorException(scriptableObjectType, arrayIndex, cellParseInfo);
      }
      list.Add(str);
    }
  }
}