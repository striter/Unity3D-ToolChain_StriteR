using System;
using System.IO;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle.Process
{
    public class DeleteDirectory : EAssetPipelineProcess
    {
        [EditorPath] public string[] m_DeletePaths;
        public override bool Execute()
        {
            foreach (var path in m_DeletePaths)
            {
                var directoryPath = UEPath.PathRegex(path).AssetToFilePath();
                Execute(directoryPath);
            }
            AssetDatabase.Refresh();
            return true;
        }

        public static void Execute(string _filePath)
        {
            if (!Directory.Exists(_filePath))
                return;
            Directory.Delete(_filePath, true);
            var metaFilePath = _filePath + ".meta";
            if (File.Exists(metaFilePath))
                File.Delete(metaFilePath);
        }

    }
}