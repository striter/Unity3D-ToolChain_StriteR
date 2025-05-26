using System;
using UnityEditor.Extensions.ScriptableObjectBundle;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
{
    [Serializable]
    public abstract class AAssetProcess : AScriptableObjectBundleElement {
        public abstract bool Preprocess(AssetImporter _importer);
        public abstract bool Postprocess(AssetImporter _importer,UnityEngine.Object _object);
    }

    public abstract class AAssetProcess<Target,Importer> : AAssetProcess
        where Target : UnityEngine.Object
        where Importer : AssetImporter
    {
        public override bool Preprocess(AssetImporter _importer) => PreProcess(_importer as Importer);
        public override bool Postprocess(AssetImporter _importer, UnityEngine.Object _object) => Postprocess( _importer as Importer,_object as Target);
        protected abstract bool PreProcess(Importer _importer);
        protected abstract bool Postprocess(Importer _importer,Target _target);
    }
    public abstract class ATextureProcess : AAssetProcess<Texture2D, TextureImporter> { }
    
    public abstract class AModelProcess : AAssetProcess<GameObject,ModelImporter>
    {
        protected override bool Postprocess(ModelImporter _importer,GameObject _target)
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
    
    public abstract class AAnimationProcess : AAssetProcess<AnimationClip,ModelImporter> { }
}