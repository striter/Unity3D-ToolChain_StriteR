using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEditor.Extensions.EditorPath;
using UnityEditor.Extensions.TextureEditor;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle.Process
{
    [Serializable]
    public class ITextureCombineIEnumerable : IChannelCollector , IEnumerable<Texture2D>
    {
        public EChannelOperation operation;
        [Foldout(nameof(operation), EChannelOperation.Constant)] [Range(0, 1)] public float constantValue;
        [Fold(nameof(operation), EChannelOperation.Constant)] [EditorPath] public string texturesPath;
        public string filter;
        
        private Texture2D[] m_Textures;
        private int m_Index;
        public int Count => m_Textures?.Length ?? 0;
        public bool Init()
        {
            if (Operation == EChannelOperation.Constant)
                return true;
            var directory = UEPath.PathRegex(texturesPath) + "/";

            var textures = UEAsset.LoadAllAssetsAtDirectory<Texture2D>(directory);
            if(!string.IsNullOrEmpty(filter))
                textures = textures.Collect(p=>p.name.Contains(filter));
            m_Textures = textures.ToArray();
            if (m_Textures.Length == 0)
            {
                Debug.LogWarning("Invalid texture path: " + directory);
                return false;
            }
            
            m_Index = 0;
            return true;
        }

        public void Next() => m_Index++;
        
        public EChannelOperation Operation => operation;
        public Texture2D Texture => m_Textures?[m_Index];
        public float ConstantValue => constantValue;
        public Color[] PixelsResolved { get; set; }

        public IEnumerator<Texture2D> GetEnumerator()
        {
            if (Operation == EChannelOperation.Constant)
                yield break;
            for (var i = 0; i < Count; i++)
                yield return m_Textures[i];
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public class CombineTextures : EditorExecutableProcess , IEnumerable<ITextureCombineIEnumerable>
    {
        public ITextureCombineIEnumerable m_R, m_G, m_B, m_A;
        
        [Header("Export")]
        [EditorPath] public string m_OutputPath;
        public string m_NameIdentity;
        [Fold(nameof(m_NameIdentity),"")]public string m_NameReplacement;
        public ETextureExportType m_ExportType = ETextureExportType.JPG;

        bool SanityCheck(out ITextureCombineIEnumerable _textureCollector)
        {
            _textureCollector = null;
            if (this.Any(combine => !combine.Init()))
                return false;

            var textureInputs = this.Collect(p => p.Operation != EChannelOperation.Constant).ToArray();
            if (!textureInputs.Any())
            {
                Debug.LogWarning("No texture input found");
                return false;
            }

            foreach (var combiner in textureInputs)
            {
                foreach (var texture2D in combiner)
                {
                    var assetPath = AssetDatabase.GetAssetPath(texture2D);
                    var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if(textureImporter == null || textureImporter.isReadable)
                        continue;
                    textureImporter.isReadable = true;
                    AssetDatabase.ImportAsset(assetPath);
                }
            }
            
            _textureCollector = textureInputs.First();
            var count = _textureCollector.Count;
            if (textureInputs.Any(p => p.Count != count))
            {
                Debug.LogWarning("Texture count mismatch");
                return false;
            }

            return true;
        }

        public override bool Execute()
        {
            if (!SanityCheck(out var textureCollector))
                return false;
            
            var dstDirectory = UEPath.PathRegex(m_OutputPath);
            if (!AssetDatabase.IsValidFolder(dstDirectory))
                AssetDatabase.CreateFolder(dstDirectory.GetPathDirectory(), dstDirectory.GetPathName());

            var count = textureCollector.Count;
            for (var i = 0; i < count; i++)
            {
                var name = textureCollector.Texture.name;
                var newTexture = ChannelCombiner.Combine(m_R,m_G,m_B,m_A,TextureFormat.ARGB32);
                if (newTexture == null)
                {
                    Debug.LogWarning("Failed to combine texture: " + name);
                    return false;
                }

                if(!string.IsNullOrEmpty(m_NameIdentity))
                    name = name.Replace(m_NameIdentity, m_NameReplacement);
                
                var filePath = $"{dstDirectory.AssetToFilePath()}/{name}.{m_ExportType.GetExtension()}";
                UTextureExport.ExportTexture(newTexture, filePath,m_ExportType);
                var importer = AssetImporter.GetAtPath(filePath.FileToAssetPath()) as TextureImporter;
                if (importer != null)
                {
                    var setting = importer.GetDefaultPlatformTextureSettings();
                    setting.maxTextureSize = math.max( textureCollector.Texture.width, textureCollector.Texture.height);
                    importer.SetPlatformTextureSettings(setting);
                    importer.SaveAndReimport();
                }
                
                this.Traversal(p=>p.Next());
            }
            return true;
        }

        public IEnumerator<ITextureCombineIEnumerable> GetEnumerator()
        {
            yield return m_R;
            yield return m_G;
            yield return m_B;
            yield return m_A;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}