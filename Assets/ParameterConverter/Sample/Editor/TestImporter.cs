using System.Collections.Generic;
using System.Text;
using ParameterConverter.Editor;
using ParameterConverter.Utils;
using UnityEditor;
using UnityEngine;

namespace ParameterConverter.Sample.Editor {
  /// <summary>
  /// データが更新されたら自動的にインポートします
  /// </summary>
  public class TestImporter : AssetPostprocessor {
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
      string[] movedAssets, string[] movedFromAssetPaths) {
      var imported = false;

      foreach (var assetPath in importedAssets) {
        var ext = assetPath.GetExtension().ToLower();

        try {
          if (ext == ".csv") {
            Convert(assetPath, CsvReader.CreateCellDataFromFile(assetPath, Encoding.UTF8));
            imported = true;
          } else if (ext == ".xml") {
            Convert(assetPath, XmlSpreadSheet2003Reader.CreateCellData(assetPath));
            imported = true;
          }
        } catch (CellDataConverterException e) {
          Debug.LogWarning(e.Message);
        }
      }

      if (imported) {
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
      }
    }

    private static void Convert(string assetPath, List<CellData> list) {
      Debug.Log(assetPath);
      CellDataConverter.Convert(list);
    }

    [CallbackFunction("ConvertTest")]
    private static void CallbackTest(string assetPath, string arg) {
      Debug.Log($"コールバックが呼ばれました {assetPath} {arg}");
    }
  }
}