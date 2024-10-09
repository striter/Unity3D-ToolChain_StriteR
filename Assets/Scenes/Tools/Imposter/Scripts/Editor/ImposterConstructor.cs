using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Runtime.Optimize.Imposter
{
    using static ImposterDefine;
    public enum EContourDownSample
    {
        _1 = 1,
        _2 = 2,
        _4 = 4,
        _8 = 8
    }
    [CreateAssetMenu(menuName = "Optimize/Imposter/Constructor", fileName = "ImposterConstructor_Default")]
    public class ImposterConstructor : ScriptableObject
    {
        public ImposterInput m_Input = ImposterInput.kDefault;
        public Shader m_Shader;
        public bool m_Instanced = true;

        public EContourDownSample m_ContourDownSample = EContourDownSample._1;
        
        public ImposterCameraHandle[] m_CameraHandles;

        [Header("Debug")]
        public bool m_AlphaTexture = false;
        public bool m_ContourMeshTexture = false;
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
            {
                Debug.LogError($"No Vertices Found : {_sceneObjectRoot}");
                return null;
            }

            var material = new Material(m_Shader){name = _initialName,enableInstancing = m_Instanced};
            var mesh = new Mesh(){name = _initialName};
            var block = new MaterialPropertyBlock();
            var boundingSphere = UGeometry.GetBoundingSphere(vertices);
            m_SharedMaterialShaderRef.Clear();
            m_RendererLayerRef.Clear();
            try
            {
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

                var boundingSphereExtrude = 0.05f;
                boundingSphere.radius += boundingSphereExtrude;
                
                block.SetVector(ImposterShaderProperties.kBoundingID, (float4)boundingSphere);
                meshRenderers.Traversal(p=>p.SetPropertyBlock(block));

                
                var alphaHandle = new ImposterCameraHandle() {m_Name = "_AlphaMask", m_Shader = null};
                var contourMeshHandle = new ImposterCameraHandle() {m_Name = "_ContourMesh", m_Shader = Shader.Find("Hidden/Imposter_ContourShape") };
                alphaHandle.Init(m_Input.TextureResolution,boundingSphere);
                contourMeshHandle.Init(m_Input.TextureResolution / (int)m_ContourDownSample,boundingSphere,false);
                
                m_CameraHandles.Traversal(p=>p.Init(m_Input.TextureResolution,boundingSphere));
                foreach (var corner in m_Input.GetImposterViewsNormalized())
                {
                    alphaHandle.Render(m_SharedMaterialShaderRef,boundingSphere, corner.direction, corner.uvRect);
                    m_CameraHandles.Traversal(handle => handle.Render(m_SharedMaterialShaderRef,boundingSphere, corner.direction, corner.uvRect));
                    contourMeshHandle.Render(m_SharedMaterialShaderRef,boundingSphere, corner.direction, G2Box.kOne);
                }

                var dilateMaterial = new Material(Shader.Find("Hidden/Imposter_Dilate")){hideFlags = HideFlags.HideAndDontSave};
                dilateMaterial.SetTexture("_MaskTex",alphaHandle.m_RenderTexture);

                m_CameraHandles.Traversal(p =>
                {
                    material.SetTexture(p.m_Name, p.OutputAsset(m_Input,_initialName, _filePath,alphaHandle.m_RenderTexture,dilateMaterial));
                });

                GameObject.DestroyImmediate(dilateMaterial);
                if(m_AlphaTexture)
                    alphaHandle.OutputAsset(m_Input,_initialName,_filePath,null,null);
                else
                    alphaHandle.Dispose();
                
                var contourPolygon = G2Polygon.kDefault;
                if (m_ContourMeshTexture)
                {
                    contourMeshHandle.OutputAsset(m_Input,_initialName,_filePath,null,null);
                }
                else
                {    
                    var contourPixels = contourMeshHandle.OutputPixels(out var resolution);
                    var contourOutline = ContourTracingData.FromColor(resolution.x, contourPixels, p=> p.to4().maxElement() > 0.01f).RadialSweep();
                    var contourPolygonPositions = UGeometry.GetBoundingPolygon(contourOutline);
                    contourPolygonPositions = CartographicGeneralization.VisvalingamWhyatt(contourPolygonPositions.Remake(p=>p/resolution),math.min(contourPolygonPositions.Count ,10),true);
                    contourPolygon = new G2Polygon(contourPolygonPositions);
                }
                
                boundingSphere.radius -= boundingSphereExtrude;
                boundingSphere.center -= (float3)_sceneObjectRoot.position;
                mesh.SetVertices(contourPolygon.Select(p=>(Vector3)((p-.5f).to3xy() * boundingSphere.radius * 2 + boundingSphere.center)).ToList());
                mesh.SetIndices(contourPolygon.GetIndexes().ToArray(),MeshTopology.Triangles,0);
                mesh.SetUVs(0,contourPolygon.Select(p=>(Vector2)p).ToArray());
                mesh.bounds = boundingSphere.GetBoundingBox();

                material.SetVector(ImposterShaderProperties.kTexelID,m_Input.GetImposterTexel());
                material.SetVector(ImposterShaderProperties.kBoundingID, (float4)boundingSphere);
                material.SetInt(ImposterShaderProperties.kModeID,(int)m_Input.mapping);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
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
            public bool m_DilateAlpha;
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
                m_Camera.nearClipPlane = 0.0001f;
                m_Camera.orthographicSize = _sphere.radius;
                m_Camera.targetTexture = m_RenderTexture;
                m_Camera.farClipPlane = _sphere.radius * 2;
                m_Camera.aspect = 1;
                m_Camera.allowMSAA = true;
                m_Camera.cullingMask = 1 << kLayerID;
                var additional = m_Camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                additional.renderPostProcessing = false;
                additional.SetRenderer(kRendererIndex);
            }

            public void Render(List<KeyValuePair<Material, Shader>> _materialRef, GSphere _sphere, float3 _direction, G2Box _rect)
            {
                _materialRef.Traversal(p => p.Key.shader = (m_Shader == null ? p.Value : m_Shader));
                var position = _sphere.GetSupportPoint(_direction * _sphere.radius);
                m_Camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(-_direction, Vector3.up));
                m_Camera.rect = _rect;
                m_Camera.Render();
            }

            public Color[] OutputPixels(out int2 resolution)
            {
                RenderTexture.active = m_RenderTexture;
                var texture2D = new Texture2D(m_RenderTexture.width, m_RenderTexture.height, TextureFormat.ARGB32, false){filterMode = FilterMode.Bilinear};
                texture2D.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
                texture2D.Apply();

                resolution = new int2(m_RenderTexture.width, m_RenderTexture.height);
                var pixels = texture2D.GetPixels();
                
                GameObject.DestroyImmediate(texture2D);
                Dispose();
                return pixels;
            }

            public Texture2D OutputAsset(ImposterInput _input,string initialName, string filePath,RenderTexture dilateTex,Material dilateMat)
            {
                var textureName = $"{initialName}{m_Name}";
                var texture2D = new Texture2D(m_RenderTexture.width, m_RenderTexture.height, TextureFormat.ARGB32, false) { name = textureName };

                if (dilateMat != null)
                {
                    RenderTexture tempTex = RenderTexture.GetTemporary( m_RenderTexture.width, m_RenderTexture.height, m_RenderTexture.depth, m_RenderTexture.format );
                    RenderTexture tempMask = RenderTexture.GetTemporary( m_RenderTexture.width, m_RenderTexture.height, m_RenderTexture.depth, m_RenderTexture.format );
                    RenderTexture dilatedMask = RenderTexture.GetTemporary( m_RenderTexture.width, m_RenderTexture.height, m_RenderTexture.depth, m_RenderTexture.format );
                    Graphics.Blit(dilateTex,dilatedMask);
                    for( int i = 0; i < _input.cellResolution / 4; i++ )
                    {
                        dilateMat.SetTexture( "_MaskTex", dilatedMask );

                        Graphics.Blit( m_RenderTexture, tempTex, dilateMat, m_DilateAlpha ? 1 : 0 );
                        Graphics.Blit( tempTex, m_RenderTexture );

                        Graphics.Blit( dilatedMask, tempMask, dilateMat, 1 );
                        Graphics.Blit( tempMask, dilatedMask );
                    }
                    RenderTexture.ReleaseTemporary( tempTex );
                    RenderTexture.ReleaseTemporary( tempMask );
                    RenderTexture.ReleaseTemporary( dilatedMask );
                }
                
                RenderTexture.active = m_RenderTexture;
                texture2D.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
                texture2D.Apply();
                
                var encoding = ETextureExportType.PNG;
                var texturePath = filePath.Replace(initialName, textureName).Replace("asset", encoding.GetExtension());
                var textureAsset = UTextureExport.ExportTexture(texture2D, texturePath, encoding);

                GameObject.DestroyImmediate(texture2D);
                Dispose();
                return textureAsset;
            }

            public void Dispose()
            {
                RenderTexture.ReleaseTemporary(m_RenderTexture);
                RenderTexture.active = null;
                m_Camera.targetTexture = null;
                GameObject.DestroyImmediate(m_Camera.gameObject);
            }
        }
    }
     
}
