using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using UnityEngine;

namespace Runtime
{
    public class QuadRenderer : ARendererBase
    {
        
        public GQuad m_Quad = GQuad.kDefault;

        protected override void PopulateMesh(Mesh _mesh, Transform _viewTransform)
        {
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var indices = new List<int>();

            m_Quad.FillQuadTriangle(positions, indices, uvs, normals);

            _mesh.SetVertices(positions);
            _mesh.SetNormals(normals);
            _mesh.SetUVs(0, uvs);
            _mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        }
    }
}