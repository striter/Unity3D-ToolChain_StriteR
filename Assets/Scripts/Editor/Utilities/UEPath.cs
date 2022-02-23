using System;
using UnityEngine;

namespace TEditor
{
    public static class UEPath
    {
        public static string FileToAssetPath(string assetPath)
        {
            int assetIndex = assetPath.IndexOf("/Assets", StringComparison.Ordinal) + 1;
            if (assetIndex != 0)
                assetPath = assetPath.Substring(assetIndex, assetPath.Length - assetIndex);
            return assetPath;
        }

        public static string AssetToFilePath(string assetPath)
        {
            assetPath=assetPath.Substring(6, assetPath.Length-6);
            return Application.dataPath + assetPath;
        }
        public static string RemoveExtension(string path)
        {
            int extensionIndex = path.LastIndexOf('.');
            if (extensionIndex >= 0)
                return path.Remove(extensionIndex);
            return path;
        }
        public static string GetPathName(string path)
        {
            path = RemoveExtension(path);
            int folderIndex = path.LastIndexOf('/');
            if (folderIndex >= 0)
                path = path.Substring(folderIndex + 1, path.Length - folderIndex - 1);
            return path;
        }

        public static string GetFileName(string path)
        {
            int folderIndex = path.LastIndexOf('/');
            if (folderIndex >= 0)
                path = path.Substring(folderIndex + 1, path.Length - folderIndex - 1);
            return path;
        }
    }
}