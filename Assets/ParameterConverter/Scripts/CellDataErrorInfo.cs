using System;
using ParameterConverter.Editor;

namespace ParameterConverter.Scripts {
  public class CellDataErrorInfo : IErrorInfo {
    private readonly CellData cellData;
    private readonly Location location;

    public CellDataErrorInfo(CellData cellData, Location location) {
      this.cellData = cellData;
      this.location = location;
    }

    private string Str => $"文字列:{cellData.GetCell(location)}";
    private string SheetName => $"Sheet:{cellData.SheetName}";
    private string AssetPath => $"AssetPath:{cellData.AssetPath}";
    private string ExcelRow => $"{location.row + 1}行";

    private string ExcelCol {
      get {
        string GetCode(int v) {
          return Convert.ToString((char)('A' + v % 26));
        }

        var col = location.col;
        var ret = GetCode(col);
        while (true) {
          col /= 26;
          if (0 < col)
            ret = GetCode(col) + ret;
          else
            break;
        }

        return ret + "列";
      }
    }

    public string Message => $"\n{Str} {SheetName} {ExcelRow} {ExcelCol}\n{AssetPath}";
  }
}