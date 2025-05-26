using System;
using UnityEditor.Extensions.ScriptableObjectBundle;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
{
    [Serializable]
    public abstract class AAssetProcess : AScriptableObjectBundleElement {
        public abstract bool Preprocess(AssetImporter _importer);
        public abstract bool PostProcess(UnityEngine.Object _object);
    }

    public abstract class AAssetProcess<Target,Importer> : AAssetProcess
        where Target : UnityEngine.Object
        where Importer : AssetImporter
    {
        public override bool Preprocess(AssetImporter _importer) => Preprocess(_importer as Importer);
        public override bool PostProcess(UnityEngine.Object _object) => PostProcess(_object as Target);
        protected abstract bool Preprocess(Importer _importer);
        protected abstract bool PostProcess(Target _target);
    }
    public abstract class ATextureProcess : AAssetProcess<Texture2D, TextureImporter> { }
    
    public abstract class AModelProcess : AAssetProcess<GameObject,ModelImporter>
    {
        protected override bool PostProcess(UnityEngine.GameObject _target)
        {
            var flag = false;
            var meshFilters = _target.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
                flag |= PostProcess(meshFilter.sharedMesh);
            var skinnedRenderers = _target.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedRenderer in skinnedRenderers)
                flag |= PostProcess(skinnedRenderer.sharedMesh);
            return flag;
        }

        protected abstract bool PostProcess(Mesh _mesh);
    }
}