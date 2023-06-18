using System.Threading.Tasks;
using ParameterConverter.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ParameterConverter.Sample.Editor {
  public static class GoogleSpreadSheetTest {
    private const string HEADER = "https://docs.google.com/spreadsheets/d";
    private const string ID = "11KNuEJ3g05e_nglNO4rSTkdnImUO-LHpxz8ZOE_kOg4";
    private const string EXPORT_CSV = "export?gid=0&format=csv";

    private static string URL => $"{HEADER}/{ID}/{EXPORT_CSV}";

    [MenuItem("Convert/Google Spread Sheet")]
    private static void ConvertGoogleSpreadSheet() {
      var _ = ConvertAsync();
    }

    private static async Task ConvertAsync() {
      Debug.Log($"START:{URL}");
      // 注意事項：スプレッドシートの共有設定について
      // 共有設定でリンクを知っていればダウンロードできる設定になっていないとロードに失敗します。
      // ブラウザと違って認証を自動的に通してくれないみたい。
      var text = await LoadAsync(URL);
      if (string.IsNullOrEmpty(text)) {
        return;
      }
      var list = CsvReader.CreateCellDataFromString(text, "SpreadSheet","Sheet");
      CellDataConverter.Convert(list);
      Debug.Log($"END:{URL}");
    }

    private static async Task<string> LoadAsync(string url) {
      var req = new UnityWebRequest(url);
      req.downloadHandler = new DownloadHandlerBuffer();

      await req.SendWebRequest();

      if (req.result != UnityWebRequest.Result.Success) {
        Debug.LogError(req.error);
        return null;
      }
      return req.downloadHandler.text;
    }
  }
}
