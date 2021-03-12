using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TEditor
{
    public static class EURender
    {
        public enum enum_Editor_MeshColor
        {
            None,
            RGBA,
            R,
            G,
            B,
            A,
        }
        public enum enum_Editor_MeshUV
        {
            None = -1,
            UV0,
            UV1,
            UV2,
            UV3,
            UV4,
            UV5,
            UV6,
            UV7,
        }

        public static Vector3[] GenerateSmoothNormals(Mesh _srcMesh, bool _convertToTangentSpace)
        {
            var groups = _srcMesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);
            Vector3[] normals = _srcMesh.normals;
            Vector3[] smoothNormals = normals.Copy();
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