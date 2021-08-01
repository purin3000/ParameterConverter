using UnityEngine;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Object = System.Object;

namespace ParameterConverter.Editor
{
    public interface IErrorInfo
    {
        string Message { get; }
    }

    public interface ILexicalCastHook
    {
        bool Cast(Type scriptableObjectType, FieldInfo fieldInfo, Type targetType, object val, int arrayIndex, out object obj, IErrorInfo errorInfo);
    }

    internal static class ReflectionUtil
    {
        readonly static Regex RegSplit = new Regex(@"^([^.]+)\.(.+)");
        readonly static Regex RegArray = new Regex(@"^([^.\[\]]+)\[(\d+)\]");

        /// ★DLLの注意事項
        /// エディタ拡張から実行した場合、Assembly-CSharp-Editorしか読まれていない状態のため、
        /// インゲーム側のModuleを使用可能にするため、明示的にDLLを指定して読み込んでいます。
        /// AsemdefなどでDLLを分割した場合、ここにDLLを追加する必要があると思われます。
        private readonly static string[] DLLs =
        {
            "Library/ScriptAssemblies/Assembly-CSharp.dll",
            "Library/ScriptAssemblies/Assembly-CSharp-Editor.dll",
        };


        private static List<Module> modules;
        public static List<Module> Modules {
            get { 
                if (modules == null) {
                    modules = new List<Module>();
                    foreach (var path in DLLs) {
                        if (File.Exists(path)) {
                            var asm = Assembly.LoadFrom(path);
                            modules.AddRange(asm.GetModules());
                        } else {
                            Debug.LogWarning($"DLL not found. path:{path}");
                        }
                    }

                }
                return modules;
            }
        }

        /// <summary>
        /// DLLを検索して型を返す
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type SearchType(string typeName)
        {
            foreach (var module in Modules) {
                var type = module.GetType(typeName);
                if (type != null) {
                    return type;
                }
            }
            return null;
        }


        /// <summary>
        /// AfterCallbackAttributeが付いたMethodを取得する
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static MethodInfo GetCustomMethod(string label) => AttributeAccessor.GetCustomMethod(label);
        public static Type GetCustomClass(string label) => AttributeAccessor.GetCustomClass(label);

        /// <summary>
        /// 型からオブジェクト生成。ScriptableObjectにも対応しています
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Object CreateObject(Type t)
        {
            Debug.Assert(t != null);

            if (t.IsSubclassOf(typeof(ScriptableObject))) {
                return ScriptableObject.CreateInstance(t);
            } else {
                return Activator.CreateInstance(t);
            }
        }

        /// <summary>
        /// 指定したフィールドに値を代入
        /// 
        /// filed.childFiled.valueのようにコンマで区切ることで、子供のメンバを対象にできます
        /// field.buffer[1]のように配列の要素を指定することも可能です
        /// 途中のメンバのインスタンスが存在しない場合は適宜生成されます
        /// </summary>
        /// <param name="t"></param>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetFieldValue(Type t, object obj, string name, object value, IEnumerable<ILexicalCastHook> lexicalCastHooks, IErrorInfo errorInfo)
        {
            Debug.AssertFormat(obj != null, $"obj == null!! name:{name}");

            var m = RegSplit.Match(name);
            if (m.Success) {
                // 最後の要素ではない
                var (fieldInfo, _, arrayIndex) = GetFieldInfo(t, m.Groups[1].Value, errorInfo);
                var nextFieldName = m.Groups[2].Value;
                if (arrayIndex == -1) {
                    // 配列ではない
                    var fieldObj = fieldInfo.GetValue(obj);
                    if (fieldObj == null) {
                        // フィールドのインスタンスを作る
                        fieldObj = CreateObject(fieldInfo.FieldType);
                        fieldInfo.SetValue(obj, fieldObj);
                    }

                    SetFieldValue(fieldInfo.FieldType, fieldObj, nextFieldName, value, lexicalCastHooks, errorInfo);

                    if (fieldInfo.FieldType.IsValueType) {
                        // 値型の場合はBoxsingの都合でコピーに対して設定されているので、コピーを設定しなおす必要がある
                        fieldInfo.SetValue(obj, fieldObj);
                    }

                } else {
                    // 配列である
                    var arrayInstance = GetArrayInstance(obj, fieldInfo, arrayIndex);

                    var elementType = fieldInfo.FieldType.GetElementType();

                    var elementObj = arrayInstance.GetValue(arrayIndex);
                    if (elementType.IsClass && elementObj == null) {
                        // 要素のインスタンスを作る
                        elementObj = CreateObject(elementType);
                        arrayInstance.SetValue(elementObj, arrayIndex);
                    }

                    SetFieldValue(elementType, elementObj, nextFieldName, value, lexicalCastHooks, errorInfo);

                    if (elementType.IsValueType) {
                        // 値型の場合はBoxsingの都合でコピーに対して設定されているので、コピーを設定しなおす必要がある
                        arrayInstance.SetValue(elementObj, arrayIndex);
                    }
                }

            } else {
                // 最後の要素である
                var (fieldInfo, _, arrayIndex) = GetFieldInfo(t, name, errorInfo);
                if (arrayIndex == -1) {
                    // 配列ではない

                    // フィールドに値を設定
                    var newValue = LexicalCast2(t, fieldInfo, fieldInfo.FieldType, value, -1, lexicalCastHooks, errorInfo);
                    fieldInfo.SetValue(obj, newValue);
                } else {
                    // 配列である。string[]のような単純な配列の場合
                    var arrayInstance = GetArrayInstance(obj, fieldInfo, arrayIndex);

                    // 配列に値を設定
                    var newValue = LexicalCast2(t, fieldInfo, fieldInfo.FieldType.GetElementType(), value, arrayIndex, lexicalCastHooks, errorInfo);
                    arrayInstance.SetValue(newValue, arrayIndex);
                }
            }
        }

