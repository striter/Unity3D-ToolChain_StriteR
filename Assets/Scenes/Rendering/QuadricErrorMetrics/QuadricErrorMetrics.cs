using System;
using System.Collections;
using System.Collections.Generic;
using QuadricErrorsMetric;
using UnityEngine;

namespace ExampleScenes.Rendering.QuadricErrorMetrics
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class QuadricErrorMetrics : MonoBehaviour
    {
        [ExtendButton("Optimize",nameof(Optimize))]
        public Mesh m_SharedMesh;
        private MeshFilter m_Filter;
        private QEMConstructor m_Constructor;


        private Mesh m_QEMMesh;
        private void OnValidate()
        {
            m_Filter = GetComponent<MeshFilter>();
            if (!m_Filter||m_SharedMesh == null)
                return;

            m_Constructor = new QEMConstructor(m_SharedMesh);
            
            m_QEMMesh = new Mesh(){name = "Test", hideFlags = HideFlags.HideAndDontSave};
            m_Filter.sharedMesh = m_QEMMesh;
        }

        void Optimize()
        {
            if (m_Constructor==null)
                return;
            
            m_Constructor.DoContract(m_QEMMesh,1);
        }
    }
}