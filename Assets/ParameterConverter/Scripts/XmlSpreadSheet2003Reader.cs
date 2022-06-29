using System.Collections.Generic;
using System.IO;
using System.Xml;
using ParameterConverter.Editor;

namespace ParameterConverter.Scripts {
  public class XmlSpreadSheet2003Reader {
    public static List<CellData> CreateCellData(string assetPath) {
      var ret = new List<CellData>();

      using (var stream = new FileStream(assetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
        var doc = new XmlDocument();
        doc.Load(stream);

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("a", "urn:schemas-microsoft-com:office:spreadsheet");

        var worksheets = doc.SelectNodes("//a:Workbook/a:Worksheet", nsmgr);
        foreach (XmlElement ws in worksheets) {
          var table = ws.SelectSingleNode("a:Table", nsmgr) as XmlElement;
          if (table == null) continue;

          var cells = ParseTable(table, nsmgr);

          var sheetName = ws.GetAttribute("ss:Name");
          var cellData = new CellData(assetPath, sheetName, cells);
          ret.Add(cellData);
        }
      }

      return ret;
    }

    private static string[,] ParseTable(XmlElement table, XmlNamespaceManager nsmgr) {
      var expandedRowCount = int.Parse(table.GetAttribute("ss:ExpandedRowCount"));
      var expandedColumnCount = int.Parse(table.GetAttribute("ss:ExpandedColumnCount"));

      var cells = new string[expandedRowCount, expandedColumnCount];

      var rowNodes = table.SelectNodes("a:Row", nsmgr);
      foreach ((var rowIndex, var rowNode) in rowNodes.Indexed<XmlElement>()) {
        var cellNodes = rowNode.SelectNodes("a:Cell", nsmgr);
        var colOffset = 0;
        foreach ((var columnIndex, var cellElem) in cellNodes.Indexed<XmlElement>()) {
          var indexStr = cellElem.GetAttribute("ss:Index");
          if (!string.IsNullOrEmpty(indexStr)) colOffset = int.Parse(indexStr) - columnIndex - 1;
          var text = cellElem.InnerText;
          text = text.Replace("\r", "");
          cells[rowIndex, columnIndex + colOffset] = text;
        }
      }

      return cells;
    }
  }
}