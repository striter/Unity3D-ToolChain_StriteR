using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Extension
{
    public static class UGeometryExplicit
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
        
        public static Vector2 IndexToQuadUV(int _index) => _index switch {
                0 => Vector2.zero,
                1 => Vector2.right,
                2 => Vector2.one,
                3 => Vector2.up,
                _ => throw new Exception("Invalid Index:" + _index)
            };
        
        public static void PopulateVertex(this GQuad _quad, List<Vector3> _vertices, List<int> _indices,
            List<Vector2> _uvs = default, List<Vector3> _normals = default,List<Vector4> _tangents = default,List<Color> _colors = default,Color _color=default)
        {
            var indexOffset = _vertices.Count;
            for (var i = 0; i < 4; i++)
            {
                _vertices.Add(_quad[i]);
                _uvs?.Add(IndexToQuadUV(i));
                _colors?.Add(_color);
            };
            _normals?.AddRange(_quad.GetVertexNormals().Select(p=>(Vector3)p));
            _tangents?.AddRange(_quad.GetVertexTangents().Select(p=>(Vector4)p.to4(1)));
            QuadToTriangleIndices(_indices, indexOffset + 0, indexOffset + 1, indexOffset + 2,indexOffset+3);
        }

        
        public static void PopulateVertex(this GLine _line,float _lineWidth,float3 _upVector, List<Vector3> _vertices, List<int> _indices,
            List<Vector2> _uvs = default, List<Vector3> _normals = default,List<Vector4> _tangents = default,List<Color> _colors = default,Color _color=default)
        {
            var indexOffset = _vertices.Count;
            var right = math.cross(_upVector,_line.direction).normalize();
            
            _vertices.Add(_line.start - right * _lineWidth * 0.5f);
            _vertices.Add(_line.end - right * _lineWidth * 0.5f);
            _vertices.Add(_line.end + right * _lineWidth * 0.5f);
            _vertices.Add(_line.start + right * _lineWidth * 0.5f);

            if (_normals != null)
            {
                _normals.Add(_upVector);
                _normals.Add(_upVector);
                _normals.Add(_upVector);
                _normals.Add(_upVector);
            }

            if (_uvs != null)
            {
                _uvs.Add(G2Quad.kDefaultUV[0]);
                _uvs.Add(G2Quad.kDefaultUV[1]);
                _uvs.Add(G2Quad.kDefaultUV[2]);
                _uvs.Add(G2Quad.kDefaultUV[3]); 
            }

            if (_tangents != null)
            {
                _tangents.Add(new Vector4(1, 0, 0, 1));
                _tangents.Add(new Vector4(1, 0, 0, 1));
                _tangents.Add(new Vector4(1, 0, 0, 1));
                _tangents.Add(new Vector4(1, 0, 0, 1));
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
        
        public static PTriangle[] GetPolygons(this UnityEngine.Mesh _srcMesh, out int[] _indices)
        {
            _indices = _srcMesh.triangles;
            return GetPolygons(_indices);
        }

        public static PLine[] GetEdges(this UnityEngine.Mesh _srcMesh, out int[] _indices)
        {
            _indices = _srcMesh.triangles;
            var polygons = GetPolygons(_srcMesh, out _indices);
            return polygons.Select(p => p.GetLines()).Resolve().Distinct().ToArray();
        }

        public static IEnumerable<PLine> GetDistinctEdges(this IEnumerable<PTriangle> _triangles) => _triangles.SelectMany(p => p.GetLines()).Select(p=>p.start > p.end ? new PLine(p.end,p.start) : p).Distinct();
        
        public static GTriangle[] GetPolygonVertices(this UnityEngine.Mesh _srcMesh, out int[] _indices,out Vector3[] _vertices)
        {
            var vertices = _srcMesh.vertices;
            var polygons = GetPolygons(_srcMesh, out _indices);
            _vertices = vertices;
            return polygons.Select(p => (GTriangle)p.Convert(vertices)).ToArray();
        }

    }
}