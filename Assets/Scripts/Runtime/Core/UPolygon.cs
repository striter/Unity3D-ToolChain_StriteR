using System.Collections;
using System.Collections.Generic;
using Geometry.Polygon;
using Geometry.Voxel;
using UnityEngine;

public static class UPolygon
{
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
        List<Vector2> _uvs, List<Vector3> _normals)
    {
        int indexOffset = _vertices.Count;
        _vertices.Add(_quad[0]);
        _vertices.Add(_quad[1]);
        _vertices.Add(_quad[2]);
        _vertices.Add(_quad[3]);
        if (_uvs!=null)
        {
            _uvs.Add(URender.IndexToQuadUV(0));
            _uvs.Add(URender.IndexToQuadUV(1));
            _uvs.Add(URender.IndexToQuadUV(2));
            _uvs.Add(URender.IndexToQuadUV(3));
        }

        if (_normals!=null)
        {
            var normal = Vector3.Cross(_quad[1]-_quad[0],_quad[3]-_quad[0]);
            for(int i=0;i<4;i++)
                _normals.Add(normal);
        }
            
        QuadToTriangleIndices(_indices, indexOffset + 0, indexOffset + 1, indexOffset + 2,indexOffset+3);
    }

    public static GTrianglePolygon[] GetPolygons(int[] _indices)
    {
        GTrianglePolygon[] polygons = new GTrianglePolygon[_indices.Length / 3];
        for (int i = 0; i < polygons.Length; i++)
        {
            int startIndex = i * 3;
            int triangle0 = _indices[startIndex];
            int triangle1 = _indices[startIndex + 1];
            int triangle2 = _indices[startIndex + 2];
            polygons[i] = new GTrianglePolygon(triangle0, triangle1, triangle2);
        }
        return polygons;
    }
    
    public static GTrianglePolygon[] GetPolygons(this Mesh _srcMesh, out int[] _indices)
    {
        _indices = _srcMesh.triangles;
        return GetPolygons(_indices);
    }
}