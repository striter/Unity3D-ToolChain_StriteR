using System;
using UnityEngine;

namespace Runtime
{
    public abstract class ARuntimeRendererBase
    {
        private Mesh m_Mesh;
        public Mesh Mesh => m_Mesh;
        public virtual Mesh Initialize(Transform _transform)
        {
            m_Mesh = new Mesh {name = GetInstanceName(),hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            return m_Mesh;
        }

        public void Dispose()
        {
            GameObject.DestroyImmediate(m_Mesh);
            m_Mesh = null;
        }
        
        public void PopulateMesh(Transform _transform,Transform _viewTransform)
        {
            if (!m_Mesh) return;
            
            m_Mesh.Clear();
            PopulateMesh(m_Mesh,_transform,_viewTransform);
        }

        protected virtual string GetInstanceName() => "Runtime Mesh";
        protected abstract void PopulateMesh(Mesh _mesh,Transform _transform,Transform _viewTransform);
        public virtual void DrawGizmos(Transform _transform){}
        public virtual bool isBillboard() => false;
    }
    
    [ExecuteInEditMode,RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public abstract class ARuntimeRendererMonoBehaviour<T> : MonoBehaviour where T:ARuntimeRendererBase,new()
    {
        public T meshConstructor = new T();
        public bool m_DrawGizmos;
        
        protected virtual void Awake()
        {
            GetComponent<MeshFilter>().sharedMesh = meshConstructor?.Initialize(transform);
            PopulateMesh();
        }

        Camera GetCurrentCamera()
        {
            if (Application.isPlaying)
                return Camera.main;
            return Camera.current;
        }
        
        public void PopulateMesh()
        {
            if (meshConstructor == null) return;
            var isBillboard = meshConstructor.isBillboard();
            var curCamera = GetCurrentCamera();
            if (isBillboard && !curCamera) return;
            meshConstructor.PopulateMesh(transform,isBillboard ? curCamera.transform : null);
        }
        
        private void OnDestroy()
        {
            meshConstructor?.Dispose();
        }

        protected virtual void Update()
        {
            if (!meshConstructor.isBillboard()) return;
            var curCamera = GetCurrentCamera();
            if (!curCamera) return; 
            if (m_CameraTRChecker.Check(curCamera.transform.localToWorldMatrix))
                meshConstructor.PopulateMesh(transform,curCamera.transform);
        }

        private void OnDrawGizmos()
        {
            if (!m_DrawGizmos) return;
            meshConstructor?.DrawGizmos(transform);
        }
        
        private ValueChecker<Matrix4x4> m_CameraTRChecker = new ValueChecker<Matrix4x4>();

        private void OnValidate()
        {
            if(meshConstructor.isBillboard())
                m_CameraTRChecker.Set(Matrix4x4.identity);
            else
                meshConstructor.PopulateMesh(transform,null);
        }

    }
}