using System;

namespace ParameterConverter {
  /// <summary>
  /// コンバート対象にするクラスを指定
  /// 
  /// ここで指定した名前をExcel側のclass指定で使用します。
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
  public class ConvertTargetAttribute
    : Attribute {
    public ConvertTargetAttribute(string label) {
      Label = label;
    }

    public string Label { get; }
  }
}