using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEditor.Extensions.EditorPath;
using UnityEditor.Extensions.TextureEditor;
using UnityEngine;

namespace Examples.Rendering.Imposter
{
    public class Imposter : MonoBehaviour
    {
        public ImposterInput m_Input = ImposterInput.kDefault;
        public ImposterCameraHandle[] m_CameraHandles;

        private List<KeyValuePair<Material, Shader>> m_SharedMaterialShaderRef = new();
        [Button]
        void Construct()
        {
            var meshRenderers = GetComponentsInChildren<MeshRenderer>(false);
            if (meshRenderers.Length == 0)
            {
                Debug.LogError("No MeshRenderer Found");
                return;
            }
            var initialName = $"Imposter_{meshRenderers.Select(p=>p.name).ToString('_')}";
            if (!UEAsset.SaveFilePath(out var filePath, "asset",initialName))
            {
                Debug.LogWarning("Invalid Folder Selected");
                return;
            }

            var vertices = new List<float3>();
            foreach (var renderer in meshRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (!meshFilter || meshFilter.sharedMesh == null)
                    continue;
                
                var matrix = renderer.transform.localToWorldMatrix;
                var intersectMesh = meshFilter.sharedMesh;
                vertices.AddRange(intersectMesh.vertices.Select(p=>(float3)matrix.MultiplyPoint(p)));
            }

            if (vertices.Count == 0)
                return;

            var block = new MaterialPropertyBlock();
            m_SharedMaterialShaderRef.Clear();
            meshRenderers.Traversal(p=>p.sharedMaterials.Traversal(p =>
            {
                if (p == null || p.shader == null)
                    return;

                m_SharedMaterialShaderRef.Add(new (p,p.shader));
            }));

            var boundingSphere = UGeometry.GetBoundingSphere(vertices);
            var boundingSphereExtrude = 0.05f;
            boundingSphere.radius += boundingSphereExtrude;
            
            block.SetVector(ImposterDefine.kBounding, (float4)boundingSphere);
            
            m_CameraHandles.Traversal(p=>p.Init(m_Input,boundingSphere));
            foreach (var (rect, direction) in m_Input.GetImposterViewsNormalized())
            {
                m_CameraHandles.Traversal(handle=>
                {
                    m_SharedMaterialShaderRef.Traversal(p=>p.Key.shader = handle.m_Shader);
                    meshRenderers.Traversal(p=>p.SetPropertyBlock(block));
                    handle.Render(boundingSphere, direction, rect);
                });
            }
            m_CameraHandles.Traversal(p=>p.Output(initialName, filePath));
            
            m_SharedMaterialShaderRef.Traversal(p=>p.Key.shader = p.Value);
            boundingSphere.radius += boundingSphereExtrude;
            boundingSphere.center = transform.worldToLocalMatrix.MultiplyPoint(boundingSphere.center);
            
            var asset = ScriptableObject.CreateInstance<ImposterData>();
            asset.m_Input = m_Input;
            asset.m_BoundingSphere = boundingSphere;
            UEAsset.CreateOrReplaceMainAsset(asset,filePath.FileToAssetPath());
        }

        [Serializable]
        public class ImposterCameraHandle
        {
            public string m_Name;
            public Shader m_Shader;
            private Camera m_Camera;
            private RenderTexture m_RenderTexture;
            
            public void Init(ImposterInput _input, GSphere _sphere)
            { 
                m_Camera = new GameObject($"Camera_{m_Name}").AddComponent<Camera>();
                m_RenderTexture = RenderTexture.GetTemporary(_input.TextureResolution.x,_input.TextureResolution.y, 0, RenderTextureFormat.ARGB32);
                _sphere.radius += 0.05f;
                RenderTexture.active = m_RenderTexture;
                GL.Clear(true,true,Color.clear);

                m_Camera.orthographic = true;
                m_Camera.clearFlags = CameraClearFlags.Depth;
                m_Camera.backgroundColor = Color.black.SetA(0f);
                m_Camera.nearClipPlane = 0.01f;
                m_Camera.orthographicSize = _sphere.radius;
                m_Camera.targetTexture = m_RenderTexture;
                m_Camera.farClipPlane = _sphere.radius * 2;
            }

            public void Render(GSphere _sphere,float3 _direction,G2Box _rect)
            {
                var index = 0;
                
                var position = _sphere.GetSupportPoint(_direction * _sphere.radius);
                m_Camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(-_direction,Vector3.up));
                m_Camera.rect = _rect;
                m_Camera.Render();
            }

            public void Output(string initialName,string filePath)
            {
                RenderTexture.active = m_RenderTexture;
                var textureName = $"{initialName}_{m_Name}";
                var texture = new Texture2D(m_RenderTexture.width, m_RenderTexture.height,TextureFormat.ARGB32,false){name = textureName};
                texture.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
                texture.Apply();
                
                var encoding = ETextureExportType.PNG;
                var texturePath = filePath.Replace(initialName, textureName).Replace("asset", encoding.GetExtension());
                UTextureEditor.ExportTexture(texture,texturePath,encoding);
                
                RenderTexture.ReleaseTemporary(m_RenderTexture);
                RenderTexture.active = null;
                m_Camera.targetTexture = null;
                GameObject.DestroyImmediate(m_Camera.gameObject);
            }
            
        }
        
        public bool m_DrawGizmos;
        private void OnDrawGizmos()
        {
            if (!m_DrawGizmos)
                return;
            Gizmos.matrix = transform.localToWorldMatrix;   
            m_Input.DrawGizmos();
        }
    }
}
