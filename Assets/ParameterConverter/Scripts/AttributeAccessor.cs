using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ParameterConverter.Editor;

namespace ParameterConverter.Scripts {
  public static class AttributeAccessor {
    private static readonly CustomMethodUtil customMethodUtil = new();

    private static readonly CustomClassUtil customClassUtil = new();

    public static MethodInfo GetCustomMethod(string label) {
      return customMethodUtil.GetCustomMethod(label);
    }

    public static Type GetCustomClass(string label) {
      return customClassUtil.GetCustomType(label);
    }


    private class CustomMethodUtil {
      private readonly Dictionary<string, MethodInfo> methods = new();

      public CustomMethodUtil() {
        var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        var customMethods = ReflectionUtil.Modules
          .SelectMany(module => module.GetTypes())
          .SelectMany(type => type.GetMethods(flags))
          .Select(method => (method, GetCustomMethod(method)))
          .Where(obj => obj.Item2 != null).ToArray();

        foreach (var (method, attribute) in customMethods) methods.Add(attribute.Label, method);
      }

      public MethodInfo GetCustomMethod(string label) {
        if (methods.TryGetValue(label, out var methodInfo)) return methodInfo;
        return null;
      }

      private AfterCallbackAttribute GetCustomMethod(MethodInfo method) {
        return (AfterCallbackAttribute)Attribute.GetCustomAttribute(method, typeof(AfterCallbackAttribute));
      }
    }

    private class CustomClassUtil {
      private readonly Dictionary<string, Type> types = new();

      public CustomClassUtil() {
        var customMethods = ReflectionUtil.Modules
          .SelectMany(module => module.GetTypes())
          .Select(type => (type, GetCustomClass(type)))
          .Where(obj => obj.Item2 != null).ToArray();

        foreach (var (type, attribute) in customMethods) types.Add(attribute.Label, type);
      }

      public Type GetCustomType(string label) {
        if (types.TryGetValue(label, out var type)) return type;
        return null;
      }

      private ParameterTargetAttribute GetCustomClass(Type type) {
        return (ParameterTargetAttribute)Attribute.GetCustomAttribute(type, typeof(ParameterTargetAttribute));
      }
    }
  }
}