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
                if (!Directory.Exists(directoryPath)) continue;
                Directory.Delete(directoryPath, true);
                var metaFilePath = directoryPath + ".meta";
                if (File.Exists(metaFilePath))
                    File.Delete(metaFilePath);
            }
            AssetDatabase.Refresh();
            return true;
        }
    }
}