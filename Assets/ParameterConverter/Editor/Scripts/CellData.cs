using System.Linq;
using System.Text.RegularExpressions;
using ParameterConverter.Editor.Internal;

namespace ParameterConverter.Editor {
  public struct Location {
    public Location(int row, int col) {
      this.row = row;
      this.col = col;
    }

    public int row;
    public int col;
  }

  public struct MinMax {
    public MinMax(int min, int max) {
      this.min = min;
      this.max = max;
    }

    public int min;
    public int max;
  }

  public struct TableRect {
    public TableRect(MinMax row, MinMax col) {
      this.row = row;
      this.col = col;
    }

    public MinMax row;
    public MinMax col;
  }
  
  /// <summary>
  /// コメントはクリアされた状態で全てセルが格納されます
  /// </summary>
  public class CellData {
    private static readonly Regex RegLineComment = new(@"^#[^#]?");
    private static readonly Regex RegCellComment = new(@"^##[^#]?");

    private readonly string[,] cells;

    public string SheetName { get; }
    public string AssetPath { get; }

    /// <summary>
    ///     最終行取得
    ///     空白で区切って最終を取得する場合はGetLastRow()を使用すること
    /// </summary>
    public int RowLength => cells.GetLength(0);

    /// <summary>
    ///     最終列取得
    ///     空白で区切って最終を取得する場合はGetLastColumn()を使用すること
    /// </summary>
    public int ColumunLength => cells.GetLength(1);

    public CellData(string assetPath, string sheetName, string[,] cells) {
      AssetPath = assetPath;
      SheetName = sheetName;
      this.cells = cells;

      for (var row = 0; row < RowLength; ++row)
      for (var col = 0; col < ColumunLength; ++col) {
        var str = cells[row, col];

        if (string.IsNullOrEmpty(str) || RegCellComment.IsMatch(str)) {
          cells[row, col] = "";
        } else if (RegLineComment.IsMatch(str)) {
          for (var col2 = col; col2 < ColumunLength; ++col2) cells[row, col2] = "";
          break;
        }
      }
    }

    public string GetCell(Location location) {
      return GetCell(location.row, location.col);
    }

    /// <summary>
    ///     セル取得
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public string GetCell(int row, int col) {
      if (row < RowLength && col < ColumunLength) {
        return cells[row, col];
      }
      return "";
    }

    public bool IsCommandCell(string str) {
      var s = str.ToLower();
      return CellDataCommand.COMMAND_LIST.Any(ss => ss == s);
    }

    public bool IsEmptyRow(int row, int colMin, int colMax) {
      for (var col = colMin; col < colMax; ++col) {
        var str = GetCell(row, col);
        if (string.IsNullOrEmpty(str)) {
          continue;
        }
        return false;
      }

      return true;
    }

    public bool IsEmptyCol(int col, int rowMin, int rowMax) {
      for (var row = rowMin; row < rowMax; ++row) {
        var str = GetCell(row, col);
        if (string.IsNullOrEmpty(str)) {
          continue;
        }
        return false;
      }

      return true;
    }

    public TableRect CalcTableRect(int row, int col) {
      var (rowMin, colMin) = SkipEmptyCell(row, col);
      var (rowMax, colMax) = CalcTableLimit(row, col);
      return new TableRect(new MinMax(rowMin, rowMax), new MinMax(colMin, colMax));
    }

    public (int row, int col) SkipEmptyCell(int row, int col) {
      for (; row < RowLength; ++row) {
        for (; col < ColumunLength; ++col) {
          var str = GetCell(row, col);
          if (string.IsNullOrEmpty(str)) {
            continue;
          }
          return (row, col);
        }

        col = 0;
      }

      return (RowLength, ColumunLength);
    }

    /// <summary>
    ///     指定したセルを起点としたテーブルの最大サイズを取得
    ///     何らかのコマンドによって解析を終了
    /// </summary>
    /// <param name="startRow"></param>
    /// <param name="startCol"></param>
    public (int row, int col) CalcTableLimit(int startRow, int startCol) {
      var lastRow = RowLength;
      var lastCol = ColumunLength;

      for (var row = startRow + 1; row < RowLength; ++row) {
        var str = GetCell(row, startCol);
        if (IsCommandCell(str)) {
          lastRow = row;
          break;
        }
      }

      for (var col = startCol + 1; col < ColumunLength; ++col) {
        var str = GetCell(startRow, col);
        if (IsCommandCell(str)) {
          lastCol = col;
          break;
        }
      }

      return (lastRow, lastCol);
    }
  }
}