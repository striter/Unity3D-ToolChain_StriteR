using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle.Process
{
    public class TransferDirectory : EAssetPipelineProcess
    {
        [EditorPath] public string m_SourcePath;
        [EditorPath] public string m_DestinationPath;
        public override void OnExecute()
        {
            var srcDirectory = UEPath.PathRegex(m_SourcePath);
            var dstDirectory = UEPath.PathRegex(m_DestinationPath);
            
            if (!AssetDatabase.IsValidFolder(dstDirectory))
                AssetDatabase.CreateFolder(dstDirectory.GetPathDirectory(), dstDirectory.GetPathName());
            
            AssetDatabase.MoveAsset(srcDirectory, dstDirectory);
            AssetDatabase.Refresh();
        }
    }
}