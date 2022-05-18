using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.Polygon;
using Geometry.Voxel;
using UnityEngine;

public static class UMesh
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
    
    static Vector3[] RegenerateNormals(int[] _indices, Vector3[] _vertices)
    {
        Vector3[] normals = new Vector3[_vertices.Length];
        GTrianglePolygon[] polygons = UGeometryPolygon.GetPolygons(_indices);
        foreach(var polygon in polygons)
        {
            GTriangle triangle = new GTriangle(polygon.GetVertices(_vertices));
            Vector3 normal = triangle.normal;
            foreach (var index in polygon)
                normals[index] += normal;
        }
        normals=normals.Select(normal => normal.normalized).ToArray();
        return normals;
    }

    public static Vector3[] GenerateSmoothNormals(Mesh _srcMesh, bool _convertToTangentSpace)
    {
        Vector3[] vertices = _srcMesh.vertices;
        var groups = vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);
        Vector3[] normals = RegenerateNormals(_srcMesh.triangles,vertices);
        Vector3[] smoothNormals = normals.DeepCopy();
        foreach (var group in groups)
        {
            if (group.Count() == 1)
                continue;
            Vector3 smoothNormal = Vector3.zero;
            foreach (var index in group)
                smoothNormal += normals[index.Value];
            smoothNormal = smoothNormal.normalized;
            foreach (var index in group)
                smoothNormals[index.Value] = smoothNormal;
        }
        if (_convertToTangentSpace)
        {
            Vector4[] tangents = _srcMesh.tangents;
            for (int i = 0; i < smoothNormals.Length; i++)
            {
                Vector3 tangent = tangents[i].ToVector3().normalized;
                Vector3 normal = normals[i].normalized;
                Vector3 biNormal = Vector3.Cross(normal, tangent).normalized * tangents[i].w;
                Matrix3x3 tbnMatrix = Matrix3x3.identity;
                tbnMatrix.SetRow(0, tangent);
                tbnMatrix.SetRow(1, biNormal);
                tbnMatrix.SetRow(2, normal);
                smoothNormals[i] = tbnMatrix * smoothNormals[i].normalized;
            }
        }
        return smoothNormals;
    }
}
