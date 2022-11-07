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
    using static UMath;
    using static KMath;
    public enum EProceduralMeshType
    {
        Hexagon,
        Plane,
        UVSphere,
        Cube,
    }
    [Serializable]
    public struct ProceduralMeshInput
    {
        public EProceduralMeshType meshType;
        [MFoldout(nameof(meshType),EProceduralMeshType.Hexagon)] public HexagonGridGenerator m_Hexagon;
        [MFoldout(nameof(meshType), EProceduralMeshType.Plane)]  public SquareGridGenerator m_Plane;
        [MFoldout(nameof(meshType), EProceduralMeshType.UVSphere)] public UVSphereGenerator m_UVSphere;
        [MFoldout(nameof(meshType), EProceduralMeshType.Cube)] public CubeGenerator m_Cube;
        public static readonly ProceduralMeshInput kDefault = new ProceduralMeshInput()
        {
            meshType = EProceduralMeshType.Plane,
            m_Hexagon = HexagonGridGenerator.kDefault,
            m_Plane = SquareGridGenerator.kDefault,
            m_UVSphere = UVSphereGenerator.kDefault,
            m_Cube = CubeGenerator.kDefault,
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
                case EProceduralMeshType.UVSphere: {
                    new ProceduralMeshJob<UVSphereGenerator>(meshData,m_UVSphere).ScheduleParallel(1,1,default).Complete();
                }break;
                case EProceduralMeshType.Cube: {
                    new ProceduralMeshJob<CubeGenerator>(meshData,m_Cube).ScheduleParallel(1,1,default).Complete();
                } break;
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
        public static readonly SquareGridGenerator kDefault = new SquareGridGenerator() {tileSize = 2f,radius=5,width = 10,height = 10,pivot = KVector.kHalf2,m_Disk = false}.Ctor();
        
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
    public struct UVSphereGenerator:IProceduralMeshGenerator
    {
        [Clamp(1,50f)]public float radius;
        [Clamp(1,500)]public int resolution;
        public static readonly UVSphereGenerator kDefault = new UVSphereGenerator(){radius=1f,resolution = 20};

        private int resolutionU => 4 * resolution;
        private int resolutionV => 2 * resolution;
        public int vertexCount => (resolutionU+1)*(resolutionV+1);
        public int triangleCount => resolutionU * resolutionV * 2;
        public void Execute(int _index, NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            int ti = 0;
            int vertexWidth = resolutionU + 1;
            
            var vertex = new Vertex();
            vertex.tangent.w = new half(-1);
            for (int j = 0; j <= resolutionV; j++)
                for (int i = 0; i <= resolutionU; i++ )
                {
                    var curIndex = new TileCoord(i, j).ToIndex(vertexWidth);
                    float2 circle = new float2( i / (float)resolutionU,j / (float)resolutionV);
                    float radius = Sin(circle.y * kPI);
                    math.sincos(kPI2*circle.x,out vertex.position.z,out vertex.position.x);
                    vertex.position.xz *= radius;
                    vertex.position.y = -Cos(kPI*circle.y) ;
                    circle.x = (i - .5f) / resolutionU;
                    vertex.texCoord0.xy = (half2)circle;
                    math.sincos(kPI2*circle.x,out var tangentZ,out var tangentX);
                    vertex.tangent.xz = new half2(new float2(tangentX,tangentZ));
                    vertex.normal = vertex.position.xyz;
                    vertex.position *= this.radius;
                    _vertices[curIndex] = vertex;
                    if ( i < resolutionU && j < resolutionV )
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

    [Serializable]
    public struct CubeGenerator : IProceduralMeshGenerator
    {
        [Clamp(1,500)]public int resolution;
        public bool sphere;
        public static CubeGenerator kDefault = new CubeGenerator() {resolution = 10};

        private const int kSideCount = 6;
        struct Side
        {
            public int index;
            public float3 origin;
            public float3 uDir;
            public float3 vDir;
        }

        struct Point
        {
            public half2 uv;
            public float3 position;
            public int index;
        }
        
        private int sideVertexCount => (resolution+1)*(resolution+1);
        public int vertexCount => kSideCount * sideVertexCount;
        public int triangleCount => kSideCount * resolution * resolution * 2;

        Side GetCubeSide(int _index)
        {
            switch (_index)
            {
                default: throw new Exception("Invalid Index");
                case 0: return new Side() { index = 0, origin = -1f, uDir = new float3(2, 0, 0), vDir = new float3(0, 2, 0) };
                case 1: return new Side() { index = 1, origin = new float3(1f, -1f, -1f), uDir = new float3(0, 0, 2f), vDir = new float3(0, 2f, 0f) };
                case 2: return new Side() { index = 2, origin = -1, uDir = new float3(0, 0, 2f), vDir = new float3(2f, 0, 0) };
                case 3: return new Side() { index = 3, origin = new float3(-1f, -1f, 1f), uDir = new float3(0, 2f, 0), vDir = new float3(2f, 0, 0) };
                case 4: return new Side() { index = 4, origin = -1f, uDir = new float3(0, 2f, 0), vDir = new float3(0f, 0, 2f) };
                case 5: return new Side() { index = 5, origin = new float3(-1f, 1f, -1f), uDir = new float3(2f, 0f, 0), vDir = new float3(0f, 0, 2f) };
            }
        }

        private float3 ConvertPoint(float3 _point)
        {
            if (sphere)
            {
                float3 p = _point;
                float3 sqrP = p * p;
                return p * math.sqrt(1f - (sqrP.yxx + sqrP.zzy) / 2f + sqrP.yxx * sqrP.zzy / 3f);
            }
            return _point;
        }

        private Point GetPoint(int _i,int _j,Side _side)
        {
            float r = resolution;
            int vertexWidth = resolution + 1;
            float2 uv = new float2(_i / r, _j / r);
            int index = sideVertexCount * _side.index + new TileCoord(_i, _j).ToIndex(vertexWidth);
            return new Point()
            {
                uv = (half2)uv,
                index = index,
                position = ConvertPoint(_side.origin + uv.x*_side.uDir + uv.y * _side.vDir),
            };
        }
        
        public void Execute(int _index, NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            int ti = 0;
            float r = resolution;

            var vertex = new Vertex();
            vertex.tangent.w = new half(-1);

            for (int k = 0; k < kSideCount; k++)
            {
                var side = GetCubeSide(k);   
                for (int j = 0; j < resolution; j++)
                for (int i = 0; i < resolution; i++)
                {
                    var pTR = GetPoint(i + 1, j + 1, side);
                    var pTL = GetPoint(i , j + 1, side);
                    var pBR = GetPoint(i + 1, j, side);
                    var pBL = GetPoint(i, j, side);
                    
                    vertex.tangent = (half4) new float4(math.normalize(pBR.position-pBL.position) , -1f);
                    vertex.normal = math.normalize(math.cross(pTR.position-pBR.position,vertex.tangent.xyz));
                    vertex.position = pTR.position;
                    vertex.texCoord0.xy = pTR.uv;
                    _vertices[pTR.index] = vertex;
                    
                    vertex.normal = math.normalize(math.cross(pTL.position-pBL.position,vertex.tangent.xyz));
                    vertex.position = pTL.position;
                    vertex.texCoord0.xy = pTL.uv;
                    _vertices[pTL.index] = vertex;
                    
                    vertex.normal = math.normalize(math.cross(pTR.position-pBR.position,vertex.tangent.xyz));
                    vertex.position = pBR.position;
                    vertex.texCoord0.xy = pBR.uv;
                    _vertices[pBR.index] = vertex;
                    
                    vertex.normal = math.normalize(math.cross(pTL.position-pBL.position,vertex.tangent.xyz));
                    vertex.position = pBL.position;
                    vertex.texCoord0.xy = pBL.uv;
                    _vertices[pBL.index] = vertex;
                    
                    _triangles[ti++] = (uint3) new int3(pTR.index, pBR.index, pBL.index);
                    _triangles[ti++] = (uint3) new int3(pBL.index, pTL.index, pTR.index);
                }
            }
         
        }
    }
}