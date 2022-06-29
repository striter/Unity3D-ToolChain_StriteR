using System;
using System.Collections;
using System.Collections.Generic;
using QuadricErrorsMetric;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ExampleScenes.Rendering.QuadricErrorMetrics
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class QuadricErrorMetrics : MonoBehaviour
    {
        public Mesh m_SharedMesh;
        [ExtendButton("Initialize",nameof(Init),null,
            "Optimize",nameof(Optimize),null )]
        public ContractConfigure m_Data;

        private MeshFilter m_Filter;
        private QEMConstructor m_Constructor;

        private Mesh m_QEMMesh;

        void Init()
        {
            m_Filter = GetComponent<MeshFilter>();
            if (!m_Filter||m_SharedMesh == null)
                return;

            m_Constructor = new QEMConstructor(m_SharedMesh);
            if(m_QEMMesh)
                Object.DestroyImmediate(m_QEMMesh);
        }
        private void Optimize()
        {
            if (m_Constructor == null)
                return;
            
            m_QEMMesh = new Mesh(){name = "Test", hideFlags = HideFlags.HideAndDontSave};
            m_Filter.sharedMesh = m_QEMMesh;
            
            m_Constructor.DoContract(m_QEMMesh,m_Data);
        }

        private void OnDrawGizmos()
        {
            if (m_Constructor == null)
                return;

            Gizmos.matrix = transform.localToWorldMatrix;
            
            int count = m_Constructor.vertices.Count;
            for (int i = 0; i < count; i++)
            {
                var vertex = m_Constructor.vertices[i];
                var qemVertex = m_Constructor.qemVertices[i];
                Gizmos.DrawSphere(vertex,.01f / Gizmos.matrix.lossyScale.x);
            }
        }
    }
}