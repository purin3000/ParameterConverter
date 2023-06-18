using System;
using ParameterConverter.Editor.Internal;

namespace ParameterConverter.Editor {
  public class CellDataConverterException : Exception {
    public CellDataConverterException(string message, CellParseInfo cellParseInfo)
      : base(message + cellParseInfo.Message) { }
  }

  public class IdValidatorException : CellDataConverterException {
    public IdValidatorException(Type t, int arrayIndex, CellParseInfo cellParseInfo)
      : base($"idの値が重複しています Class:{t.Name} Index:{arrayIndex}", cellParseInfo) { }
  }

  public class EnumNotFoundException : CellDataConverterException {
    public EnumNotFoundException(string enumName, string label, CellParseInfo cellParseInfo)
      : base($"enum定義が見つかりません Enum:{enumName} 文字列:{label}", cellParseInfo) { }
  }

  public class ValueTypeException : CellDataConverterException {
    public ValueTypeException(TypeCode typeCode, object val, CellParseInfo cellParseInfo)
      : base($"値変換に失敗 変数の型:{typeCode} 文字列:{val}", cellParseInfo) { }
  }

  public class ArrayIndexException : CellDataConverterException {
    public ArrayIndexException(string str, CellParseInfo cellParseInfo)
      : base($"値の代入に失敗。配列の添え字を取得できませんでした index:{str}", cellParseInfo) { }
  }

  public class FieldNameException : CellDataConverterException {
    public FieldNameException(string fieldName, CellParseInfo cellParseInfo)
      : base($"値の代入に失敗。フィールドを取得できませんでした fieldName:{fieldName}", cellParseInfo) { }
  }

  public class TargetObjectNotFoundException : CellDataConverterException {
    public TargetObjectNotFoundException(CellParseInfo cellParseInfo)
      : base("targetObjectが存在しません。.class設定を確認してください", cellParseInfo) { }
  }

  public class ClassTypeNotFoundException : CellDataConverterException {
    public ClassTypeNotFoundException(string label, CellParseInfo cellParseInfo)
      : base($"クラスが見つかりません label:{label}", cellParseInfo) { }
  }

  public class DuplicateDefinitionsException : CellDataConverterException {
    public DuplicateDefinitionsException(CellParseInfo cellParseInfo)
      : base("クラス定義が重複しています", cellParseInfo) { }
  }

  public class ObjectNotFoundException : CellDataConverterException {
    public ObjectNotFoundException(CellParseInfo cellParseInfo)
      : base("オブジェクトが存在しません", cellParseInfo) { }
  }

  public class CustomFunctionNotFoundException : CellDataConverterException {
    public CustomFunctionNotFoundException(string attributeName, CellParseInfo cellParseInfo)
      : base($"関数が見つかりません attributeName:{attributeName}", cellParseInfo) { }
  }

  public class InvokeException : CellDataConverterException {
    public InvokeException(string attributeName, CellParseInfo cellParseInfo)
      : base($"関数呼び出しに失敗 attributeName:{attributeName}", cellParseInfo) { }
  }

  public class UnknownException : CellDataConverterException {
    public UnknownException(string str, string output, CellParseInfo cellParseInfo)
      : base($"未知のエラーです str:{str} output:{output}", cellParseInfo) { }
  }
}