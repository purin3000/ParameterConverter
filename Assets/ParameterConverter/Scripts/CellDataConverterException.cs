using System;

namespace ParameterConverter.Scripts {
  public class CellDataConverterException : Exception {
    public CellDataConverterException(string message, IErrorInfo errorInfo)
      : base(message + errorInfo.Message) { }
  }

  public class IdValidatorException : CellDataConverterException {
    public IdValidatorException(Type t, int arrayIndex, IErrorInfo errorInfo)
      : base($"idの値が重複しています Class:{t.Name} Index:{arrayIndex}", errorInfo) { }
  }

  public class EnumNotFoundException : CellDataConverterException {
    public EnumNotFoundException(string enumName, string label, IErrorInfo errorInfo)
      : base($"enum定義が見つかりません Enum:{enumName} 文字列:{label}", errorInfo) { }
  }

  public class ValueTypeException : CellDataConverterException {
    public ValueTypeException(TypeCode typeCode, object val, IErrorInfo errorInfo)
      : base($"値変換に失敗 変数の型:{typeCode} 文字列:{val}", errorInfo) { }
  }

  public class ArrayIndexException : CellDataConverterException {
    public ArrayIndexException(string str, IErrorInfo errorInfo)
      : base($"値の代入に失敗。配列の添え字を取得できませんでした index:{str}", errorInfo) { }
  }

  public class FieldNameException : CellDataConverterException {
    public FieldNameException(string fieldName, IErrorInfo errorInfo)
      : base($"値の代入に失敗。フィールドを取得できませんでした fieldName:{fieldName}", errorInfo) { }
  }

  public class TargetObjectNotFoundException : CellDataConverterException {
    public TargetObjectNotFoundException(IErrorInfo errorInfo)
      : base("targetObjectが存在しません。.class設定を確認してください", errorInfo) { }
  }

  public class ClassTypeNotFoundException : CellDataConverterException {
    public ClassTypeNotFoundException(string label, IErrorInfo errorInfo)
      : base($"クラスが見つかりません label:{label}", errorInfo) { }
  }

  public class DuplicateDefinitionsException : CellDataConverterException {
    public DuplicateDefinitionsException(IErrorInfo errorInfo)
      : base("クラス定義が重複しています", errorInfo) { }
  }

  public class ObjectNotFoundException : CellDataConverterException {
    public ObjectNotFoundException(IErrorInfo errorInfo)
      : base("オブジェクトが存在しません", errorInfo) { }
  }

  public class CustomFunctionNotFoundException : CellDataConverterException {
    public CustomFunctionNotFoundException(string attributeName, IErrorInfo errorInfo)
      : base($"関数が見つかりません attributeName:{attributeName}", errorInfo) { }
  }

  public class InvokeException : CellDataConverterException {
    public InvokeException(string attributeName, IErrorInfo errorInfo)
      : base($"関数呼び出しに失敗 attributeName:{attributeName}", errorInfo) { }
  }

  public class UnknownException : CellDataConverterException {
    public UnknownException(string str, string output, IErrorInfo errorInfo)
      : base($"未知のエラーです str:{str} output:{output}", errorInfo) { }
  }
}