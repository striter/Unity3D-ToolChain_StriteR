using System.Runtime.InteropServices;
using Runtime.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheVoxel.ChunkProcess
{


        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct ExplicitMeshJob : IJobFor
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Vertex
            {
                public float3 position;
                public float3 normal;
                public half4 tangent;
                public half4 color;
                public half2 uv;
            }

            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeHashMap<Int3,ChunkVoxel> voxels;
            [WriteOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<Vertex> vertices;
            [WriteOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<uint3> indexes;
            public ExplicitMeshJob(Mesh.MeshData _meshData, NativeHashMap<Int3,ChunkVoxel> _voxels,int _vertexCount,int _indexCount)
            {
                voxels = _voxels;
                
                var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(5,Allocator.Temp,NativeArrayOptions.UninitializedMemory);
                vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position , VertexAttributeFormat.Float32,3);
                vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal , VertexAttributeFormat.Float32 , 3);
                vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent , VertexAttributeFormat.Float16 , 4);
                vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.Color , VertexAttributeFormat.Float16 , 4);
                vertexAttributes[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

                _meshData.SetVertexBufferParams(_vertexCount,vertexAttributes);
                _meshData.SetIndexBufferParams(_indexCount,IndexFormat.UInt32);

                vertices = _meshData.GetVertexData<Vertex>();
                indexes = _meshData.GetIndexData<uint>().Reinterpret<uint3>(sizeof(uint));
                
                vertexAttributes.Dispose();
            }

            static float ConvertToAO(int _intervalSrc,int _side1,int _side2,int _cornerByte,int _corner)
            {
                var side1 = (_intervalSrc >> _side1) & 1;
                var side2 = (_intervalSrc >> _side2) & 1;

                var corner = (_cornerByte >> _corner) & 1;
                return (3-side1-side2-corner)/3f;
            }
            
            public void Execute(int _)
            {
                var v = new Vertex();
                var length = voxels.Count();
                var keys = voxels.GetKeyArray(Allocator.Temp);

                var vertexIndex = 0;
                var triangleIndex = 0;
                for (var i = 0; i < length; i++)
                {
                    var voxel = voxels[keys[i]];
                    if (voxel.sideGeometry == ChunkVoxel.kEmptyGeometry || voxel.sideGeometry == ChunkVoxel.kFullGeometry)
                        continue;

                    v.color = (half4)DVoxel.GetVoxelBaseColor(voxel.type);
                    float3 centerOS = DVoxel.GetVoxelPositionOS(voxel.identity);
                    for (var j = 0; j < 6; j++)
                    {
                        if (UByte.PosValid(voxel.sideGeometry, j))
                            continue;

                        var facing = UCube.IndexToFacing(j);
                        UCube.GetCornerGeometry(facing ,out var b,out var l,out var f,out var r,out var n,out var t);
                        float aob = ConvertToAO(voxel.intervalGeometry, b.side1, b.side2, voxel.cornerGeometry,b.corner),
                            aol = ConvertToAO(voxel.intervalGeometry, l.side1, l.side2, voxel.cornerGeometry, l.corner),
                            aof = ConvertToAO(voxel.intervalGeometry, f.side1, f.side2, voxel.cornerGeometry, f.corner),
                            aor = ConvertToAO(voxel.intervalGeometry, r.side1, r.side2, voxel.cornerGeometry, r.corner);
                        
                        v.normal = n;
                        v.tangent = t;

                        v.position = b.position * DVoxel.kVoxelSize + centerOS;
                        v.uv = (half2) new float2(0, 0);
                        v.color.w = (half)aob;
                        vertices[vertexIndex+0] = v;
                        v.position = l.position * DVoxel.kVoxelSize + centerOS;
                        v.color.w = (half)aol;
                        v.uv = (half2) new float2(0, 1);
                        vertices[vertexIndex+1] = v;
                        v.position = f.position * DVoxel.kVoxelSize + centerOS;
                        v.color.w = (half)aof;
                        v.uv = (half2) new float2(1, 1);
                        vertices[vertexIndex+2] = v;
                        v.position = r.position * DVoxel.kVoxelSize + centerOS;
                        v.color.w = (half) aor;
                        v.uv = (half2) new float2(1, 0);
                        vertices[vertexIndex+3] = v;

                        var iB = (uint)vertexIndex+0;
                        var iL = (uint)vertexIndex+1;
                        var iF = (uint)vertexIndex+2;
                        var iR = (uint)vertexIndex+3;
                        if (aol + aor > aof + aob)
                        {
                            indexes[triangleIndex] = new uint3(iL, iR, iB);
                            indexes[triangleIndex + 1] = new uint3(iL, iF, iR);
                        }
                        else
                        {
                            indexes[triangleIndex] = new uint3(iB, iL, iF);
                            indexes[triangleIndex + 1] = new uint3(iB, iF, iR);
                        }
                        
                        vertexIndex += 4;
                        triangleIndex += 2;
                    }
                }

                keys.Dispose();
            }
        }
}