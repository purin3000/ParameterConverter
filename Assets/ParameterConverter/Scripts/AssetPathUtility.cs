using UnityEngine;
using System.IO;

namespace ParameterConverter
{
    public static class AssetPathExtension
    {
        public static string GetDirectoryName(this string str) => AssetPathUtility.GetDirectoryName(str);
        public static string GetFileName(this string str) => AssetPathUtility.GetFileName(str);
        public static string GetFileNameWithoutExtension(this string str) => AssetPathUtility.GetFileNameWithoutExtension(str);
        public static string GetExtension(this string str) => AssetPathUtility.GetExtension(str);
        public static string ChangeSeparator(this string str) => AssetPathUtility.ChangeSeparator(str);
        public static string ChangeExtension(this string str, string ext) => AssetPathUtility.ChangeExtension(str, ext);
    }

    public class AssetPathUtility
    {
        public static string GetDirectoryName(string str)
        {
            try {
                if (!string.IsNullOrEmpty(str)) {
                    return ChangeSeparator(Path.GetDirectoryName(str));
                }
                return "";
            }
            catch {
                Debug.Log($"error string:{str}");
                return "";
            }
        }

        public static string GetFileName(string str)
        {
            return ChangeSeparator(Path.GetFileName(str));
        }

        public static string GetFileNameWithoutExtension(string str)
        {
            return ChangeSeparator(Path.GetFileNameWithoutExtension(str));
        }

        public static string GetExtension(string str)
        {
            return Path.GetExtension(str);
        }

        /// <summary>
        /// �Z�p���[�^�[���X���b�V���֓��ꂷ��
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ChangeSeparator(string str)
        {
            return str.Replace(@"\", "/");
        }

        /// <summary>
        /// �g���q��ύX
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ext">�s���I�h�͂Ȃ��Ă��ǂ��B�Ȃ��ꍇ�͓����Œǉ������</param>
        /// <returns></returns>
        public static string ChangeExtension(string str, string ext)
        {
            return ChangeSeparator(Path.ChangeExtension(str, ext));
        }
    }
}
