using System;
using System.Linq;
using System.Runtime.InteropServices;
using Procedural.Hexagon;
using Procedural.Tile;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Procedural
{
    public enum EProceduralMeshType
    {
        Hexagon,
        Plane,
        UVPlane,
    }
    [Serializable]
    public struct ProceduralMeshInput
    {
        public EProceduralMeshType meshType;
        [MFoldout(nameof(meshType),EProceduralMeshType.Hexagon)] public HexagonGridGenerator m_Hexagon;
        [MFoldout(nameof(meshType), EProceduralMeshType.Plane)]  public SquareGridGenerator m_Plane;
        [MFoldout(nameof(meshType), EProceduralMeshType.UVPlane)] public UVPlaneGenerator m_UVPlane;
        public static readonly ProceduralMeshInput kDefault = new ProceduralMeshInput()
        {
            meshType = EProceduralMeshType.Plane,
            m_Hexagon = HexagonGridGenerator.kDefault,
            m_Plane = SquareGridGenerator.kDefault,
            m_UVPlane = UVPlaneGenerator.kDefault,
        };

        public void Output(Mesh _mesh)
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            switch (meshType)
            {
                default: throw new Exception($"Invalid Enum{meshType}");
                case EProceduralMeshType.Hexagon:{
                    new ProceduralMeshJob<HexagonGridGenerator>(meshData, m_Hexagon).ScheduleParallel(1,1,default).Complete();
                } break;
                case EProceduralMeshType.Plane:  {
                    new ProceduralMeshJob<SquareGridGenerator>(meshData, m_Plane).ScheduleParallel(1,1,default).Complete(); 
                } break;
                case EProceduralMeshType.UVPlane: {
                    new ProceduralMeshJob<UVPlaneGenerator>(meshData,m_UVPlane).ScheduleParallel(1,1,default).Complete();
                }break;
            }

            _mesh.bounds = meshData.GetSubMesh(0).bounds;
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,_mesh, MeshUpdateFlags.DontRecalculateBounds);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public float3 position, normal;
        public half4 tangent;
        public half2 texCoord0;
    }

    public interface IProceduralMeshGenerator
    {
        public int vertexCount { get; }
        public int triangleCount { get;}
        void Execute(int _index,NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles);
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
    
    [Serializable]
    public struct HexagonGridGenerator:IProceduralMeshGenerator,ISerializationCallbackReceiver
    {
        public float tileSize;
        public int radius;
        public bool centered;
        public bool rounded;
        public static HexagonGridGenerator kDefault = new HexagonGridGenerator() {tileSize = 2f,radius =  25,centered = false,rounded = false}.Ctor();
        
        public int vertexCount { get; set; }
        public int triangleCount { get; set; }

        private int vertexCountPerHexagon;
        private int triangleCountPerHexagon;
        private Vector3 boundsMin, boundsMax;
        public HexagonGridGenerator Ctor()
        {
            var hexagonCount = UHexagon.GetCoordsInRadius(HexCoord.zero, radius,rounded).Count();
            vertexCountPerHexagon = centered? 7 : 6;
            triangleCountPerHexagon = centered? 6 : 4;
            vertexCount = hexagonCount * vertexCountPerHexagon;
            triangleCount = hexagonCount * triangleCountPerHexagon;
            return this;
        }
        
        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize()=>Ctor();
        
        public void Execute(int _index,NativeArray<Vertex> _vertices,NativeArray<uint3> _indices)
        {
            boundsMin = Vector3.zero;
            boundsMax = Vector3.zero;
            int index = 0;

            int sqrRadius = UMath.Pow2(radius+1);
            var unitPoints = KHexagon.kFlatUnitPoints;

            // foreach (var coord in  UHexagon.GetCoordsInRadius(HexCoord.zero, radius,rounded))
            for (int i = -radius; i <= radius; i++)
            for (int j = -radius; j <= radius; j++)
            {
               var coord = new HexCoord(i, j);
               if (!coord.InRange(radius))
                   continue;
               if (rounded && coord.x * coord.x + coord.y * coord.y + coord.z*coord.z >= sqrRadius)
                   continue;
               
               uint startVertex = (uint)(index*vertexCountPerHexagon);
               var startTriangle = index*triangleCountPerHexagon;
               var center = new Coord(KHexagon.kFlatAxialToPixel.Multiply(coord.col,coord.row));
               
               int curVertexIndex = (int)startVertex;
               if (centered)
               {
                   Vector3 position = new Vector3(center.x, 0f, center.y)*tileSize;
                   _vertices[curVertexIndex++] = new Vertex()
                   {
                       position = new float3(position),
                       normal = new float3(new float3(0f,1f,0f)),
                       tangent = new half4( new float4(0f,0f,1f,1f)),
                       texCoord0 = new half2(new float2(center.x + .5f, center.y + .5f))
                   };
               }

               for (int k = 0; k < unitPoints.Length; k++)
               {
                   var unitPoint = unitPoints[k];
                   var hexCorner = unitPoint + center;
                   Vector3 position = new Vector3(hexCorner.x, 0f, hexCorner.y)*tileSize;
                   Vertex curVertex = new Vertex
                   {
                       position = new float3(position),
                       normal = new float3(new float3(0f,1f,0f)),
                       tangent = new half4( new float4(0f,0f,1f,1f)),
                       texCoord0 = new half2(new float2(hexCorner.x + .5f, hexCorner.y + .5f))
                   };
                   boundsMin = Vector3.Min(boundsMin, position);
                   boundsMax = Vector3.Max(boundsMax, position);
                   _vertices[curVertexIndex++] = curVertex;
               }

               if (centered)
               {
                   _indices[startTriangle+0]=new uint3(startVertex,startVertex+1,startVertex+2);
                   _indices[startTriangle+1]=new uint3(startVertex,startVertex+2,startVertex+3);
                   _indices[startTriangle+2]=new uint3(startVertex,startVertex+3,startVertex+4);
                   _indices[startTriangle+3]=new uint3(startVertex,startVertex+4,startVertex+5);
                   _indices[startTriangle+4]=new uint3(startVertex,startVertex+5,startVertex+6);
                   _indices[startTriangle+5]=new uint3(startVertex,startVertex+6,startVertex+1);
               }
               else
               {
                   _indices[startTriangle+0]=new uint3(startVertex,startVertex+1,startVertex+2);
                   _indices[startTriangle+1]=new uint3(startVertex,startVertex+2,startVertex+3);
                   _indices[startTriangle+2]=new uint3(startVertex,startVertex+3,startVertex+4);
                   _indices[startTriangle+3]=new uint3(startVertex,startVertex+4,startVertex+5);
               }
               index++;
            }
        }
    }

    [Serializable]
    public struct SquareGridGenerator : IProceduralMeshGenerator, ISerializationCallbackReceiver
    {
        public float tileSize;
        public bool m_Disk;
        [MFoldout(nameof(m_Disk),true)]public int radius;
        [MFoldout(nameof(m_Disk),false)]public int width;
        [MFoldout(nameof(m_Disk),false)]public int height;
        [MFoldout(nameof(m_Disk),false)]public Vector2 pivot;
        public static readonly SquareGridGenerator kDefault = new SquareGridGenerator() {radius=20,width = 10,height = 10,pivot = KVector.kHalf2,m_Disk = false}.Ctor();
        
        public int vertexCount { get; set; }
        public int triangleCount { get; set; }
        SquareGridGenerator Ctor()
        {
            int squareCount = m_Disk?  UTile.GetCoordsInRadius(TileCoord.kZero,radius).Count() : width*height;
            
            vertexCount = squareCount * 4;
            triangleCount = squareCount * 2;
            return this;
        }
        public void Execute(int _index, NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            if(m_Disk)
                PopulateDiskMesh(_vertices,_triangles);
            else
                PopulateSquareMesh(_vertices,_triangles);
        }

        void PopulateSquareMesh( NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            float3 tileSize3 = new float3(tileSize, 0, tileSize);
            float3 size = new float3(width,0f,height)*tileSize;
            float3 pivotOffset = new float3(size.x*pivot.x,0f,size.z* pivot.y);
            int tileIndex = 0;
            for (int i=0;i< width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int startVertex = tileIndex * 4;
                    int startTriangle = tileIndex * 2;
                    _vertices[startVertex] = new Vertex() {
                        position = new float3(i, 0, j) * tileSize3 - pivotOffset,
                        normal = new float3(0, 1, 0),
                        tangent = new half4( new float4(0f,0f,1f,1f)),
                        texCoord0 = new half2(new float2(0f, 0f)),
                    };
                    _vertices[startVertex+1] = new Vertex() {
                        position = new float3(i + 1, 0, j)*tileSize3-pivotOffset,
                        normal = new float3(0, 1, 0),
                        tangent = new half4( new float4(0f,0f,1f,1f)),
                        texCoord0 = new half2(new float2(1f, 0f)),
                    };
                    _vertices[startVertex+2] = new Vertex() {
                        position = new float3(i, 0, j + 1)*tileSize3-pivotOffset,
                        normal = new float3(0, 1, 0),
                        tangent = new half4( new float4(0f,0f,1f,1f)),
                        texCoord0 = new half2(new float2(0f, 1f)),
                    };
                    _vertices[startVertex+3] = new Vertex()  {
                        position = new float3(i + 1, 0, j + 1)*tileSize3-pivotOffset,
                        normal = new float3(0, 1, 0),
                        tangent = new half4( new float4(0f,0f,1f,1f)),
                        texCoord0 = new half2(new float2(1f, 1f)),
                    };
                    
                    uint index0 = (uint)startVertex ;
                    uint index1 = (uint)startVertex + 1;
                    uint index2 = (uint)startVertex + 2;
                    uint index3 = (uint)startVertex + 3;
                    _triangles[startTriangle]=new uint3(index0,index2,index3);
                    _triangles[startTriangle+1]=new uint3(index0,index3,index1);
                    tileIndex++;
                }
            }
        }
        
        public void PopulateDiskMesh( NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            float3 offset = -new float3(tileSize/2f, 0, tileSize/2f);
            int tileIndex = 0;
            // foreach (var coord in UTile.GetCoordsInRadius(TileCoord.kZero, radius)) {
            int sqrRadius = UMath.Pow2(radius+1);
            for(int i=-radius;i<=radius;i++)
            for(int j=-radius;j<=radius;j++)
            {
                var coord = new TileCoord(i, j);
                if (!(UMath.Pow2(Mathf.Abs(coord.x)) + UMath.Pow2(Mathf.Abs(coord.y)) <= sqrRadius))
                    continue;
                var center = coord.ToCoord();
                var startPos = new float3(center.x*tileSize,0f,center.y*tileSize) + offset;
                int startVertex = tileIndex * 4;
                int startTriangle = tileIndex * 2;
                _vertices[startVertex] = new Vertex() {
                    position = startPos,
                    normal = new float3(0, 1, 0),
                    tangent = new half4( new float4(0f,0f,1f,1f)),
                    texCoord0 = new half2(new float2(0f, 0f)),
                };
                _vertices[startVertex+1] = new Vertex() {
                    position = startPos + new float3(tileSize, 0, 0),
                    normal = new float3(0, 1, 0),
                    tangent = new half4( new float4(0f,0f,1f,1f)),
                    texCoord0 = new half2(new float2(1f, 0f)),
                };
                _vertices[startVertex+2] = new Vertex() {
                    position = startPos + new float3(0, 0, tileSize),
                    normal = new float3(0, 1, 0),
                    tangent = new half4( new float4(0f,0f,1f,1f)),
                    texCoord0 = new half2(new float2(0f, 1f)),
                };
                _vertices[startVertex+3] = new Vertex()  {
                    position = startPos + new float3(tileSize, 0, tileSize),
                    normal = new float3(0, 1, 0),
                    tangent = new half4( new float4(0f,0f,1f,1f)),
                    texCoord0 = new half2(new float2(1f, 1f)),
                };
                
                uint index0 = (uint)startVertex ;
                uint index1 = (uint)startVertex + 1;
                uint index2 = (uint)startVertex + 2;
                uint index3 = (uint)startVertex + 3;
                _triangles[startTriangle] = new uint3(index0,index2,index3);
                _triangles[startTriangle+1] = new uint3(index0,index3,index1);
                tileIndex++;
            }
        }
    
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize() => Ctor();
    }
    
    [Serializable]
    public struct UVPlaneGenerator:IProceduralMeshGenerator
    { 
         [Clamp(1,500)]public int resolution;
        public static readonly UVPlaneGenerator kDefault = new UVPlaneGenerator(){resolution = 20};
        
        public int vertexCount => UMath.Pow2(resolution + 1);
        public int triangleCount => resolution * resolution * 2;
        public void Execute(int _index, NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            int ti = 0;
            float r = resolution;
            float2 pivotOffset = new float2(.5f,.5f);
            int vertexWidth = resolution + 1;
            
            var vertex = new Vertex();
            vertex.texCoord0.y = new half();
            vertex.normal.y = 1;
            vertex.tangent.xw = new half2(new float2(1f, -1f));
            for (int j = 0; j <= resolution; j++)
                for (int i = 0; i <= resolution; i++ )
                {
                    var curIndex = new TileCoord(i, j).ToIndex(vertexWidth);
                    vertex.position.xz = new float2(i/r,j / r )-pivotOffset;
                    vertex.texCoord0.xy = (half2)new float2( i / r,j / r);
                    _vertices[curIndex] = vertex;
                    if ( i < resolution && j < resolution )
                    {
                        var iTR = new TileCoord(i + 1, j + 1).ToIndex(vertexWidth);
                        var iTL = new TileCoord(i, j + 1).ToIndex(vertexWidth);
                        var iBR = new TileCoord(i + 1, j).ToIndex(vertexWidth);
                        var iBL = new TileCoord(i , j).ToIndex(vertexWidth);
                        
                        _triangles[ti++] =  (uint3)new int3( iTR,iBR,iBL);
                        _triangles[ti++] = (uint3)new int3(iBL,iTL,iTR);
                    }
                }
        }
    }
}