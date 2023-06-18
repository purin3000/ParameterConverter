using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ParameterConverter.Editor.Internal {
  public interface IValueValidator {
    public void Check(Type scriptableObjectType, FieldInfo fieldInfo,  object value, int arrayIndex, CellParseInfo cellParseInfo);
  }

  internal static class ReflectionUtil {
    private static readonly Regex REG_SPLIT = new(@"^([^.]+)\.(.+)");
    private static readonly Regex REG_ARRAY = new(@"^([^.\[\]]+)\[(\d+)\]");

    /// ★DLLの注意事項
    /// エディタ拡張から実行した場合、Assembly-CSharp-Editorしか読まれていない状態のため、
    /// インゲーム側のModuleを使用可能にするため、明示的にDLLを指定して読み込んでいます。
    /// AsemdefなどでDLLを分割した場合、ここにDLLを追加する必要があると思われます。
    private static readonly string[] DLLs = {
      "Library/ScriptAssemblies/Assembly-CSharp.dll",
      "Library/ScriptAssemblies/Assembly-CSharp-Editor.dll"
    };

    private static readonly Dictionary<(Type, string), (FieldInfo, int)> FIELD_INFO_CACHE = new();

    private static List<Module> modules;
    public static List<Module> Modules {
      get {
        if (modules == null) {
          modules = new List<Module>();
          foreach (var path in DLLs)
            if (File.Exists(path)) {
              var asm = Assembly.LoadFrom(path);
              modules.AddRange(asm.GetModules());
            } else {
              Debug.LogWarning($"DLL not found. path:{path}");
            }
        }

        return modules;
      }
    }

    /// <summary>
    /// DLLを検索して型を返す
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static Type SearchType(string typeName) {
      foreach (var module in Modules) {
        var type = module.GetType(typeName);
        if (type != null) {
          return type;
        }
      }

      return null;
    }

    /// <summary>
    /// AfterCallbackAttributeが付いたMethodを取得する
    /// </summary>
    /// <param name="label"></param>
    /// <returns></returns>
    public static MethodInfo GetCustomMethod(string label) {
      return AttributeAccessor.GetCustomMethod(label);
    }

    public static Type GetCustomClass(string label) {
      return AttributeAccessor.GetCustomClass(label);
    }

    /// <summary>
    /// 型からオブジェクト生成。ScriptableObjectにも対応しています
    /// </summary>
    public static object CreateObject(Type t) {
      Debug.Assert(t != null);

      if (t.IsSubclassOf(typeof(ScriptableObject))) {
        return ScriptableObject.CreateInstance(t);
      }
      return Activator.CreateInstance(t);
    }

    /// <summary>
    /// 指定したフィールドに値を代入
    /// filed.childFiled.valueのようにコンマで区切ることで、子供のメンバを対象にできます
    /// field.buffer[1]のように配列の要素を指定することも可能です
    /// 途中のメンバのインスタンスが存在しない場合は適宜生成されます
    /// </summary>
    public static void SetFieldValue(Type t, object obj, string name, object value,
      IValueValidator valueValidator, CellParseInfo cellParseInfo) {
      Debug.AssertFormat(obj != null, $"obj == null!! name:{name}");

      var m = REG_SPLIT.Match(name);
      if (m.Success) {
        // 最後の要素ではない
        var (fieldInfo, arrayIndex) = GetFieldInfo(t, m.Groups[1].Value, cellParseInfo);
        var nextFieldName = m.Groups[2].Value;
        if (arrayIndex == -1) {
          // 配列ではない
          var fieldObj = fieldInfo.GetValue(obj);
          if (fieldObj == null) {
            // フィールドのインスタンスを作る
            fieldObj = CreateObject(fieldInfo.FieldType);
            fieldInfo.SetValue(obj, fieldObj);
          }

          SetFieldValue(fieldInfo.FieldType, fieldObj, nextFieldName, value, valueValidator, cellParseInfo);

          if (fieldInfo.FieldType.IsValueType) // 値型の場合はBoxingの都合でコピーに対して設定されているので、コピーを設定しなおす必要がある
          {
            fieldInfo.SetValue(obj, fieldObj);
          }
        } else {
          // 配列である
          var arrayInstance = GetArrayInstance(obj, fieldInfo, arrayIndex);

          var elementType = fieldInfo.FieldType.GetElementType();
          Debug.Assert(elementType != null);
          
          var elementObj = arrayInstance.GetValue(arrayIndex);
          if (elementType.IsClass && elementObj == null) {
            // 要素のインスタンスを作る
            elementObj = CreateObject(elementType);
            arrayInstance.SetValue(elementObj, arrayIndex);
          }

          SetFieldValue(elementType, elementObj, nextFieldName, value, valueValidator, cellParseInfo);

          if (elementType.IsValueType) // 値型の場合はBoxingの都合でコピーに対して設定されているので、コピーを設定しなおす必要がある
          {
            arrayInstance.SetValue(elementObj, arrayIndex);
          }
        }
      } else {
        // 最後の要素である
        var (fieldInfo, arrayIndex) = GetFieldInfo(t, name, cellParseInfo);
        if (arrayIndex == -1) {
          // 配列ではない

          // フィールドに値を設定
          valueValidator.Check(t, fieldInfo, value, -1, cellParseInfo);
          var newValue = LexicalCast(fieldInfo.FieldType, value, cellParseInfo);
          fieldInfo.SetValue(obj, newValue);
        } else {
          // 配列である。string[]のような単純な配列の場合
          var arrayInstance = GetArrayInstance(obj, fieldInfo, arrayIndex);

          // 配列に値を設定
          valueValidator.Check(t, fieldInfo, value, arrayIndex, cellParseInfo);
          var newValue = LexicalCast(fieldInfo.FieldType.GetElementType(), value, cellParseInfo);
          arrayInstance.SetValue(newValue, arrayIndex);
        }
      }
    }

    /// <summary>
    /// ターゲットに合わせてキャスト
    /// </summary>
    private static object LexicalCast(Type targetType, object value, CellParseInfo cellParseInfo) {
      if (targetType.IsEnum) {
        //enumはInt32で来る
        var label = value.ToString();
        if (!string.IsNullOrEmpty(label)) {
          var index = Array.FindIndex(Enum.GetNames(targetType), __str => __str == label);
          if (index != -1) {
            return Enum.GetValues(targetType).GetValue(index);
          }
          throw new EnumNotFoundException(targetType.Name, label, cellParseInfo);
        }

        return 0;
      }

      var typeCode = Type.GetTypeCode(targetType);
      try {
        if (typeCode == TypeCode.Int32) {
          if (string.IsNullOrEmpty(value.ToString())) {
            return 0;
          }
          return Convert.ToInt32(value);
        }

        if (typeCode == TypeCode.Single) {
          if (string.IsNullOrEmpty(value.ToString())) {
            return 0;
          }
          return Convert.ToSingle(value);
        }

        if (typeCode == TypeCode.Int16) {
          if (string.IsNullOrEmpty(value.ToString())) {
            return 0;
          }
          return Convert.ToInt16(value);
        }

        if (typeCode == TypeCode.SByte) {
          if (string.IsNullOrEmpty(value.ToString())) {
            return 0;
          }
          return Convert.ToSByte(value);
        }

        if (typeCode == TypeCode.UInt32) {
          if (string.IsNullOrEmpty(value.ToString())) {
            return 0;
          }
          return Convert.ToUInt32(value);
        }

        if (typeCode == TypeCode.UInt16) {
          if (string.IsNullOrEmpty(value.ToString())) {
            return 0;
          }
          return Convert.ToUInt16(value);
        }

        if (typeCode == TypeCode.Byte) {
          if (string.IsNullOrEmpty(value.ToString())) {
            return 0;
          }
          return Convert.ToByte(value);
        }

        if (typeCode == TypeCode.Double) {
          if (string.IsNullOrEmpty(value.ToString())) {
            return 0;
          }
          return Convert.ToDouble(value);
        }

        if (typeCode == TypeCode.Boolean) {
          var label = value.ToString().ToLower();
          if (string.IsNullOrEmpty(label)) {
            return false;
          }
          if (label == "true") {
            return true;
          }
          if (label == "false") {
            return false;
          }
          if (label == "0") {
            return false;
          }
          return true;
        }
      } catch {
        throw new ValueTypeException(typeCode, value, cellParseInfo);
      }

      return string.IsNullOrEmpty(value.ToString()) ? "" : Convert.ToString(value);
    }

    /// <summary>
    /// フィールド要素の取得
    /// fieldNameで指定されるフィールドが配列の場合、配列の添え字も取得します
    /// </summary>
    /// <returns></returns>
    private static (FieldInfo, int arrayIndex) GetFieldInfo(Type t, string name,
      CellParseInfo cellParseInfo) {
      var fieldName = name;
      var arrayIndex = -1;

      if (FIELD_INFO_CACHE.TryGetValue((t, name), out var cache)) {
        return cache;
      }
      
      var ma = REG_ARRAY.Match(name);
      if (ma.Success) {
        // 配列時フィールド名と添え字を取得
        fieldName = ma.Groups[1].Value;
        try {
          arrayIndex = Convert.ToInt32(ma.Groups[2].Value);
        } catch {
          throw new ArrayIndexException(ma.Groups[2].Value, cellParseInfo);
        }
      }

      var fieldInfo = t.GetField(fieldName,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty);
      if (fieldInfo == null) {
        throw new FieldNameException(fieldName, cellParseInfo);
      }

      FIELD_INFO_CACHE.Add((t, name), (fieldInfo, arrayIndex));
      
      return (fieldInfo, arrayIndex);
    }

    /// <summary>
    /// 配列のインスタンスを取得
    /// 必要に応じて配列がなければ作るし、長さが足りなければ配列拡張して内容をコピーもします
    /// </summary>
    private static Array GetArrayInstance(object obj, FieldInfo fieldInfo, int arrayIndex) {
      var elementType = fieldInfo.FieldType.GetElementType();

      var arrayInstance = fieldInfo.GetValue(obj) as Array;
      if (arrayInstance == null) {
        // 配列初期化
        arrayInstance = Array.CreateInstance(elementType, arrayIndex + 1);
        fieldInfo.SetValue(obj, arrayInstance);
      }


      if (arrayInstance.Length <= arrayIndex) {
        // 配列拡張
        var src = arrayInstance;
        var dst = Array.CreateInstance(elementType, arrayIndex + 1);

        src.CopyTo(dst, 0);

        arrayInstance = dst;
        fieldInfo.SetValue(obj, arrayInstance);
      }

      return arrayInstance;
    }
  }
}