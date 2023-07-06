using UnityEngine;

namespace Runtime
{
    [ExecuteInEditMode,RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public abstract class ARuntimeRendererBase : MonoBehaviour
    {
        private Mesh m_Mesh;
        protected virtual void Awake()
        {
            m_Mesh = new Mesh {name = GetInstanceName(),hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
            PopulateMesh(m_Mesh);
        }
        private void OnDestroy()
        {
            GameObject.DestroyImmediate(m_Mesh);
        }

        protected void PopulateMesh()
        {
            m_Mesh.Clear();
            PopulateMesh(m_Mesh);
        }
    
        protected abstract string GetInstanceName();
        protected abstract void PopulateMesh(Mesh _mesh);
    }

}