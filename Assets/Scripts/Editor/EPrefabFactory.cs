using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.Extensions
{
    public interface IPrefabFactoryAsset<T> where T:UnityEngine.Object
    {
        public T m_Asset { get; }
    }
    
    public class PrefabFactoryLoadAsset<T>:IPrefabFactoryAsset<T> where T:UnityEngine.Object
    {
        public T m_Asset { get; }
        public string AssetPath { get; }

        public PrefabFactoryLoadAsset(string _assetPath)
        {
            AssetPath = _assetPath;
            m_Asset = AssetDatabase.LoadAssetAtPath<T>(_assetPath);
            if (m_Asset == null)
                throw new Exception($"Invalid Asset<{typeof(T)}> at Path:\n{_assetPath}");
        }
        public void Import()
        {
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
        }
    }
    
    public class APrefabFactoryLoadOrCreateAsset<T>:IPrefabFactoryAsset<T> where T : UnityEngine.Object
    {       
        public T m_Asset { get; }
        public APrefabFactoryLoadOrCreateAsset(string _assetPath,Func<T> _createAsset) 
        {
            m_Asset = AssetDatabase.LoadAssetAtPath<T>(_assetPath);
            if(m_Asset == null)
                m_Asset = UEAsset.CreateOrReplaceMainAsset(_createAsset(), _assetPath);
        }
    }

    public class APrefabFactoryCreateAndReplaceAsset<T>:IPrefabFactoryAsset<T> where T : Object
    {       
        public T m_Asset { get; }

        public APrefabFactoryCreateAndReplaceAsset(string _assetPath,ref T _replaceAsset)
        {
            m_Asset = UEAsset.CreateOrReplaceMainAsset(_replaceAsset, _assetPath);
            _replaceAsset = m_Asset;
        }
    }
    
    public class FPrefabFactoryModelAsset : PrefabFactoryLoadAsset<GameObject>
    {
        public ModelImporter m_Importer { get; }
        public FPrefabFactoryModelAsset(string _assetPath) : base(_assetPath)
        {
            m_Importer = AssetImporter.GetAtPath(_assetPath) as ModelImporter;
        }
    }

    public class FPrefabFactoryTextureAsset : PrefabFactoryLoadAsset<Texture>
    {
        public TextureImporter m_Importer { get; }

        public FPrefabFactoryTextureAsset(string _assetPath) : base(_assetPath)
        {
            m_Importer = AssetImporter.GetAtPath(_assetPath) as TextureImporter;
        }
    }
    
}