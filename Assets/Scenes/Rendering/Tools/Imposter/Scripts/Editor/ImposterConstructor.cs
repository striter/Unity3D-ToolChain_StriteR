using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEditor.Extensions.EditorPath;
using UnityEditor.Extensions.TextureEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Examples.Rendering.Imposter
{
    [CreateAssetMenu(menuName = "Optimize/Imposter/Constructor", fileName = "ImposterConstructor_Default")]
    public class ImposterConstructor : ScriptableObject
    {
        public ImposterInput m_Input = ImposterInput.kDefault;
        public Shader m_Shader;
        public bool m_Instanced;

        public ImposterCameraHandle[] m_CameraHandles;
        private static int kLayerID = 30;
        private static List<KeyValuePair<Material, Shader>> m_SharedMaterialShaderRef = new();
        private static List<KeyValuePair<Renderer,int>> m_RendererLayerRef = new();
        public ImposterData Construct(Transform _sceneObjectRoot,string _initialName,string _filePath)
        {
            if (!_sceneObjectRoot.gameObject.IsSceneObject())
            {
                Debug.LogError($"{_sceneObjectRoot} is not SceneObject");
                return null;
            }
            
            if (m_Shader == null)
            {
                Debug.LogError($"Invalid Constructor Renderer : {this}");
                return null;
            }
            
            var meshRenderers = _sceneObjectRoot.GetComponentsInChildren<Renderer>(false);
            if (meshRenderers.Length == 0)
            {
                Debug.LogError($"No Renderer Found : {_sceneObjectRoot}");
                return null;
            }

            var vertices = new List<float3>();
            foreach (var renderer in meshRenderers)
            {
                Mesh sharedMesh = null;
                if (renderer is SkinnedMeshRenderer skinned)
                    sharedMesh = skinned.sharedMesh;
                else
                {
                    var meshFilter = renderer.GetComponent<MeshFilter>();
                    if(meshFilter != null)
                        sharedMesh = meshFilter.sharedMesh;
                }
                
                if(sharedMesh == null)
                    continue;
                
                var matrix = renderer.transform.localToWorldMatrix;
                vertices.AddRange(sharedMesh.vertices.Select(p=>(float3)matrix.MultiplyPoint(p)));
            }

            if (vertices.Count == 0)
                return null;

            var material = new Material(m_Shader){name = _initialName,enableInstancing = m_Instanced};
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
            
            block.SetVector(ImposterShaderProperties.kBoundingID, (float4)boundingSphere);
            meshRenderers.Traversal(p=>p.SetPropertyBlock(block));

            m_CameraHandles.Traversal(p=>p.Init(m_Input.TextureResolution,boundingSphere));
            foreach (var corner in m_Input.GetImposterViewsNormalized())
                m_CameraHandles.Traversal(handle => handle.Render(m_SharedMaterialShaderRef,boundingSphere, corner.direction, corner.uvRect));
            m_CameraHandles.Traversal(p=> { material.SetTexture(p.m_Name,p.OutputAsset(_initialName, _filePath)); });
            
            var mesh = new Mesh(){name = _initialName};
            var contourMeshHandle = new ImposterCameraHandle() {m_Name = "_ContourMesh", m_Shader = Shader.Find("Hidden/Imposter_ContourShape") };
            contourMeshHandle.Init(m_Input.cellResolution,boundingSphere,false);
            foreach (var corner in m_Input.GetImposterViewsNormalized())
                contourMeshHandle.Render(m_SharedMaterialShaderRef,boundingSphere, corner.direction, G2Box.kOne);

            var contourPolygon = G2Polygon.kDefault;
            var contourPixels = contourMeshHandle.OutputPixels(out var resolution);
            var contourOutline = ContourTracing.FromColorAlpha(resolution.x, contourPixels, 0.5f).MooreNeighborTracing();
            contourPolygon = UGeometry.GetBoundingPolygon(contourOutline.Select(p=>(float2)p).ToList());
            contourPolygon = new G2Polygon(CartographicGeneralization.VisvalingamWhyatt(contourPolygon.positions.Select(p=>p/resolution).ToList(),math.min(contourPolygon.positions.Length ,10),true));
        
            boundingSphere.radius += boundingSphereExtrude;
            boundingSphere.center -= (float3)_sceneObjectRoot.position;

            mesh.SetVertices(contourPolygon.Select(p=>(Vector3)((p-.5f).to3xy()) * boundingSphere.radius * 2).ToList());
            mesh.SetIndices(contourPolygon.GetIndexes().ToArray(),MeshTopology.Triangles,0);
            mesh.SetUVs(0,contourPolygon.Select(p=>(Vector2)p).ToArray());
            mesh.bounds = boundingSphere.GetBoundingBox();

            material.SetVector(ImposterShaderProperties.kTexelID,m_Input.GetImposterTexel());
            material.SetVector(ImposterShaderProperties.kBoundingID, (float4)boundingSphere);
            material.SetInt(ImposterShaderProperties.kModeID,(int)m_Input.mapping);

            m_RendererLayerRef.Traversal(p=>p.Key.gameObject.layer = p.Value);
            m_SharedMaterialShaderRef.Traversal(p=>p.Key.shader = p.Value);
            
            var asset = ScriptableObject.CreateInstance<ImposterData>();
            asset.m_Input = m_Input;
            asset.m_BoundingSphere = boundingSphere;
            asset.m_Material = material;
            asset.m_Instanced = m_Instanced;
            asset.m_Mesh = mesh;
            var assetPath = _filePath.FileToAssetPath();
            return UEAsset.CreateAssetCombination(assetPath,asset);
        }

        [Serializable]
        public class ImposterCameraHandle
        {
            public string m_Name;
            public Shader m_Shader;
            private Camera m_Camera;
            public RenderTexture m_RenderTexture { get; private set; }

            public void Init(int2 _resolution, GSphere _sphere, bool _clear = true)
            {
                m_Camera = new GameObject($"Camera_{m_Name}").AddComponent<Camera>();
                m_RenderTexture = RenderTexture.GetTemporary(_resolution.x, _resolution.y, 0, RenderTextureFormat.ARGB32);

                _sphere.radius += 0.05f;
                RenderTexture.active = m_RenderTexture;
                GL.Clear(true, true, Color.clear);

                m_Camera.orthographic = true;
                m_Camera.clearFlags = _clear ? CameraClearFlags.Depth : CameraClearFlags.Nothing;
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

            public void Render(List<KeyValuePair<Material, Shader>> _materialRef, GSphere _sphere, float3 _direction,
                G2Box _rect)
            {
                if (m_Shader != null)
                    _materialRef.Traversal(p => p.Key.shader = m_Shader);

                var position = _sphere.GetSupportPoint(_direction * _sphere.radius);
                m_Camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(-_direction, Vector3.up));
                m_Camera.rect = _rect;
                m_Camera.Render();
            }


            public Color[] OutputPixels(out int2 resolution)
            {
                RenderTexture.active = m_RenderTexture;
                var texture2D = new Texture2D(m_RenderTexture.width, m_RenderTexture.height, TextureFormat.ARGB32, false);
                texture2D.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
                texture2D.Apply();

                resolution = new int2(m_RenderTexture.width, m_RenderTexture.height);
                var pixels = texture2D.GetPixels();

                Dispose(texture2D);
                return pixels;
            }

            public Texture2D OutputAsset(string initialName, string filePath)
            {
                RenderTexture.active = m_RenderTexture;
                var textureName = $"{initialName}{m_Name}";
                var texture2D =
                    new Texture2D(m_RenderTexture.width, m_RenderTexture.height, TextureFormat.ARGB32, false)
                        { name = textureName };
                texture2D.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
                texture2D.Apply();

                var encoding = ETextureExportType.PNG;
                var texturePath = filePath.Replace(initialName, textureName).Replace("asset", encoding.GetExtension());
                var textureAsset = UTextureEditor.ExportTexture(texture2D, texturePath, encoding);

                Dispose(texture2D);
                return textureAsset;
            }

            void Dispose(Texture2D _texture2D)
            {
                RenderTexture.ReleaseTemporary(m_RenderTexture);
                RenderTexture.active = null;
                m_Camera.targetTexture = null;
                GameObject.DestroyImmediate(m_Camera.gameObject);
                GameObject.DestroyImmediate(_texture2D);
            }
        }
    }
     
}
