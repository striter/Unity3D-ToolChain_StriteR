using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.Index;
using Geometry.Voxel;
using UnityEngine;

namespace TEditor
{
    public static class UERender
    {
        static Vector3[] RenegerateNormals(int[] _indices, Vector3[] _verticies)
        {
            Vector3[] normals = new Vector3[_verticies.Length];
            GTriangleIndex[] polygons = URender.GetPolygons(_indices);
            foreach(var polygon in polygons)
            {
                GTriangle triangle = new GTriangle(polygon.GetVertices(_verticies));
                Vector3 normal = triangle.normal;
                foreach (var index in polygon)
                    normals[index] += normal;
            }
            normals=normals.Select(normal => normal.normalized).ToArray();
            return normals;
        }

        public static Vector3[] GenerateSmoothNormals(Mesh _srcMesh, bool _convertToTangentSpace)
        {
            Vector3[] verticies = _srcMesh.vertices;
            var groups = verticies.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);
            Vector3[] normals = RenegerateNormals(_srcMesh.triangles,verticies);
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

}