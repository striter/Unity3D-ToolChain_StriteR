using System;
using System.Collections.Generic;
using UnityEditor.Extensions.EditorExecutable;
using UnityEngine;

namespace UnityEditor.Extensions.AssetProcess
{
    [Serializable]
    public abstract class AAssetProcess : AScriptableObjectBundleElement {
        public abstract bool Preprocess(AssetImporter _importer);
        public abstract bool Postprocess(UnityEngine.Object _object);
        public abstract string[] kRelativeBackends { get; }
    }

    public abstract class AAssetProcess<Target,Importer> : AAssetProcess
        where Target : UnityEngine.Object
        where Importer : AssetImporter
    {
        public override bool Preprocess(AssetImporter _importer) => Preprocess(_importer as Importer);
        public override bool Postprocess(UnityEngine.Object _object) => Postprocess( _object as Target);
        protected abstract bool Preprocess(Importer _importer);
        protected abstract bool Postprocess(Target _target);
    }

    public abstract class ATextureProcess : AAssetProcess<Texture2D, TextureImporter>
    {
        public override string[] kRelativeBackends => new[] { ".png", ".jpg", ".jpeg", ".tga" };
    } 
    
    public abstract class AModelProcess : AAssetProcess<GameObject,ModelImporter>
    {
        public override string[] kRelativeBackends => new[] { ".fbx" , ".obj"};
        protected override bool Postprocess(GameObject _target)
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

    public abstract class AAnimationProcess : AAssetProcess<AnimationClip, ModelImporter>
    {
        public override string[] kRelativeBackends => new[] { ".fbx", ".obj" };
    }
}