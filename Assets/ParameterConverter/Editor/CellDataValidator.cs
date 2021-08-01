using System;
using System.Collections.Generic;
using System.Reflection;

namespace ParameterConverter.Editor
{
    /// <summary>
    /// .idに格納する値が一意ではない場合はエラーにする
    /// </summary>
    public class ArrayValueValidator : ILexicalCastHook
    {
        private List<string> idList = new List<string>();

        public bool Cast(Type scriptableObjectType, FieldInfo fieldInfo, Type targetType, object val, int arrayIndex, out object obj, IErrorInfo errorInfo)
        {
            if (fieldInfo.Name == "id" && 0 <= arrayIndex) {
                var str = val.ToString();
                if (idList.Contains(str)) {
                    throw new IdValidatorException(scriptableObjectType, arrayIndex, errorInfo);
                }
                idList.Add(str);
            }

            obj = null;
            return false;
        }
    }
}