        public static object LexicalCast2(Type scriptableObjectType, FieldInfo fieldInfo, Type targetType, object value, int arrayIndex, IEnumerable<ILexicalCastHook> lexicalCastHooks, IErrorInfo errorInfo)
        {
            foreach (var hook in lexicalCastHooks) {
                if (hook.Cast(scriptableObjectType, fieldInfo, targetType, value, arrayIndex, out var newValue, errorInfo)) {
                    return newValue;
                }
            }
            return LexicalCast(targetType, value, errorInfo);
        }

        /// <summary>
        /// ターゲットに合わせてキャスト
        /// シリアライズ時に型が違うとエラーになるので
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object LexicalCast(Type targetType, object value, IErrorInfo errorInfo)
        {
            if (targetType.IsEnum) { //enumはInt32で来る
                var label = value.ToString();
                if (!string.IsNullOrEmpty(label)) {
                    int index = Array.FindIndex(Enum.GetNames(targetType), (__str) => __str == label);
                    if (index != -1) {
                        return Enum.GetValues(targetType).GetValue(index);
                    }
                    throw new EnumNotFoundException(targetType.Name, label, errorInfo);

                } else {
                    return 0;
                }
            }

            var typeCode = Type.GetTypeCode(targetType);
            try {
                if (typeCode == TypeCode.Int32) {
                    if (string.IsNullOrEmpty(value.ToString())) {
                        return 0;
                    }
                    return Convert.ToInt32(value);
                }
                if (typeCode == TypeCode.Int16) {
                    if (string.IsNullOrEmpty(value.ToString())) {
                        return 0;
                    }
                    return Convert.ToInt16(value);
                }
                if (typeCode == TypeCode.SByte) {
                    if (string.IsNullOrEmpty(value.ToString())) {
                        return 0;
                    }
                    return Convert.ToSByte(value);
                }
                if (typeCode == TypeCode.UInt32) {
                    if (string.IsNullOrEmpty(value.ToString())) {
                        return 0;
                    }
                    return Convert.ToUInt32(value);
                }
                if (typeCode == TypeCode.UInt16) {
                    if (string.IsNullOrEmpty(value.ToString())) {
                        return 0;
                    }
                    return Convert.ToUInt16(value);
                }
                if (typeCode == TypeCode.Byte) {
                    if (string.IsNullOrEmpty(value.ToString())) {
                        return 0;
                    }
                    return Convert.ToByte(value);
                }
                if (typeCode == TypeCode.Single) {
                    if (string.IsNullOrEmpty(value.ToString())) {
                        return 0;
                    }
                    return Convert.ToSingle(value);
                }
                if (typeCode == TypeCode.Double) {
                    if (string.IsNullOrEmpty(value.ToString())) {
                        return 0;
                    }
                    return Convert.ToDouble(value);
                }
                if (typeCode == TypeCode.Boolean) {
                    var label = value.ToString().ToLower();
                    if (string.IsNullOrEmpty(label)) {
                        return false;
                    }
                    if (label == "true") {
                        return true;
                    }
                    if (label == "false") {
                        return false;
                    }
                    if (label == "0") {
                        return false;
                    }
                    return true;
                }
            }
            catch {
                throw new ValueTypeException(typeCode, value, errorInfo);
            }

            if (string.IsNullOrEmpty(value.ToString())) {
                return "";
            }
            return Convert.ToString(value);
        }

        /// <summary>
        /// フィールド要素の取得
        /// fieldNameで指定されるフィールドが配列の場合、配列の添え字も取得します
        /// </summary>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <param name="fieldName"></param>
        /// <param name="arrayIndex"></param>
        /// <returns></returns>
        private static (FieldInfo, string fieldName, int arrayIndex) GetFieldInfo(Type t, string name, IErrorInfo errorInfo)
        {
            string fieldName = name;
            int arrayIndex = -1;

            var ma = RegArray.Match(name);
            if (ma.Success) {
                // 配列時フィールド名と添え字を取得
                fieldName = ma.Groups[1].Value;
                try {
                    arrayIndex = Convert.ToInt32(ma.Groups[2].Value);
                }
                catch {
                    throw new ArrayIndexException(ma.Groups[2].Value, errorInfo);
                }
            }

            var fieldInfo = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty);
            if (fieldInfo == null) {
                throw new FieldNameException(fieldName, errorInfo);
            }

            return (fieldInfo, fieldName, arrayIndex);
        }

        /// <summary>
        /// 配列のインスタンスを取得
        /// 必要に応じて配列がなければ作るし、長さが足りなければ配列拡張して内容をコピーもします
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldInfo"></param>
        /// <param name="arrayIndex"></param>
        /// <returns></returns>
        private static Array GetArrayInstance(object obj, FieldInfo fieldInfo, int arrayIndex)
        {
            var elementType = fieldInfo.FieldType.GetElementType();

            var arrayInstance = fieldInfo.GetValue(obj) as Array;
            if (arrayInstance == null) {
                // 配列初期化
                arrayInstance = Array.CreateInstance(elementType, arrayIndex + 1);
                fieldInfo.SetValue(obj, arrayInstance);
            }


            if (arrayInstance.Length <= arrayIndex) {
                // 配列拡張
                var src = arrayInstance;
                var dst = Array.CreateInstance(elementType, arrayIndex + 1);

                src.CopyTo(dst, 0);

                arrayInstance = dst;
                fieldInfo.SetValue(obj, arrayInstance);
            }

            return arrayInstance;
        }

    }
}

