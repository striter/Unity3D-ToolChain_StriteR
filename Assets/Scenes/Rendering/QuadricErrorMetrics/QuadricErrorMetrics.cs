using System;
using QuadricErrorsMetric;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Examples.Rendering.QuadricErrorMetrics
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class QuadricErrorMetrics : MonoBehaviour
    {
        public Mesh m_SharedMesh;
        [Readonly]public int vertexCount;
        [Readonly]public int trianglesCount;
        public ContractConfigure m_Data;

        private MeshFilter m_Filter;
        private UQuadricErrorMetrics2 m_Constructor = new UQuadricErrorMetrics2();

        private Mesh m_QEMMesh;

        [Button]
        void Init()
        {
            m_Filter = GetComponent<MeshFilter>();
            if (!m_Filter||m_SharedMesh == null)
                return;

            if(m_QEMMesh) Object.DestroyImmediate(m_QEMMesh);
            
            m_QEMMesh = new Mesh(){name = "Test", hideFlags = HideFlags.HideAndDontSave};
            m_Filter.sharedMesh = m_QEMMesh;
            m_Constructor.Init(m_SharedMesh);
            m_Constructor.PopulateMesh(m_QEMMesh);
        }

        [Button]
        private void Optimize()
        {
            m_Constructor.simplify_mesh(500); // m_Constructor.Collapse(m_Data);
            m_Constructor.PopulateMesh(m_QEMMesh);
        }

        public bool m_DrawGizmos;
        private void OnDrawGizmos()
        {
            if (m_Constructor == null)
                return;

            Gizmos.matrix = transform.localToWorldMatrix;
            vertexCount = m_Constructor.vertices.Count;
            trianglesCount = m_Constructor.triangles.Count;
            if(m_DrawGizmos)
                m_Constructor.DrawGizmos();
        }
    }
}