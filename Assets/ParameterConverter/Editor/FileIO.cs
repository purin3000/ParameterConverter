using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace ParameterConverter.Editor
{
    public class FileIO
    {
        public static void OpenExplorer(string pathStr)
        {
            var openPath = $"/e,{Directory.GetCurrentDirectory()}\\{pathStr.Replace("/", "\\")}";
            System.Diagnostics.Process.Start("explorer", openPath);
        }

        public static string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory().ChangeSeparator();
        }

        public static string GetAbsolutePath(string str)
        {
            var path = (Directory.GetCurrentDirectory() + "/" + str).ChangeSeparator();
            return path;
        }

        public static void CreateAsset(UnityEngine.Object obj, string assetPath)
        {
            CreateDirectory(assetPath.GetDirectoryName());
            AssetDatabase.CreateAsset(obj, assetPath);
        }

        public static void CreateDirectory(string str)
        {
            if (string.IsNullOrEmpty(str) || ExistDirectory(str)) {
                return;
            }

            if (str.StartsWith("Assets/")) {
                createDirectoryByAssetDatabase(str);
            } else {
                createDirectoryBySystemIO(str);
            }
        }

        public static void DeleteDirectory(string str)
        {
            if (string.IsNullOrEmpty(str) || !ExistDirectory(str)) {
                return;
            }

            if (str.StartsWith("Assets/")) {
                AssetDatabase.DeleteAsset(str);
            } else {
                try {
                    Directory.Delete(str, true);
                }
                catch {
                    // 何故か失敗することがあるので一度だけリトライ
                    System.Threading.Thread.Sleep(500);
                    Directory.Delete(str, true);
                }
            }
        }

        public static bool ExistDirectory(string str)
        {
            if (str.StartsWith("Assets/")) {
                return AssetDatabase.IsValidFolder(str);
            } else {
                return Directory.Exists(str);
            }
        }

        public static bool ExistFile(string str)
        {
            return File.Exists(str);
        }

        public static void DeleteFile(string str)
        {
            if (ExistFile(str)) {
                if (str.StartsWith("Assets/")) {
                    AssetDatabase.DeleteAsset(str);
                } else {
                    try {
                        File.Delete(str);
                    }
                    catch {
                        // 何故か失敗することがあるので一度だけリトライ
                        System.Threading.Thread.Sleep(200);
                        File.Delete(str);
                    }
                }
            }
        }

        public static void WriteAllText(string path, string text)
        {
            CreateDirectory(path.GetDirectoryName());

            File.WriteAllText(path, text);
        }

        public static void WriteAllText(string path, string text, System.Text.Encoding encoding)
        {
            CreateDirectory(path.GetDirectoryName());

            File.WriteAllText(path, text, encoding);
        }

        public static T ReadJsonAutoCreate<T>(string path) where T : class, new()
        {
            if (File.Exists(path)) {
                return ReadJson<T>(path);
            }
            return new T();
        }

        public static T ReadJson<T>(string path) where T : class, new()
        {
            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }

        public static void WriteJson(string path, object obj)
        {
            var json = JsonUtility.ToJson(obj, true);
            WriteAllText(path, json);
        }

        /// <summary>
        /// コピーしてついでにアセットの名前を指定する
        /// 
        /// アセットのパラメーターを引き継ぐ場合にアセット名が空欄になるのを防ぐためのもの
        /// 
        /// インポートタイミングよっては新規アセットはメモリ上にしか存在しないため、
        /// まだファイル名が名前が決まっていないのでアセット名が空欄になる。
        /// この段階でコピーすると当然、アセット名が空欄になってしまう。
        /// 空欄だとUnityが適当なタイミングで勝手に名前を付けるため、無用なデータの更新が起きてしまう。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="assetName"></param>
        public static void CopySerializedSetName(UnityEngine.Object source, UnityEngine.Object dest, string assetName)
        {
            // アセット名が空欄になるのを防ぐため自分で名前を引き継ぐ
            // 多分まだアセット化されていない場合、名前が決まっていないのだろう
            EditorUtility.CopySerialized(source, dest);
            dest.name = assetName;
            AssetDatabase.SaveAssets();
        }

        public static void CreateOrReplaceAsset(string assetPath, string assetName, Type type, UnityEngine.Object obj)
        {
            var oldAsset = AssetDatabase.LoadAssetAtPath(assetPath, type);
            if (oldAsset) {
                CopySerializedSetName(obj, oldAsset, assetName);
            } else {
                CreateDirectory(assetPath.GetDirectoryName());
                AssetDatabase.CreateAsset(obj, assetPath);
            }
        }

        private static void createDirectoryByAssetDatabase(string str)
        {
            if (!string.IsNullOrEmpty(str) && !AssetDatabase.IsValidFolder(str)) {
                var parent = str.GetDirectoryName();
                createDirectoryByAssetDatabase(parent);

                AssetDatabase.CreateFolder(parent, Path.GetFileName(str));
            }
        }

        private static void createDirectoryBySystemIO(string str)
        {
            if (!string.IsNullOrEmpty(str) && !Directory.Exists(str)) {
                var parent = str.GetDirectoryName();
                createDirectoryBySystemIO(parent);

                Directory.CreateDirectory(str);
            }
        }

    }
}
