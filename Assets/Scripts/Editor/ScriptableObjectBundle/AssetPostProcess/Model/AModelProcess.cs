using UnityEditor.Extensions.ScriptableObjectBundle;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline.Model
{
    public abstract class AModelProcess : AssetPostProcessRule
    {
        public void Process(GameObject _gameObject)
        {
            var meshFilters = _gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
                ProcessMesh(meshFilter.sharedMesh);
            var skinnedRenderers = _gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedRenderer in skinnedRenderers)
                ProcessMesh(skinnedRenderer.sharedMesh);
        }

        protected abstract void ProcessMesh(Mesh _mesh);
    }

}