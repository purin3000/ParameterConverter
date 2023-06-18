using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ParameterConverter.Editor {
  public class ReplacePathPathStr : IReplacePathStr {
    private static readonly Regex REG_MACRO = new Regex(@"{([^}]+)}");

    private static readonly Dictionary<string, string> MACRO_TABLE = new() {
      {"SAMPLE_DATA", "Assets/ParameterConverter/Sample/Data"}
    };
    
    public string Convert(string src) {
      var m = REG_MACRO.Match(src);
      if (!m.Success) {
        return src;
      }
      if (MACRO_TABLE.TryGetValue(m.Groups[1].Value, out var str)) {
        return REG_MACRO.Replace(src, str);
      }
      return src;
    }
  }
}