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
        [ExtendButton("Optimize",nameof(Optimize),null,
            "Reset",nameof(OnValidate),null
        )]
        public Mesh m_SharedMesh;

        public int m_OptimizeCount = 5;
        private MeshFilter m_Filter;
        private QEMConstructor m_Constructor;

        private Mesh m_QEMMesh;
        private void OnValidate()
        {
            m_Filter = GetComponent<MeshFilter>();
            if (!m_Filter||m_SharedMesh == null)
                return;

            m_Constructor = new QEMConstructor(m_SharedMesh);
            if(m_QEMMesh)
                Object.Destroy(m_SharedMesh);
            
            m_QEMMesh = new Mesh(){name = "Test", hideFlags = HideFlags.HideAndDontSave};
            m_Filter.sharedMesh = m_QEMMesh;
        }
        
        void Optimize()
        {
            if (m_Constructor==null)
                return;
            
            m_Constructor.DoContract(m_QEMMesh,m_OptimizeCount);
        }
    }
}