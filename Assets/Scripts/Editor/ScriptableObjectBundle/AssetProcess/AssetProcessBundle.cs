using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Extensions;
using Runtime.Pool;
using UnityEditor.Extensions.EditorPath;
using UnityEditor.Extensions.ScriptableObjectBundle;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
{
    [Serializable, CreateAssetMenu(fileName = "AssetProcess", menuName = "AssetProcess/Bundle", order = 0)]
    public class AssetProcessBundle : AScriptableObjectBundle
    {
        public bool m_Enable = true;
        public override Type GetBaseType() => typeof(AAssetProcess);

        public virtual void RefreshAssets()
        {
            var backends = PoolList<string>.Empty($"{nameof(RefreshAssets)}_Backends");
            foreach (var process in m_Objects.CollectAs<ScriptableObject, AAssetProcess>())
                backends.TryAddRange(process.kRelativeBackends);

            var folderPath = AssetDatabase.GetAssetPath(this).GetPathDirectory();
            var dirtyAssetPaths = PoolList<string>.Empty(nameof(RefreshAssets));

            foreach(var backend in backends)
            {
                var assetPaths = Directory.GetFiles(folderPath,$"*{backend}",SearchOption.AllDirectories);
                dirtyAssetPaths.AddRange(assetPaths);
            }

            foreach (var assetPath in dirtyAssetPaths)
                AssetDatabase.ImportAsset(assetPath);
        }
    }
}