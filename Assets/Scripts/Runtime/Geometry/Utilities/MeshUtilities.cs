using System.Collections.Generic;
using System.Linq;
using Geometry;
using TObjectPool;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public static class UMesh
    {
        public static readonly int[] kQuadToTriangles = new int[] {0, 1, 2, 2, 3, 0};
        public static void QuadToTriangleIndices(List<int> _indices, int _quad0, int _quad1, int _quad2, int _quad3)
        {
            _indices.Add(_quad0);
            _indices.Add(_quad1);
            _indices.Add(_quad2);
            _indices.Add(_quad2);
            _indices.Add(_quad3);
            _indices.Add(_quad0);
        }

        public static void ApplyQuadIndexes(NativeArray<int3> _indices, int _start,
            int _quad0, int _quad1, int _quad2, int _quad3)
        {
            _indices[_start] = new int3(_quad0,_quad1,_quad2);
            _indices[_start + 1] = new int3(_quad2,_quad3,_quad0);
        }
        
        public static void ApplyQuadIndexes(NativeArray<ushort> _indices, ushort _start,
            ushort _quad0, ushort _quad1, ushort _quad2, ushort _quad3)
        {
            _indices[_start + 0] = _quad0;
            _indices[_start + 1] = _quad1;
            _indices[_start + 2] = _quad2;
            _indices[_start + 3] = _quad2;
            _indices[_start + 4] = _quad3;
            _indices[_start + 5] = _quad0;
        }
        
        public static void FillQuadTriangle(this GQuad _quad, List<Vector3> _vertices, List<int> _indices,
            List<Vector2> _uvs, List<Vector3> _normals,List<Color> _colors,Color _color=default)
        {
            int indexOffset = _vertices.Count;
            for (int i = 0; i < 4; i++)
            {
                _vertices.Add(_quad[i]);
                _uvs?.Add(URender.IndexToQuadUV(i));
                _normals?.Add(_quad.normal);
                _colors?.Add(_color);
            }
            QuadToTriangleIndices(_indices, indexOffset + 0, indexOffset + 1, indexOffset + 2,indexOffset+3);
        }

        public static PTriangle[] GetPolygons(int[] _indices)
        {
            PTriangle[] polygons = new PTriangle[_indices.Length / 3];
            for (int i = 0; i < polygons.Length; i++)
            {
                int startIndex = i * 3;
                int triangle0 = _indices[startIndex];
                int triangle1 = _indices[startIndex + 1];
                int triangle2 = _indices[startIndex + 2];
                polygons[i] = new PTriangle(triangle0, triangle1, triangle2);
            }
            return polygons;
        }
        
        public static PTriangle[] GetPolygons(this Mesh _srcMesh, out int[] _indices)
        {
            _indices = _srcMesh.triangles;
            return GetPolygons(_indices);
        }

        public static GTriangle[] GetPolygonVertices(this Mesh _srcMesh, out int[] _indices,out Vector3[] _vertices)
        {
            var vertices = _srcMesh.vertices;
            var polygons = GetPolygons(_srcMesh, out _indices);
            _vertices = vertices;
            return polygons.Select(p => (GTriangle)p.Convert(vertices)).ToArray();
        }

    }
}