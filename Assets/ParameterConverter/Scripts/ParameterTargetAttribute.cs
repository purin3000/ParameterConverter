using System;

namespace ParameterConverter {
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
  public class ParameterTargetAttribute
    : Attribute {
    public ParameterTargetAttribute(string label) {
      Label = label;
    }

    public string Label { get; }
  }
}