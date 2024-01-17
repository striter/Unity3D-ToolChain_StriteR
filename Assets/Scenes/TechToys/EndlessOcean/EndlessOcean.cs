using System.Collections.Generic;
using System.Linq;
using Runtime.DataStructure;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace TechToys.EndlessOcean
{
    [ExecuteInEditMode]
    public class EndlessOcean : MonoBehaviour
    {
        public float range = 20;
        public int size = 3;
        public int split = 20;

        private Camera m_Camera;
        private Mesh m_Mesh;
        private MeshFilter m_Filter;
        private QuadTree_float2 m_QuadTree = new QuadTree_float2(3);

        private float3x2_homogenous kQuadTreeMatrix;

        private void Awake()
        {
            m_Camera = transform.Find("Camera").GetComponent<Camera>();
            m_Mesh = new Mesh() {name = "Dynamic", hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            m_Filter = GetComponentInChildren<MeshFilter>();
            m_Filter.sharedMesh = m_Mesh;

            RefreshMesh(m_Camera.transform.position);
        }

        private void OnDestroy()
        {
            GameObject.DestroyImmediate(m_Mesh);
            m_Filter.sharedMesh = null;
            GetComponentInChildren<MeshFilter>().sharedMesh = null;
        }

        public G2Box GetCurrentBoundary(float3 _cameraPos,out float3x2_homogenous quadMatrix)
        {
            var center = math.round(_cameraPos.xz / range);
            quadMatrix = float3x2_homogenous.TRS(center * range,45 * kmath.kDeg2Rad,1 );
            return new G2Box(0,range * math.pow(3,size));
        }
        

        // Update is called once per frame
        void Update()
        {
            RefreshMesh(m_Camera.transform.position);
        }

        void RefreshMesh(float3 _position)
        {
            m_QuadTree.ConstructGeometry(GetCurrentBoundary(_position,out kQuadTreeMatrix),size);
            m_Mesh.Clear();
            List<int> indices = new List<int>();
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector4> tangents = new List<Vector4>();

            int quadStartIndex = 0;
            foreach (var node in m_QuadTree)
            {
                for (int j = 0; j <= split; j++)
                {
                    for (int i = 0; i <= split; i++)
                    {
                        positions.Add(kQuadTreeMatrix.mulPosition(node.boundary.GetPoint(new float2(i, j) / split)).to3xz());
                        normals.Add(kfloat3.up);
                        tangents.Add(kfloat3.right.to4(1));
                    }
                }

                int vertexCountPerColumn = split + 1;
                for (int j = 0; j < split; j++)
                {
                    for (int i = 0; i < split; i++)
                    {
                        PQuad quad = new PQuad(
                            i + j * vertexCountPerColumn ,
                            i + (j + 1) * vertexCountPerColumn,
                            (i + 1) + (j + 1) * vertexCountPerColumn,
                            (i + 1) + j * vertexCountPerColumn
                        ) + quadStartIndex;
                        indices.AddRange(UMesh.kQuadToTriangles.Select(p=>quad[p]));
                    }
                }

                quadStartIndex = positions.Count;
            }

            m_Mesh.SetVertices(positions);
            m_Mesh.SetNormals(normals);
            m_Mesh.SetTangents(tangents);
            m_Mesh.SetIndices(indices,MeshTopology.Triangles,0,false);
        }
        
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = kQuadTreeMatrix.ToMatrix4x4XZ();
            m_QuadTree.DrawGizmos();
        }
    }

}