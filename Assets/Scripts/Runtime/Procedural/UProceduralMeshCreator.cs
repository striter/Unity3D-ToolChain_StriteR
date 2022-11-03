using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Procedural.Hexagon;
using Procedural.Tile;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Procedural
{
    public enum EProceduralMeshType
    {
        Square,
        Disk,
        Hexagon,
    }
    [Serializable]
    public struct ProceduralMeshInput
    {
        public EProceduralMeshType meshType;
        public float tileSize;

        [MFoldout(nameof(meshType),EProceduralMeshType.Disk,EProceduralMeshType.Hexagon)]public int radius;
        [MFoldout(nameof(meshType),EProceduralMeshType.Hexagon)] public bool centered;
        [MFoldout(nameof(meshType), EProceduralMeshType.Hexagon)] public bool rounded;
        
        [MFoldout(nameof(meshType),EProceduralMeshType.Square)]public int width;
        [MFoldout(nameof(meshType),EProceduralMeshType.Square)]public int height;
        [MFoldout(nameof(meshType),EProceduralMeshType.Square)]public Vector2 pivot;

        public static readonly ProceduralMeshInput kDefault = new ProceduralMeshInput()
        {
            meshType = EProceduralMeshType.Disk,
            radius = 25,
            tileSize = 2f,
            centered = false,
            width = 10,
            height = 10,
            pivot = Vector2.one*.5f,
        };

        public void Output( Mesh _mesh)
        {
            switch (meshType)
            {
                case EProceduralMeshType.Disk: ProceduralMeshCreator.PopulateDiskMesh(_mesh,tileSize,radius); break;
                case EProceduralMeshType.Hexagon: ProceduralMeshCreator.PopulateHexagonMesh(_mesh,tileSize,radius,centered,rounded); break;
                case EProceduralMeshType.Square:ProceduralMeshCreator.PopulateSquareMesh(_mesh,tileSize,width,height,pivot); break;
            }
        }
    }
    
    public class ProceduralMeshCreator
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        {
            public float3 position, normal;
            public half4 tangnet;
            public half2 texCoord0;
        }

        static void PopulateMeshData(Mesh _mesh , int _vertexCount, int _indexCount,Func<NativeArray<Vertex>,NativeArray<uint>,Bounds> _populateVertices)
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            
            Mesh.MeshData meshData = meshDataArray[0];
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(4,Allocator.Temp,NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position , VertexAttributeFormat.Float32,3);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal , VertexAttributeFormat.Float32 , 3);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent , VertexAttributeFormat.Float16 , 4);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0 , VertexAttributeFormat.Float16 , 2);
        
            meshData.SetVertexBufferParams(_vertexCount,vertexAttributes);
            vertexAttributes.Dispose();

            NativeArray<Vertex> vertices = meshData.GetVertexData<Vertex>(0);
            
            meshData.SetIndexBufferParams(_indexCount,IndexFormat.UInt32);
            NativeArray<uint> indices = meshData.GetIndexData<uint>();
            var bounds = _populateVertices(vertices, indices);
            
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,new SubMeshDescriptor(0,_indexCount){vertexCount = _vertexCount});
            _mesh.bounds = bounds;
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,_mesh, MeshUpdateFlags.DontRecalculateBounds);
            
        }

        public static void PopulateHexagonMesh(Mesh _mesh,float _tileSize, int _radius,bool _centered,bool _rounded)
        {
            var hexagonCoordsList = UHexagon.GetCoordsInRadius(HexCoord.zero, _radius,_rounded).ToList();
            int hexagonCount = hexagonCoordsList.Count;
            int vertexCountPerHexagon = _centered? 7 : 6;
            int indexCountPerHexagon = _centered? 18 : 12;
            int vertexCount = (hexagonCount * vertexCountPerHexagon);
            int indexCount = (hexagonCount * indexCountPerHexagon);
           PopulateMeshData(_mesh, vertexCount,indexCount, (vertices,indices) =>
           {
               UBoundsChecker.Begin();
               for(int i=0;i<hexagonCount;i++)
               {
                   uint startVertex = (uint)(i*vertexCountPerHexagon);
                   var startIndex = i*indexCountPerHexagon;
                   var center = hexagonCoordsList[i].ToCoord();
                   
                   int curVertexIndex = (int)startVertex;
                   if (_centered)
                   {
                       Vector3 position = new Vector3(center.x, 0f, center.y)*_tileSize;
                       vertices[curVertexIndex++] = new Vertex()
                       {
                           position = new float3(position),
                           normal = new float3(new float3(0f,1f,0f)),
                           tangnet = new half4( new float4(0f,0f,1f,1f)),
                           texCoord0 = new half2(new float2(center.x + .5f, center.y + .5f))
                       };
                   }
                   for (int j = 0; j < UHexagon.kUnitPoints.Length; j++)
                   {
                       var unitPoint = UHexagon.kUnitPoints[j];
                       var hexCorner = unitPoint + center;
                       Vector3 position = new Vector3(hexCorner.x, 0f, hexCorner.y)*_tileSize;
                       Vertex curVertex = new Vertex
                       {
                           position = new float3(position),
                           normal = new float3(new float3(0f,1f,0f)),
                           tangnet = new half4( new float4(0f,0f,1f,1f)),
                           texCoord0 = new half2(new float2(hexCorner.x + .5f, hexCorner.y + .5f))
                       };
                       UBoundsChecker.CheckBounds(position);
                       vertices[curVertexIndex++] = curVertex;
                   }

                   if (_centered)
                   {
                       indices[startIndex+0]=startVertex+0;
                       indices[startIndex+1]=startVertex+1;
                       indices[startIndex+2]=startVertex+2;
                
                       indices[startIndex+3]=startVertex+0;
                       indices[startIndex+4]=startVertex+2;
                       indices[startIndex+5]=startVertex+3;
                
                       indices[startIndex+6]=startVertex+0;
                       indices[startIndex+7]=startVertex+3;
                       indices[startIndex+8]=startVertex+4;
                
                       indices[startIndex+9]=startVertex+0;
                       indices[startIndex+10]=startVertex+4;
                       indices[startIndex+11]=startVertex+5;
                       
                       indices[startIndex+12]=startVertex+0;
                       indices[startIndex+13]=startVertex+5;
                       indices[startIndex+14]=startVertex+6;
                       
                       indices[startIndex+15]=startVertex+0;
                       indices[startIndex+16]=startVertex+6;
                       indices[startIndex+17]=startVertex+1;
                   }
                   else
                   {
                       indices[startIndex+0]=startVertex+0;
                       indices[startIndex+1]=startVertex+1;
                       indices[startIndex+2]=startVertex+2;
                
                       indices[startIndex+3]=startVertex+0;
                       indices[startIndex+4]=startVertex+2;
                       indices[startIndex+5]=startVertex+3;
                
                       indices[startIndex+6]=startVertex+0;
                       indices[startIndex+7]=startVertex+3;
                       indices[startIndex+8]=startVertex+4;
                
                       indices[startIndex+9]=startVertex+0;
                       indices[startIndex+10]=startVertex+4;
                       indices[startIndex+11]=startVertex+5;
                   }
               }

               return  UBoundsChecker.CalculateBounds();
           });
        }

        public static void PopulateSquareMesh(Mesh _mesh,float _tileSize,int _width,int _height,Vector2 _pivot)
        {
            int squareCount = _width * _height;
            int indexCount = squareCount * 6;
            int vertexCount = squareCount * 4;
            
            float3 tileSize = new float3(_tileSize, 0, _tileSize);
            float3 size = new float3(_width,0f,_height)*tileSize;
            float3 pivotOffset = new float3(size.x*_pivot.x,0f,size.z* _pivot.y);
            PopulateMeshData(_mesh,vertexCount,indexCount, (vertices, indexes) =>
            {
                int tileIndex = 0;
                for (int i=0;i< _width; i++)
                {
                    for (int j = 0; j < _height; j++)
                    {
                        int startVertex = tileIndex * 4;
                        int startIndex = tileIndex * 6;
                        vertices[startVertex] = new Vertex() {
                            position = new float3(i, 0, j) * tileSize - pivotOffset,
                            normal = new float3(0, 1, 0),
                            tangnet = new half4( new float4(0f,0f,1f,1f)),
                            texCoord0 = new half2(new float2(0f, 0f)),
                        };
                        vertices[startVertex+1] = new Vertex() {
                            position = new float3(i + 1, 0, j)*tileSize-pivotOffset,
                            normal = new float3(0, 1, 0),
                            tangnet = new half4( new float4(0f,0f,1f,1f)),
                            texCoord0 = new half2(new float2(1f, 0f)),
                        };
                        vertices[startVertex+2] = new Vertex() {
                            position = new float3(i, 0, j + 1)*tileSize-pivotOffset,
                            normal = new float3(0, 1, 0),
                            tangnet = new half4( new float4(0f,0f,1f,1f)),
                            texCoord0 = new half2(new float2(0f, 1f)),
                        };
                        vertices[startVertex+3] = new Vertex()  {
                            position = new float3(i + 1, 0, j + 1)*tileSize-pivotOffset,
                            normal = new float3(0, 1, 0),
                            tangnet = new half4( new float4(0f,0f,1f,1f)),
                            texCoord0 = new half2(new float2(1f, 1f)),
                        };
                        UBoundsChecker.CheckBounds(vertices[startVertex].position);
                        UBoundsChecker.CheckBounds(vertices[startVertex+1].position);
                        UBoundsChecker.CheckBounds(vertices[startVertex+2].position);
                        UBoundsChecker.CheckBounds(vertices[startVertex+3].position);
                        
                        uint indice0 = (uint)startVertex ;
                        uint indice1 = (uint)startVertex + 1;
                        uint indice2 = (uint)startVertex + 2;
                        uint indice3 = (uint)startVertex + 3;
                        indexes[startIndex]=(indice0);
                        indexes[startIndex+1]=(indice2);
                        indexes[startIndex+2]=(indice3);
                        indexes[startIndex+3]=(indice0);
                        indexes[startIndex+4]=(indice3);
                        indexes[startIndex+5]=(indice1);
                        tileIndex++;
                    }
                }

                return UBoundsChecker.CalculateBounds();
            });
        }
        
        public static void PopulateDiskMesh(Mesh _mesh,float _tileSize,int _radius)
        {
            var tileCoordsList = UTile.GetCoordsInRadius(TileCoord.kZero, _radius).ToList();
            var tileCount = tileCoordsList.Count;
            float3 offset = -new float3(_tileSize/2f, 0, _tileSize/2f);
            PopulateMeshData(_mesh,tileCount*4,tileCount*6, (vertices, indexes) =>
            {
                int tileIndex = 0;
                foreach (var coord in tileCoordsList)
                {
                    var center = coord.ToCoord();
                    var startPos = new float3(center.x*_tileSize,0f,center.y*_tileSize) + offset;
                    int startVertex = tileIndex * 4;
                    int startIndex = tileIndex * 6;
                    vertices[startVertex] = new Vertex() {
                        position = startPos,
                        normal = new float3(0, 1, 0),
                        tangnet = new half4( new float4(0f,0f,1f,1f)),
                        texCoord0 = new half2(new float2(0f, 0f)),
                    };
                    vertices[startVertex+1] = new Vertex() {
                        position = startPos + new float3(_tileSize, 0, 0),
                        normal = new float3(0, 1, 0),
                        tangnet = new half4( new float4(0f,0f,1f,1f)),
                        texCoord0 = new half2(new float2(1f, 0f)),
                    };
                    vertices[startVertex+2] = new Vertex() {
                        position = startPos + new float3(0, 0, _tileSize),
                        normal = new float3(0, 1, 0),
                        tangnet = new half4( new float4(0f,0f,1f,1f)),
                        texCoord0 = new half2(new float2(0f, 1f)),
                    };
                    vertices[startVertex+3] = new Vertex()  {
                        position = startPos + new float3(_tileSize, 0, _tileSize),
                        normal = new float3(0, 1, 0),
                        tangnet = new half4( new float4(0f,0f,1f,1f)),
                        texCoord0 = new half2(new float2(1f, 1f)),
                    };
                    UBoundsChecker.CheckBounds(vertices[startVertex].position);
                    UBoundsChecker.CheckBounds(vertices[startVertex+1].position);
                    UBoundsChecker.CheckBounds(vertices[startVertex+2].position);
                    UBoundsChecker.CheckBounds(vertices[startVertex+3].position);
                    
                    uint indice0 = (uint)startVertex ;
                    uint indice1 = (uint)startVertex + 1;
                    uint indice2 = (uint)startVertex + 2;
                    uint indice3 = (uint)startVertex + 3;
                    indexes[startIndex]=indice0;
                    indexes[startIndex+1]=indice2;
                    indexes[startIndex+2]=indice3;
                    indexes[startIndex+3]=indice0;
                    indexes[startIndex+4]=indice3;
                    indexes[startIndex+5]=indice1;
                    tileIndex++;
                }

                return UBoundsChecker.CalculateBounds();
            });
        }
    }
    
}