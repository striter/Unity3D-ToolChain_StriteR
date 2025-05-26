using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions.EditorExecutable.Process
{
    public class TransferDirectory : EditorExecutableProcess
    {
        [EditorPath] public string m_SourcePath;
        [EditorPath] public string m_DestinationPath;
        public override bool Execute()
        {
            var srcDirectory = UEPath.PathRegex(m_SourcePath);
            var dstDirectory = UEPath.PathRegex(m_DestinationPath);
            
            if (!AssetDatabase.IsValidFolder(dstDirectory))
                AssetDatabase.CreateFolder(dstDirectory.GetPathDirectory(), dstDirectory.GetPathName());
            
            AssetDatabase.MoveAsset(srcDirectory, dstDirectory);
            AssetDatabase.Refresh();
            return true;
        }
    }
}