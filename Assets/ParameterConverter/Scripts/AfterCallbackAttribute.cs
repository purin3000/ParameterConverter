using System;

namespace ParameterConverter
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AfterCallbackAttribute
        : Attribute
    {
        private string label;
        public string Label => label;

        public AfterCallbackAttribute(string label)
        {
            this.label = label;
        }
    }

}
