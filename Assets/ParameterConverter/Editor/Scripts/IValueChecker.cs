using System;
using System.Reflection;
using ParameterConverter.Editor.Internal;

namespace ParameterConverter.Editor {
  public interface IValueChecker {
    void Check(Type scriptableObjectType, FieldInfo fieldInfo, object val, int arrayIndex,
      CellParseInfo cellParseInfo);
  }
}