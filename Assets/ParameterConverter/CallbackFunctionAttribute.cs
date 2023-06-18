using System;

namespace ParameterConverter {
  /// <summary>
  /// コンバート後に呼び出す関数を指定
  ///
  /// コンバート後にマージする場合など、コンバート後に呼び出される関数を指定します。
  /// ここで指定したラベルをExcel側で指定します。
  /// </summary>
  [AttributeUsage(AttributeTargets.Method)]
  public class CallbackFunctionAttribute
    : Attribute {
    public CallbackFunctionAttribute(string label) {
      Label = label;
    }

    public string Label { get; }
  }
}