using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Extensions;
using UnityEditor.Extensions.EditorPath;
using UnityEditor.Extensions.TextureEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

namespace Examples.Rendering.Imposter
{
    [CreateAssetMenu(menuName = "Optimize/Imposter/Constructor", fileName = "ImposterConstructor_Default")]
    public class ImposterConstructor : ScriptableObject
    {
        public ImposterInput m_Input = ImposterInput.kDefault;
        public Shader m_Shader;
        public ImposterCameraHandle[] m_CameraHandles;
        
        private static List<KeyValuePair<Material, Shader>> m_SharedMaterialShaderRef = new();
        private static List<KeyValuePair<Renderer,int>> m_RendererLayerRef = new();
        private static int kLayerID = 30;
        private string GetDirectory() => UEAsset.MakeSureDirectory(UEPath.PathRegex("<#activeScenePath>/Imposter"));
        
        [Button]
        void GenerateSceneSelections()
        {
            var directory = GetDirectory();
            UEAsset.DeleteAllAssetAtPath(directory.FileToAssetPath());
            var index = 0;
            foreach (var obj in Selection.objects)
            {
                if (!obj.IsSceneObject())
                {
                    Debug.LogWarning($"{obj} is not SceneObject");
                    continue;
                }
                
                var name = UEPath.PathRegex($"<#activeSceneName>_{index++}_Imposter_{obj.name}");
                Construct((obj as GameObject).transform,name,$"{directory}/{name}.asset");
            }
        }
        
        public void Construct(Transform _sceneObjectRoot,string _initialName,string _filePath)
        {
            if (!_sceneObjectRoot.gameObject.IsSceneObject())
            {
                Debug.LogError($"{_sceneObjectRoot} is not SceneObject");
                return;
            }
            
            if (m_Shader == null)
            {
                Debug.LogError($"Invalid Renderer : {this}");
                return;
            }
            
            var meshRenderers = _sceneObjectRoot.GetComponentsInChildren<MeshRenderer>(false);
            if (meshRenderers.Length == 0)
            {
                Debug.LogError($"No MeshRenderer Found : {_sceneObjectRoot}");
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

            var material = new Material(m_Shader){name = _initialName};
            var block = new MaterialPropertyBlock();
            m_SharedMaterialShaderRef.Clear();
            m_RendererLayerRef.Clear();
            meshRenderers.Traversal(p=>
            {
                m_RendererLayerRef.Add(new (p,p.gameObject.layer));
                p.gameObject.layer = kLayerID;
                p.sharedMaterials.Traversal(p =>
                {
                    if (p == null || p.shader == null)
                        return;

                    m_SharedMaterialShaderRef.Add(new(p, p.shader));
                });
            });

            var boundingSphere = UGeometry.GetBoundingSphere(vertices);
            var boundingSphereExtrude = 0.05f;
            boundingSphere.radius += boundingSphereExtrude;
            
            block.SetVector(ImposterDefine.kBounding, (float4)boundingSphere);
            
            m_CameraHandles.Traversal(p=>p.Init(m_Input,boundingSphere));
            foreach (var corner in m_Input.GetImposterViewsNormalized())
            {
                m_CameraHandles.Traversal(handle=>
                {
                    if(handle.m_Shader != null)
                        m_SharedMaterialShaderRef.Traversal(p=>p.Key.shader = handle.m_Shader);
                    meshRenderers.Traversal(p=>p.SetPropertyBlock(block));
                    handle.Render(boundingSphere, corner.direction, corner.rect);
                });
            }
            m_CameraHandles.Traversal(p=>
            {
                var texture = p.Output(_initialName, _filePath);
                material.SetTexture(p.m_Name,texture);
            });
            
            m_RendererLayerRef.Traversal(p=>p.Key.gameObject.layer = p.Value);
            m_SharedMaterialShaderRef.Traversal(p=>p.Key.shader = p.Value);
            boundingSphere.radius += boundingSphereExtrude;
            boundingSphere.center = _sceneObjectRoot.transform.worldToLocalMatrix.MultiplyPoint(boundingSphere.center);
            
            var asset = ScriptableObject.CreateInstance<ImposterData>();
            asset.m_Input = m_Input;
            asset.m_BoundingSphere = boundingSphere;
            asset.m_Material = material;
            var assetPath = _filePath.FileToAssetPath();
            asset = UEAsset.CreateAssetCombination(assetPath,asset);
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
                m_Camera.allowMSAA = true;
                m_Camera.cullingMask = 1 << kLayerID;
                var additional = m_Camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                additional.renderPostProcessing = false;
            }

            public void Render(GSphere _sphere,float3 _direction,G2Box _rect)
            {
                var position = _sphere.GetSupportPoint(_direction * _sphere.radius);
                m_Camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(-_direction,Vector3.up));
                m_Camera.rect = _rect;
                m_Camera.Render();
            }

            public Texture2D Output(string initialName,string filePath)
            {
                RenderTexture.active = m_RenderTexture;
                var textureName = $"{initialName}{m_Name}";
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
                return AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath.FileToAssetPath());
            }
        }
    }
}
