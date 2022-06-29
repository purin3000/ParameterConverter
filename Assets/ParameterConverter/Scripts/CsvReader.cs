using System.Collections.Generic;
using System.Text;
using ParameterConverter.Editor;

namespace ParameterConverter.Scripts {
  public static class CsvReader {
    public static List<CellData> CreateCellDataFromFile(string assetPath, Encoding encoding) {
      var obj = CsvSplitter.SplitFromFile(assetPath, encoding);

      var list = new List<CellData>();
      list.Add(new CellData(assetPath, assetPath.GetFileName(), obj));
      return list;
    }
 
    public static List<CellData> CreateCellDataFromString(string str, string assetPath, string sheetName) {
      var obj = CsvSplitter.SplitFromString(str);

      var list = new List<CellData>();
      list.Add(new CellData(assetPath, sheetName, obj));
      return list;
    }
  }
}
