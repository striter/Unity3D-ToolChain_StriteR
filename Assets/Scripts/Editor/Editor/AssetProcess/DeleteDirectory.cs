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
                UEAsset.DeleteDirectory(UEPath.PathRegex(path).AssetToFilePath());
            return true;
        }

    }
}