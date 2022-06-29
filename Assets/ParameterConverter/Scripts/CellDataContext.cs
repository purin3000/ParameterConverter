using System;
using System.Collections.Generic;
using System.Linq;
using ParameterConverter.Scripts;
using UnityEngine;

namespace ParameterConverter.Editor {
  public class CellDataContext {
    private readonly List<FunctionData> callbackList = new();

    private readonly Dictionary<string, ObjInfo> objInfoTable = new();

    public CellDataContext()
      : this(new[] { new ArrayValueValidator() }) { }

    public CellDataContext(IEnumerable<ILexicalCastHook> validators) {
      Validators = validators;
    }

    public IEnumerable<ILexicalCastHook> Validators { get; }

    public void SaveAssets() {
      foreach (var (output, objInfo) in objInfoTable.Select(pair => (pair.Key, pair.Value)))
        FileIO.CreateOrReplaceAsset(output, output.GetFileNameWithoutExtension(), objInfo.targetType,
          objInfo.targetObject);

      foreach (var func in callbackList) func.CallFunc();

      objInfoTable.Clear();
    }

    public ScriptableObject CreateObject(string output, Type targetType) {
      if (objInfoTable.TryGetValue(output, out var arrayIndex)) return arrayIndex.targetObject;

      var targetObject = ReflectionUtil.CreateObject(targetType) as ScriptableObject;
      if (targetObject == null) return null;

      objInfoTable.Add(output, new ObjInfo(targetType, targetObject));
      return targetObject;
    }

    public int GetArrayOffset(string output) {
      if (objInfoTable.TryGetValue(output, out var objInfo)) return objInfo.arrayIndex;
      return 0;
    }

    public void SetArrayOffset(string output, int arrayIndex) {
      if (objInfoTable.TryGetValue(output, out var objInfo)) objInfo.arrayIndex = arrayIndex;
    }

    public void AddFunctionData(string attributeName, string arg, string assetPath, IErrorInfo errorInfo) {
      callbackList.Add(new FunctionData(attributeName, arg, assetPath, errorInfo));
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
      private readonly IErrorInfo errorInfo;

      public FunctionData(string attributeName, string arg, string assetPath, IErrorInfo errorInfo) {
        this.attributeName = attributeName;
        this.assetPath = assetPath;
        this.arg = arg;
        this.errorInfo = errorInfo;
      }

      public void CallFunc() {
        var methodInfo = ReflectionUtil.GetCustomMethod(attributeName);
        if (methodInfo == null) throw new CustomFunctionNotFoundException(attributeName, errorInfo);

        try {
          methodInfo.Invoke(null, new object[] { assetPath, arg, errorInfo });
        } catch {
          throw new InvokeException(attributeName, errorInfo);
        }
      }
    }
  }
}