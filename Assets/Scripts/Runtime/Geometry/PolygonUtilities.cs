using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public static class UPolygon
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


    public static G2Polygon Clip(this G2Polygon _polygon,G2Plane _plane)    //Convex and Counter Clockwise is needed
    {
        List<float2> clippedPolygon = new List<float2>(_polygon);
        for (int i = _polygon.Count - 1; i >=0; i--)
        {
            var curPoint = _polygon[i];
            var nextPoint = _polygon[(i + 1) % _polygon.Count];
            var curDot = _plane.dot(curPoint);
            var nextDot = _plane.dot(nextPoint);
            var curForward = curDot > 0;
            var nextForward = nextDot > 0;

            if (curForward && !nextForward)
            {
                var t = curDot / math.dot(_plane.normal,(curPoint-nextPoint));
                var insertPoint = math.lerp(curPoint, nextPoint, t);
                clippedPolygon.Insert(i + 1, insertPoint);//;
            }
            else if (!curForward&&!nextForward)
                clippedPolygon.RemoveAt(i);
        }
        
        return new G2Polygon(clippedPolygon);
    }
}