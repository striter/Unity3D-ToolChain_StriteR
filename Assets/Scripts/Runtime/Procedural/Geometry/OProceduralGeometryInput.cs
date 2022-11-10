using System;
using System.Runtime.InteropServices;
using Procedural.Geometry.Cube;
using Procedural.Geometry.Sphere;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Procedural.Geometry
{
    public enum EProceduralMeshType
    {
        GridHexagon,
        GridSquare,
        Cube,
        SphereUV,
        SphereCube,
        SpherePolygon,
    }

    [Serializable]
    public struct ProceduralMeshInput
    {
        public EProceduralMeshType meshType;
        [MFoldout(nameof(meshType),EProceduralMeshType.GridHexagon)] public HexagonGridGenerator m_Hexagon;
        [MFoldout(nameof(meshType), EProceduralMeshType.GridSquare)]  public SquareGridGenerator m_Plane;
        [MFoldout(nameof(meshType), EProceduralMeshType.Cube)] public CubeGenerator m_Cube;
        [MFoldout(nameof(meshType), EProceduralMeshType.SphereUV)] public UVSphereGenerator m_UVSphere;
        [MFoldout(nameof(meshType), EProceduralMeshType.SphereCube)] public CubeSphereGenerator m_CubeSphere;
        [MFoldout(nameof(meshType), EProceduralMeshType.SpherePolygon)] public PolygonSphereGenerator m_PolygonSphere;
        public static readonly ProceduralMeshInput kDefault = new ProceduralMeshInput()
        {
            meshType = EProceduralMeshType.GridSquare,
            m_Hexagon = HexagonGridGenerator.kDefault,
            m_Plane = SquareGridGenerator.kDefault,
            m_Cube = CubeGenerator.kDefault,
            m_UVSphere = UVSphereGenerator.kDefault,
            m_CubeSphere = CubeSphereGenerator.kDefault,
            m_PolygonSphere = PolygonSphereGenerator.kDefault,
        };

        public void Output(Mesh _mesh)
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            switch (meshType)
            {
                default: throw new Exception($"Invalid Enum{meshType}");
                case EProceduralMeshType.GridHexagon:{
                    new ProceduralMeshJob<HexagonGridGenerator>(meshData, m_Hexagon).ScheduleParallel(1,1,default).Complete();
                } break;
                case EProceduralMeshType.GridSquare:  {
                    new ProceduralMeshJob<SquareGridGenerator>(meshData, m_Plane).ScheduleParallel(1,1,default).Complete(); 
                } break;
                case EProceduralMeshType.Cube:  {
                    new ProceduralMeshJob<CubeGenerator>(meshData,m_Cube).ScheduleParallel(1,1,default).Complete();
                }break;
                case EProceduralMeshType.SphereUV: {
                    new ProceduralMeshJob<UVSphereGenerator>(meshData,m_UVSphere).ScheduleParallel(1,1,default).Complete();
                }break;
                case EProceduralMeshType.SphereCube: {
                    new ProceduralMeshJob<CubeSphereGenerator>(meshData,m_CubeSphere).Execute(0);//.ScheduleParallel(1,1,default).Complete();
                }break;
                case EProceduralMeshType.SpherePolygon: {
                    new ProceduralMeshJob<PolygonSphereGenerator>(meshData,m_PolygonSphere).ScheduleParallel(1,1,default).Complete();
                }break;
            }

            _mesh.bounds = meshData.GetSubMesh(0).bounds;
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,_mesh, MeshUpdateFlags.DontRecalculateBounds);
        }
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct ProceduralMeshJob<T>:IJobFor where T:struct,IProceduralMeshGenerator
    { 
        [WriteOnly]private Mesh.MeshData meshData;
        private T generator;
        public ProceduralMeshJob(Mesh.MeshData _mesh,T _generator)
        {
            meshData = _mesh;
            generator = _generator;
        }

        public void Execute(int _jobIndex)
        {
            int vertexCount = generator.vertexCount;
            int indexCount = generator.triangleCount * 3;
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(4,Allocator.Temp,NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position , VertexAttributeFormat.Float32,3);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal , VertexAttributeFormat.Float32 , 3);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent , VertexAttributeFormat.Float16 , 4);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0 , VertexAttributeFormat.Float16 , 2);

            meshData.SetVertexBufferParams(generator.vertexCount,vertexAttributes);
            vertexAttributes.Dispose();

            var vertices = meshData.GetVertexData<Vertex>(0);

            meshData.SetIndexBufferParams(indexCount,IndexFormat.UInt32);
            var indices = meshData.GetIndexData<uint>().Reinterpret<uint3>(sizeof(uint));
            generator.Execute(_jobIndex,vertices,indices);
            
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 compare = vertices[i].position;
                min = Vector3.Min(min,compare);
                max = Vector3.Max(max, compare);
            }

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,new SubMeshDescriptor(0,indexCount){vertexCount = generator.vertexCount,bounds =UBounds.MinMax(min, max)});
        }
    }
}