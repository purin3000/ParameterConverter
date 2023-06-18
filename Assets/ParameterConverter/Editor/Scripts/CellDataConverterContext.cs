using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ParameterConverter.Editor.Internal;
using ParameterConverter.Utils;
using UnityEngine;

namespace ParameterConverter.Editor {
  /// <summary>
  /// CellDataConvertに渡される管理情報
  /// </summary>
  public class CellDataConverterContext : IValueValidator {
    private readonly List<FunctionData> callbackList = new();

    private readonly Dictionary<string, ObjInfo> objInfoTable = new();

    private readonly IReadOnlyList<IValueChecker> validatorList;
    private readonly IReadOnlyList<IReplacePathStr> replaceStrList;

    public CellDataConverterContext(IReadOnlyList<IValueChecker> validatorList, IReadOnlyList<IReplacePathStr> replaceStrList) {
      this.validatorList = validatorList;
      this.replaceStrList = replaceStrList;
    }

    public string ReplaceStr(string str) {
      foreach (var replaceStr in replaceStrList) {
        str = replaceStr.Convert(str);
      }
      return str;
    }

    public void Check(Type scriptableObjectType, FieldInfo fieldInfo, object value, int arrayIndex,
      CellParseInfo cellParseInfo) {
      if (validatorList == null) {
        return;
      }
      foreach (var hook in validatorList) {
        hook.Check(scriptableObjectType, fieldInfo, value, arrayIndex, cellParseInfo);
      }
    }
    
    public void SaveAssets() {
      foreach (var (output, objInfo) in objInfoTable.Select(pair => (pair.Key, pair.Value))) {
        FileIO.CreateOrReplaceAsset(output, output.GetFileNameWithoutExtension(), objInfo.targetType,
          objInfo.targetObject);
      }

      foreach (var func in callbackList) {
        func.CallFunc();
      }

      objInfoTable.Clear();
    }

    public ScriptableObject CreateObject(string output, Type targetType) {
      if (objInfoTable.TryGetValue(output, out var arrayIndex)) {
        return arrayIndex.targetObject;
      }

      var targetObject = ReflectionUtil.CreateObject(targetType) as ScriptableObject;
      if (targetObject == null) {
        return null;
      }

      objInfoTable.Add(output, new ObjInfo(targetType, targetObject));
      return targetObject;
    }

    public int GetArrayOffset(string output) =>
      objInfoTable.TryGetValue(output, out var objInfo) ? objInfo.arrayIndex : 0;

    public void SetArrayOffset(string output, int arrayIndex) {
      if (objInfoTable.TryGetValue(output, out var objInfo)) {
        objInfo.arrayIndex = arrayIndex;
      }
    }

    public void AddFunctionData(string attributeName, string arg, string assetPath, CellParseInfo cellParseInfo) {
      callbackList.Add(new FunctionData(attributeName, arg, assetPath, cellParseInfo));
    }

    private class ObjInfo {
      public int arrayIndex;
      public readonly ScriptableObject targetObject;

      public readonly Type targetType;

      public ObjInfo(Type targetType, ScriptableObject targetObject) {
        this.targetType = targetType;
        this.targetObject = targetObject;
      }
    }

    private class FunctionData {
      private readonly string arg;
      private readonly string assetPath;
      private readonly string attributeName;
      private readonly CellParseInfo cellParseInfo;

      public FunctionData(string attributeName, string arg, string assetPath, CellParseInfo cellParseInfo) {
        this.attributeName = attributeName;
        this.assetPath = assetPath;
        this.arg = arg;
        this.cellParseInfo = cellParseInfo;
      }

      public void CallFunc() {
        var methodInfo = ReflectionUtil.GetCustomMethod(attributeName);
        if (methodInfo == null) {
          throw new CustomFunctionNotFoundException(attributeName, cellParseInfo);
        }

        try {
          methodInfo.Invoke(null, new object[] { assetPath, arg });
        } catch {
          throw new InvokeException(attributeName, cellParseInfo);
        }
      }
    }
  }
}