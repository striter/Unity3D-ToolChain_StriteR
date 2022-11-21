using System.Collections.Generic;
using System.Runtime.InteropServices;
using Geometry;
using TPool;
using TPoolStatic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheVoxel
{
    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public float3 position;
        public float3 normal;
        public half4 tangent;
        public half4 color;
        public half2 uv;
    }
    
    public class ChunkElement : PoolBehaviour<Int2>
    {
        private MeshFilter m_Filter;
        private MeshRenderer m_Renderer;
        private Mesh m_Mesh;
        private Dictionary<Int3, ChunkVoxel> m_Voxels = new Dictionary<Int3, ChunkVoxel>();

        public override void OnPoolSpawn(Int2 _identity)
        {
            base.OnPoolSpawn(_identity);
            transform.position = DVoxel.GetChunkPositionWS(m_PoolID);
            m_Filter = GetComponent<MeshFilter>();
            m_Renderer = GetComponent<MeshRenderer>();
            m_Mesh = new Mesh();
            m_Mesh.MarkDynamic();
            m_Filter.sharedMesh = m_Mesh;
            Generate();
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Voxels.Clear();
        }

        void Generate()
        {
            float r = DVoxel.kVoxelSize;
            for (int i = 0; i < DVoxel.kChunkVoxelSize; i++)
            for (int j = 0; j < DVoxel.kChunkVoxelSize; j++)
            {
                float2 p2D = new float2(i / r  + m_PoolID.x, j / r + m_PoolID.y);
                int heightOffset = (int) (Noise.Perlin.Unit1f2(p2D / 12f) * 5f);
                int groundHeight = DVoxel.kTerrainHeight + heightOffset;
                int dirtOffset = 5 + (int)(Noise.Value.Unit1f2(p2D)*2f);
                int stoneHeight = groundHeight - dirtOffset;
                int dirtHeight = groundHeight;
                
                for (int k = 0; k <= groundHeight; k++)
                {
                    Int3 identity = new Int3(i,k,j);
                    EVoxelType type = EVoxelType.Grass;
                    if (k < dirtHeight)
                        type = EVoxelType.Dirt;
                    if (k < stoneHeight)
                        type = EVoxelType.Stone;

                    float3 p3D = new float3(p2D.x,k/r,p2D.y);
                    if (Noise.Perlin.Unit1f3(p3D / 8) > .5f)
                        continue;
                    
                    m_Voxels.Add(identity,TSPool<ChunkVoxel>.Spawn().Init(identity,type));
                }
            }


            SetDirty();
        }

        void SetDirty()
        {
            int sideCount = 0;
            foreach (var voxel in m_Voxels.Values)
            {
                voxel.Refresh(m_Voxels);
                sideCount += voxel.m_SideCount;
            }
            
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            
            int vertexCount = sideCount * 4;
            int indexCount = sideCount * 6;

            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(5,Allocator.Temp,NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position , VertexAttributeFormat.Float32,3);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal , VertexAttributeFormat.Float32 , 3);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent , VertexAttributeFormat.Float16 , 4);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.Color , VertexAttributeFormat.Float16 , 4);
            vertexAttributes[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

            meshData.SetVertexBufferParams(vertexCount,vertexAttributes);
            vertexAttributes.Dispose();
            
            var vertices = meshData.GetVertexData<Vertex>();

            meshData.SetIndexBufferParams(indexCount,IndexFormat.UInt32);
            var indexes = meshData.GetIndexData<uint>().Reinterpret<uint3>(sizeof(uint));

            int vertexIndex = 0;
            int triangleIndex = 0;
            Vertex v = new Vertex();
            foreach (var voxel in m_Voxels.Values)
            {
                if (voxel.m_SideCount == 0)
                    continue;
                var color = UColor.IndexToColor((int) voxel.m_Type);
                float3 centerOS = DVoxel.GetVoxelPositionOS(voxel.m_Identity);
                v.color = (half4)new float4(color.r,color.g,color.b,color.a) ;
                for (int i = 0; i < 6; i++)
                {
                    if (!voxel.m_SideGeometry[i])
                        continue;

                    UCubeFacing.GetFacingQuadGeometry(UCubeFacing.IndexToFacing(i),out var b,out var l,out var f,out var r,out var n,out var t);
                    v.normal = n;
                    v.tangent = t;

                    v.position = b * DVoxel.kVoxelSize + centerOS;
                    v.uv = (half2) new float2(0, 0);
                    vertices[vertexIndex+0] = v;
                    v.position = l * DVoxel.kVoxelSize + centerOS;
                    v.uv = (half2) new float2(0, 1);
                    vertices[vertexIndex+1] = v;
                    v.position = f * DVoxel.kVoxelSize + centerOS;
                    v.uv = (half2) new float2(1, 1);
                    vertices[vertexIndex+2] = v;
                    v.position = r * DVoxel.kVoxelSize + centerOS;
                    v.uv = (half2) new float2(1, 0);
                    vertices[vertexIndex+3] = v;

                    uint iB = (uint)vertexIndex+0;
                    uint iL = (uint)vertexIndex+1;
                    uint iF = (uint)vertexIndex+2;
                    uint iR = (uint)vertexIndex+3;
                    indexes[triangleIndex++] = new uint3(iB, iL, iF);
                    indexes[triangleIndex++] = new uint3(iB, iF, iR);
                    vertexIndex += 4;
                }
            }

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,new SubMeshDescriptor(0,indexCount){vertexCount = vertexCount});
            m_Mesh.bounds = UBounds.MinMax(Vector3.zero, new float3(DVoxel.kVoxelSize * DVoxel.kChunkVoxelSize));
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,m_Mesh, MeshUpdateFlags.DontRecalculateBounds);
        }

        public bool m_Gizmos;
        private void OnDrawGizmos()
        {
            if (!m_Gizmos)
                return;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var voxel in m_Voxels.Values)
            {
                if (voxel.m_Type < 0)
                    continue;
                Gizmos.color = UColor.IndexToColor((int)voxel.m_Type);
                
                Gizmos.DrawWireSphere(DVoxel.GetVoxelPositionOS( voxel.m_Identity),DVoxel.kVoxelSize/4f);
            }
        }
    }

}