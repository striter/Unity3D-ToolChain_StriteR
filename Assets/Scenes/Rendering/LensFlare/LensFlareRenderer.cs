using System;
using System.Linq;
using System.Linq.Extensions;
using Runtime;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Examples.Rendering.LensFlare
{
    [RequireComponent(typeof(MeshRenderer))]
    public class LensFlareRenderer : ARendererBase , IRendererViewSpace
    {
        public bool ViewSpaceRequired => true;
        public GSphere m_Occlusion = GSphere.kDefault;
        
        [ScriptableObjectEdit] public LensFlareAsset m_Asset;
        private MaterialPropertyBlock m_Block;

        private static readonly Vector4[] kOcclusionSample = ULowDiscrepancySequences.Sobol2D(32u).Remake(UCoordinates.Polar.ToCartesian_ShirleyChiu).Select(p=> (Vector4)p.to4()).ToArray();
        private Material[] m_Materials;
        
        private GSphere GetOcclusionSphereWS() => m_Occlusion + transform.position;
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_Block = null;

            if (m_Materials != null)
            {
                m_Materials.Traversal(CoreUtils.Destroy);
                m_Materials = null;
            }
        }

        private static Shader kShader;
        protected override void PopulateMesh(Mesh _mesh, Camera _viewCamera)
        {
            kShader ??= Shader.Find("Hidden/LensFlare");
            if (m_Asset == null)
                return;

            if (m_Asset.m_CellsData.Count == 0)
                return;

            var frustum = new GFrustum(_viewCamera);
            var occlusionSphere = GetOcclusionSphereWS();
            if (!frustum.Intersect(GetOcclusionSphereWS().GetBoundingBox()))
                return;

            var worldToScreen = _viewCamera.worldToCameraMatrix;
            var positionCS = math.mul(worldToScreen, transform.position.ToVector4()).xyz;
            positionCS = _viewCamera.WorldToViewportPoint(transform.position);

            var renderer = GetComponent<MeshRenderer>();
            if (renderer)
            {
                m_Block ??= new();
                m_Block.Clear();
                m_Block.SetInt("_OcclusionSampleCount",kOcclusionSample.Length);
                m_Block.SetVectorArray("_OcclusionSample", kOcclusionSample);
                m_Block.SetVector("_OcclusionSphere",(float4)occlusionSphere);
                renderer.SetPropertyBlock(m_Block);
            }

            var worldToLocalMatrix = transform.worldToLocalMatrix;
            var vertices = PoolList<Vector3>.ISpawn();
            var uvs = PoolList<Vector2>.ISpawn();
            var indexes = PoolList<int>.ISpawn();

            m_Materials ??= Array.Empty<Material>();
            m_Materials = m_Materials.Resize(m_Asset.m_CellsData.Count,() => CoreUtils.CreateEngineMaterial(kShader),CoreUtils.Destroy);

            foreach (var (index,cell) in m_Asset.m_CellsData.WithIndex())
            {
                var data = cell.data;
                var quadCenterCS = positionCS;
                quadCenterCS.xy = math.lerp(quadCenterCS.xy, (1 - quadCenterCS.xy) , data.offset); 
                var cellCenterWS = (float3)_viewCamera.ViewportToWorldPoint(quadCenterCS);

                var size = data.size;
                var startIndex = vertices.Count;
                var axis = new GCoordinates(cellCenterWS,_viewCamera.transform.right,_viewCamera.transform.up);
                vertices.AddRange(G2Quad.kDefault.Select(p=>worldToLocalMatrix.MultiplyPoint(axis.GetPoint(p * size))));
                uvs.AddRange(G2Quad.kDefaultUV.Select(p=>(Vector2)p));
                indexes.AddRange(UGeometryExplicit.kQuadToTriangles.Select(p=>startIndex + p));
                
                m_Materials[index].SetTexture("_MainTex",cell.texture);
            }

            if(renderer != null)
                renderer.sharedMaterials = m_Materials;
            
            _mesh.SetVertices(vertices);
            _mesh.SetUVs(0,uvs);
            _mesh.SetIndices(indexes,MeshTopology.Triangles,0,false);
            _mesh.subMeshCount = m_Asset.m_CellsData.Count;
            foreach (var (index,_) in m_Asset.m_CellsData.WithIndex())
                _mesh.SetSubMesh(index,new SubMeshDescriptor(index * 6,6));
            _mesh.bounds = m_Occlusion.GetBoundingBox();
            
            PoolList<Vector3>.IDespawn(vertices);
            PoolList<Vector2>.IDespawn(uvs);
            PoolList<int>.IDespawn(indexes);
        }

        public override void DrawGizmos(Camera _camera)
        {
            base.DrawGizmos(_camera);
            Gizmos.matrix = Matrix4x4.identity;
            
            var occlusionSphere = GetOcclusionSphereWS();
            occlusionSphere.DrawGizmos();
            Gizmos.DrawSphere(occlusionSphere.center,.1f);            
        }
    }
}
