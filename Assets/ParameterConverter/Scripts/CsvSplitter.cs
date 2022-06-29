using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ParameterConverter.Scripts {
  public static class CsvSplitter {
    private const string DQ_CODE = "$_DQDQ_$";
    private static readonly Regex REG_SPLIT = new(",|\"([^\"]+)\"");

    public static string[,] SplitFromString(string str) {
      return LinesToCells(StringToLines(str));
    }

    public static string[,] SplitFromFile(string filePath, Encoding encoding) {
      return LinesToCells(FileToLines(filePath, encoding));
    }

    private static List<string> FileToLines(string filePath, Encoding encoding) {
      var result = new List<string>();
      using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      using (var reader = new StreamReader(stream, encoding)) {
        while (!reader.EndOfStream) {
          result.Add(reader.ReadLine());
        }
      }
      return result;
    }

    private static List<string> StringToLines(string str) {
      var result = new List<string>();
      using (var reader = new StringReader(str)) {
        string l;
        while ((l = reader.ReadLine()) != null) {
          result.Add(l);
        }
      }
      return result;
    }
    
    private static string[,] LinesToCells(List<string> lines) {
      var cells = new List<string[]>();
      foreach (var line in lines) {
        var line2 = line.Replace("\"\"", DQ_CODE);
        var splitWords = REG_SPLIT.Split(line2);
        var replaceWords = splitWords.Select(str => 
          str.Replace(DQ_CODE, "\"\"")).ToArray();
        cells.Add(replaceWords);
      }

      var maxCells = cells.Max(list => list.Length);

      var cells2 = new string[cells.Count, maxCells];
      foreach (var (col, words) in cells.Indexed())
      foreach (var (row, word) in words.Indexed())
        cells2[col, row] = word;

      // var cell2 = cells.Select(list => list.ToArray()).ToArray();
      return cells2;
    }
  }
}