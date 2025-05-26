using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using UnityEngine;

namespace Runtime
{
    public class QuadRenderer : ARendererBase
    {
        public GQuad m_Quad = GQuad.kDefault;
        protected override void PopulateMesh(Mesh _mesh, Transform _viewTransform)
        {
            PoolList<Vector3>.ISpawn(out var vertices);
            PoolList<Vector3>.ISpawn(out var normals);
            PoolList<Vector2>.ISpawn(out var uvs);
            PoolList<int>.ISpawn(out var indexes);

            m_Quad.PopulateVertex(vertices, indexes, uvs, normals);

            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetUVs(0, uvs);
            _mesh.SetIndices(indexes, MeshTopology.Triangles, 0);
            
            PoolList<Vector3>.IDespawn(vertices);
            PoolList<Vector3>.IDespawn(normals);
            PoolList<Vector2>.IDespawn(uvs);
            PoolList<int>.IDespawn(indexes);
        }
    }
}