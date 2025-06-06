﻿using System;
using System.IO;
using System.Linq.Extensions;
using Runtime.Pool;
using UnityEditor.Extensions.EditorPath;
using UnityEditor.Extensions.EditorExecutable;
using UnityEngine;

namespace UnityEditor.Extensions.AssetProcess
{
    [Serializable, CreateAssetMenu(fileName = "AssetProcess", menuName = "AssetProcess/Bundle", order = 0)]
    public class AssetProcessBundle : AScriptableObjectBundle
    {
        public bool m_Enable = true;
        public override Type GetBaseType() => typeof(AAssetProcess);

        public virtual void ManualRefreshAssets()
        {
            var backends = PoolList<string>.Empty($"{nameof(ManualRefreshAssets)}_Backends");
            foreach (var process in m_Objects.CollectAs<ScriptableObject, AAssetProcess>())
                backends.TryAddRange(process.kRelativeBackends);

            var folderPath = AssetDatabase.GetAssetPath(this).GetPathDirectory();
            var dirtyAssetPaths = PoolList<string>.Empty($"{nameof(ManualRefreshAssets)}_DirtyAssets");

            var allFiles = Directory.GetFiles(folderPath,$"*.*",SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                var ext = file.GetExtension().ToLower();
                if(backends.Contains(ext))
                    dirtyAssetPaths.Add(file);
            }

            Debug.Log($"Refreshed {dirtyAssetPaths.Count} assets from {folderPath}");
            foreach (var assetPath in dirtyAssetPaths)
                AssetDatabase.ImportAsset(assetPath);
        }
    }
}