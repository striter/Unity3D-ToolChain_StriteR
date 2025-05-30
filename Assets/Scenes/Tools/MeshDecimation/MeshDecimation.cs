using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Examples.Rendering.MeshDecimation
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class MeshDecimation : MonoBehaviour
    {
        public Mesh m_SharedMesh;
        [Readonly]public int vertexCount;
        [Readonly]public int trianglesCount;

        private MeshDecimationCore m_Constructor = new();
        private MeshFilter m_Filter;
        
        private Mesh m_QEMMesh;

        [InspectorButton]
        void Init()
        {
            m_Filter = GetComponent<MeshFilter>();
            if (!m_Filter||m_SharedMesh == null)
                return;

            if(m_QEMMesh) Object.DestroyImmediate(m_QEMMesh);
            
            m_QEMMesh = new Mesh(){name = "Test", hideFlags = HideFlags.HideAndDontSave};
            m_Filter.sharedMesh = m_QEMMesh;
            m_Constructor.Initialize(m_SharedMesh);
            m_Constructor.PopulateMesh(m_QEMMesh);
        }

        [InspectorButton]
        private void Contract(int _edgeIndex)
        {
            m_Constructor.Collapse(_edgeIndex);
            m_Constructor.PopulateMesh(m_QEMMesh);
        }

        public enum EGizmosMode
        {
            None,
            Vertex,
            Triangle,
            Edge
        }
        
        public EGizmosMode m_GizmosMode = EGizmosMode.None;
        private void OnDrawGizmos()
        {
            if (m_Constructor == null || m_Constructor.vertices.Count == 0)
                return;

            Gizmos.matrix = transform.localToWorldMatrix;
            vertexCount = m_Constructor.vertices.Count;
            trianglesCount = m_Constructor.triangles.Count;
            switch (m_GizmosMode)
            {
                case EGizmosMode.Vertex:
                    foreach (var point in m_Constructor.vertices)
                        Gizmos.DrawSphere(point, .02f);
                    break;
                case EGizmosMode.Triangle:
                {
                    var index = 0;
                    foreach (var triangle in m_Constructor.triangles)
                    {
                        Gizmos.color = UColor.IndexToColor(index++);
                        var gTriangle = GTriangle.Convert(triangle, p=>m_Constructor.vertices[p]);
                        UGizmos.DrawLinesConcat(gTriangle.shrink(0.9f));
                    }
                    
                }
                    break;
                case EGizmosMode.Edge:
                {
                    var index = 0;
                    foreach (var edge in m_Constructor.edges)
                    {
                        if (index++ == 0)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawSphere(m_Constructor.vertices[edge.start],.03f);
                            Gizmos.color = Color.green;
                            Gizmos.DrawSphere(m_Constructor.vertices[edge.end],.03f);
                        }
                        
                        Gizmos.color = Color.white.SetA(.5f);
                        UGizmos.DrawLines(edge, _p => m_Constructor.vertices[_p]);
                    }
                    
                }
                    break;
            }
        }
    }

    public class MeshDecimationCore
    {
        public List<Vector3> vertices { get; private set; } = new();
        public List<PTriangle> triangles { get; private set; } = new();
        public List<PLine> edges { get; private set; } = new();
        public void Initialize(Mesh mesh)
        {
            mesh.GetVertices(vertices);
            mesh.GetPolygons(out var indexes).FillList(triangles);
            triangles.GetDistinctEdges().FillList(edges);
        }

        public void PopulateMesh(Mesh _mesh)
        {
            _mesh.Clear();
            _mesh.SetVertices(vertices.Select(p => (Vector3)p).ToList());
            _mesh.SetTriangles(triangles.Select(p=>(IEnumerable<int>)p).Resolve().ToArray(), 0);
        }

        public void Collapse(int _edgeIndex)
        {
            var edge = edges[_edgeIndex];
            var edgeStart = edge.start;
            var edgeEnd = edge.end;
            // vertices[edgeStart] = (vertices[edgeStart] + vertices[edgeEnd]) / 2;
            vertices.RemoveAt(edgeEnd);
            
            for (var i = triangles.Count - 1; i >= 0; i--)
            {
                var triangle = triangles[i];
                var matchCount = 0;
                for (var j = 0; j < 3; j++)
                {
                    var index = triangle[j];
                    if (index == edgeStart || index == edgeEnd) 
                        matchCount++;
                    
                    if(index == edgeEnd) index = edgeStart;
                    else if(index > edgeEnd) index -= 1;
                    triangle[j] = index;
                }

                if (matchCount == 2)
                {
                    triangles.RemoveAt(i);
                    continue;
                }

                triangles[i] = triangle;
            }

            triangles.GetDistinctEdges().FillList(edges);
        }
        
    }
}