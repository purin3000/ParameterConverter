using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace ParameterConverter.Editor {
    public static class AttributeAccessor
    {
        readonly static CustomMethodUtil customMethodUtil = new CustomMethodUtil();
        public static MethodInfo GetCustomMethod(string label) => customMethodUtil.GetCustomMethod(label);

        readonly static CustomClassUtil customClassUtil = new CustomClassUtil();
        public static Type GetCustomClass(string label) => customClassUtil.GetCustomType(label);


        class CustomMethodUtil
        {
            private Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

            public CustomMethodUtil()
            {
                var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

                var customMethods = ReflectionUtil.Modules
                    .SelectMany(module => module.GetTypes())
                    .SelectMany(type => type.GetMethods(flags))
                    .Select(method => (method, GetCustomMethod(method)))
                    .Where(obj => obj.Item2 != null).ToArray();

                foreach (var (method, attribute) in customMethods) {
                    methods.Add(attribute.Label, method);
                }
            }

            public MethodInfo GetCustomMethod(string label)
            {
                if (methods.TryGetValue(label, out var methodInfo)) {
                    return methodInfo;
                }
                return null;
            }

            private AfterCallbackAttribute GetCustomMethod(MethodInfo method)
                           => (AfterCallbackAttribute)Attribute.GetCustomAttribute(method, typeof(AfterCallbackAttribute));
        }

        class CustomClassUtil
        {
            private Dictionary<string, Type> types = new Dictionary<string, Type>();

            public CustomClassUtil()
            {
                var customMethods = ReflectionUtil.Modules
                    .SelectMany(module => module.GetTypes())
                    .Select(type => (type, GetCustomClass(type)))
                    .Where(obj => obj.Item2 != null).ToArray();

                foreach (var (type, attribute) in customMethods) {
                    types.Add(attribute.Label, type);
                }
            }

            public Type GetCustomType(string label)
            {
                if (types.TryGetValue(label, out var type)) {
                    return type;
                }
                return null;
            }

            private ParameterTargetAttribute GetCustomClass(Type type)
                           => (ParameterTargetAttribute)Attribute.GetCustomAttribute(type, typeof(ParameterTargetAttribute));

        }
    }

}
