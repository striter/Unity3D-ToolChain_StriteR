using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Scripting;
using UnityEngine;

namespace Runtime
{
    public class QuadRenderer : ARendererBase
    {
        
        public GQuad m_Quad = GQuad.kDefault;
        protected override void PopulateMesh(Mesh _mesh, Transform _viewTransform)
        {
            ListPool<Vector3>.ISpawn(out var vertices);
            ListPool<Vector3>.ISpawn(out var normals);
            ListPool<Vector2>.ISpawn(out var uvs);
            ListPool<int>.ISpawn(out var indexes);

            m_Quad.PopulateVertex(vertices, indexes, uvs, normals);

            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetUVs(0, uvs);
            _mesh.SetIndices(indexes, MeshTopology.Triangles, 0);
            
            ListPool<Vector3>.IDespawn(vertices);
            ListPool<Vector3>.IDespawn(normals);
            ListPool<Vector2>.IDespawn(uvs);
            ListPool<int>.IDespawn(indexes);
        }
    }
}