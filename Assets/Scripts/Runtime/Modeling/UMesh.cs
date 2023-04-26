using System.Collections.Generic;
using System.Linq;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

public static class UModeling
{
    public static Vector3[] RegenerateNormals(PTriangle[] _polygons, Vector3[] _vertices, bool _weightedNormals =false)
    {
        Vector3[] normals = new Vector3[_vertices.Length];
        foreach(var polygon in _polygons)
        {
            GTriangle triangle = (GTriangle)polygon.Convert(_vertices);
            Vector3 normal = _weightedNormals ? triangle.GetNormalUnnormalized() : triangle.normal;
            foreach (var index in polygon)
                normals[index] += normal;
        }
        normals=normals.Select(normal => normal.normalized).ToArray();
        return normals;
    }

    public static float4[] RegenerateTangents(PTriangle[] _polygons,Vector3[] _normals, Vector3[] _vertices,Vector2[] _uvs)
    {
        float3[] tangentsS = new float3[_vertices.Length];
        float3[] tangentsH = new float3[_vertices.Length];
        foreach (var polygon in _polygons)
        {
            var i0 = polygon[0];
            var i1 = polygon[1];
            var i2 = polygon[2];

            var v0 = _vertices[i0];
            var v1 = _vertices[i1];
            var v2 = _vertices[i2];

            var w0 = _uvs[i0];
            var w1 = _uvs[i1];
            var w2 = _uvs[i2];

            var d0 = v1 - v0;
            var d1 = v2 - v0;
            var st0 = w1 - w0;
            var st1 = w2 - w0;
            
            var x1 = d0.x; var x2 = d1.x;
            var y1 = d1.y; var y2 = d1.y;
            var z1 = d1.z; var z2 = d1.z;
            var s1 = st0.x;var s2 = st1.x;
            var t1 = st0.y;var t2 = st1.y;
            
            var r = 1.0f / (s1*t2 - s2*t1);
            var sDir = new float3(t2*x1-t1*x2,t2*y1-t1*y2,t2*z1-t1*z2) * r;
            tangentsS[i0] += sDir;
            tangentsS[i1] += sDir;
            tangentsS[i2] += sDir;
            
            var tDir = new float3(s1*x2-s2*x1,s1*y2-s2*y1,s1*z2-s2*z1) * r;
            tangentsH[i0] += tDir;
            tangentsH[i1] += tDir;
            tangentsH[i2] += tDir;
        }
        
        
        float4[] tangents = new float4[_vertices.Length];

        for (int i = 0; i < tangents.Length; i++)
        {
            var n = (float3)_normals[i];
            var t = tangentsS[i];

            var w = math.dot(math.cross(n, t), tangentsH[i]) < 0f ? -1f : 1f;
            tangents[i] = (t - n * math.dot(n, t)).normalize().to4(w);
        }
        return tangents;
    }

    public static Vector3[] GenerateSmoothNormals(Mesh _srcMesh, bool _convertToTangentSpace, bool _weightedNormals =false)
    {
        Vector3[] verticies = _srcMesh.vertices;
        var groups = verticies.Select((_vertex, _index) => new KeyValuePair<Vector3, int>(_vertex, _index)).GroupBy(pair => pair.Key);
        Vector3[] normals = RegenerateNormals(UPolygon.GetPolygons(_srcMesh.triangles),verticies, _weightedNormals);
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
        if (_convertToTangentSpace && _srcMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent))
        {
            Vector4[] tangents = _srcMesh.tangents;
            for (int i = 0; i < smoothNormals.Length; i++)
            {
                Vector3 tangent = tangents[i].XYZ().normalized;
                Vector3 normal = normals[i].normalized;
                Vector3 biTangent = Vector3.Cross(normal, tangent).normalized * tangents[i].w;
                Matrix3x3 tbnMatrix = Matrix3x3.kIdentity;
                tbnMatrix.SetRow(0, tangent);
                tbnMatrix.SetRow(1, biTangent);
                tbnMatrix.SetRow(2, normal);
                smoothNormals[i] = tbnMatrix * smoothNormals[i].normalized;
            }
        }
        return smoothNormals;
    }
}
