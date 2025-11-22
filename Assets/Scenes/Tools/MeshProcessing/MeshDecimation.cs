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

        private GMesh m_Constructor = new();
        private MeshFilter m_Filter;
        
        private Mesh m_QEMMesh;

        private void OnValidate()
        {
            if (m_SharedMesh == null)
                return;
            
            m_Constructor = new GMesh(m_SharedMesh);
            
            if(m_QEMMesh != null)
                GameObject.DestroyImmediate(m_QEMMesh);
            m_QEMMesh = new Mesh(){name = "Test", hideFlags = HideFlags.HideAndDontSave};
            m_Filter = GetComponent<MeshFilter>();
            if(m_Filter != null)
                m_Filter.sharedMesh = m_QEMMesh;
        }

        [InspectorButton]
        private void Contract(int _edgeIndex)
        {
            m_Constructor.RemoveEdge(_edgeIndex);
            m_Constructor.Populate(m_QEMMesh);
        }

        private void OnDrawGizmos()
        {
            m_Constructor.DrawGizmos();
        }
    }

}