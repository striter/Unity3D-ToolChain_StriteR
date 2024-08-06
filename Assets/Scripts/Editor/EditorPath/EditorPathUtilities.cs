using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityEditor.Extensions.EditorPath
{
    public static class UEPath
    {
        public static Dictionary<string, Func<string>> kReplacementRegex { get;private set; } = new() {
            { "<#activeSceneName>", () => UnityEngine.SceneManagement.SceneManager.GetActiveScene().path.GetFileName(true) },
            { "<#activeScenePath>", () => UnityEngine.SceneManagement.SceneManager.GetActiveScene().path.GetPathDirectory() },
        };

        public static Dictionary<string, Func<string>> kActivePath { get; private set; } = new() {
                { "Active Directory", GetCurrentProjectWindowDirectory },
            };
        
        public static string PathRegex(string _srcPath)
        {
            foreach (var (key, value) in kReplacementRegex)
                _srcPath = _srcPath.Replace(key, value());
            return _srcPath;
        }
        
        public static string FileToAssetPath(this string assetPath)
        {
            int assetIndex = assetPath.IndexOf("/Assets", StringComparison.Ordinal) + 1;
            assetPath = assetPath.Replace(@"\", @"/");
            if (assetIndex != 0)
                assetPath = assetPath.Substring(assetIndex, assetPath.Length - assetIndex);
            return assetPath;
        }

        public static string AssetToFilePath(this string assetPath)
        {
            assetPath=assetPath.Substring(6, assetPath.Length-6);
            return Application.dataPath + assetPath;
        }
        public static string GetExtension(this string path)
        {
            int extensionIndex = path.LastIndexOf('.');
            if (extensionIndex >= 0)
                return path.Remove(0,extensionIndex);
            return null;
        }
        public static string RemoveExtension(this string path)
        {
            int extensionIndex = path.LastIndexOf('.');
            if (extensionIndex >= 0)
                return path.Remove(extensionIndex);
            return path;
        }
        public static string GetPathName(this string path)
        {
            path = RemoveExtension(path);
            int folderIndex = path.LastIndexOf('/');
            if (folderIndex >= 0)
                path = path.Substring(folderIndex + 1, path.Length - folderIndex - 1);
            return path;
        }

        public static string GetFileName(this string path,bool _removeExtension = false)
        {
            int folderIndex = path.LastIndexOf('/');
            if (folderIndex >= 0)
                path = path.Substring(folderIndex + 1, path.Length - folderIndex - 1);
            
            if(_removeExtension)
                path = RemoveExtension(path);
            return path;
        }

        public static string GetPathDirectory(this string path)
        {
            var folderIndex = path.LastIndexOf('/');
            if (folderIndex >= 0)
                path = path.Substring(0, folderIndex);
            return path;
        } 
        public static string GetCurrentProjectWindowDirectory() => (string)(typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null));
        public static bool IsAvailableProjectWindowDirectory(string path) => AssetDatabase.LoadMainAssetAtPath(path) != null;
        public static void SetCurrentProjectWindowDirectory(string path) => Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
    }
}