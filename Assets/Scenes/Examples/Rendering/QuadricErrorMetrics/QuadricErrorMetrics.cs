using QuadricErrorsMetric;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Examples.Rendering.QuadricErrorMetrics
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class QuadricErrorMetrics : MonoBehaviour
    {
        public Mesh m_SharedMesh;
        public ContractConfigure m_Data;

        private MeshFilter m_Filter;
        private QEMConstructor m_Constructor;

        private Mesh m_QEMMesh;

        [Button]
        void Init()
        {
            m_Filter = GetComponent<MeshFilter>();
            if (!m_Filter||m_SharedMesh == null)
                return;

            if(m_QEMMesh) Object.DestroyImmediate(m_QEMMesh);
            
            m_Constructor = new QEMConstructor(m_SharedMesh);
            m_QEMMesh = new Mesh(){name = "Test", hideFlags = HideFlags.HideAndDontSave};
            m_Filter.sharedMesh = m_QEMMesh;
            m_Constructor.PopulateMesh(m_QEMMesh);
        }
        
        [Button]
        private void Optimize()
        {
            if (m_Constructor == null)
                return;
            
            m_Constructor.Collapse(m_Data);
            m_Constructor.PopulateMesh(m_QEMMesh);
        }

    }
}