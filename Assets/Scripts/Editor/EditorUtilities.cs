using System.IO;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public static class EditorUtilities
    {
        public static bool RenameAssets(string _folder,string _comparer,string _replace)
        {
            int count = 0;
            foreach (var assetPath in System.IO.Directory.GetFiles(UEPath.AssetToFilePath(_folder)))
            {
                count++;
                string assetName = Path.GetFileName(assetPath);
                File.Move(assetPath,assetPath.Replace(assetName,assetName.Replace(_comparer,_replace)));
            }
            AssetDatabase.Refresh();
            Debug.Log($"{count} Assets Renamed");
            return true;
        }
    }
}