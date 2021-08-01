using System;

namespace ParameterConverter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class ParameterTargetAttribute
        : Attribute
    {
        private string label;
        public string Label => label;

        public ParameterTargetAttribute(string label)
        {
            this.label = label;
        }
    }
}
