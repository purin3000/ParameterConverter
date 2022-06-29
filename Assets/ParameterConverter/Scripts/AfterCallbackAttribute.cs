using System;

namespace ParameterConverter {
  [AttributeUsage(AttributeTargets.Method)]
  public class AfterCallbackAttribute
    : Attribute {
    public AfterCallbackAttribute(string label) {
      Label = label;
    }

    public string Label { get; }
  }
}