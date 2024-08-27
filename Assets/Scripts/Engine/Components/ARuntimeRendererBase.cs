using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

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
        public virtual void OnValidate(){}
        public virtual void DrawGizmos(Transform _transform,Transform _viewTransform){}
        public virtual bool isBillboard() => false;
    }
    
    [ExecuteInEditMode,RequireComponent(typeof(MeshFilter),typeof(MeshRenderer)),DisallowMultipleComponent]
    public abstract class ARuntimeRendererMonoBehaviour<T> : MonoBehaviour where T:ARuntimeRendererBase,new()
    {
        public T meshConstructor = new T();
        private bool m_Dirty;
        public bool m_DrawGizmos;
        
        protected virtual void Awake()
        {
            GetComponent<MeshFilter>().sharedMesh = meshConstructor?.Initialize(transform);
            meshConstructor.OnValidate();
            m_Dirty = true;
        }

        private void OnDestroy()
        {
            meshConstructor?.Dispose();
        }

        private void OnEnable()
        {
            if(meshConstructor.isBillboard())
                RenderPipelineManager.beginCameraRendering += BillboardValidator;
            RenderPipelineManager.beginCameraRendering += BeginRendering;
        }

        private void OnDisable()
        {
            if(meshConstructor.isBillboard())
                RenderPipelineManager.beginCameraRendering -= BillboardValidator;
            RenderPipelineManager.beginCameraRendering -= BeginRendering;
        }

        public void PopulateMesh() => m_Dirty = true;

        private ValueChecker<Matrix4x4> m_CameraTRChecker = new ValueChecker<Matrix4x4>();
        protected void BillboardValidator(ScriptableRenderContext _context, Camera _camera)
        {
            if (m_CameraTRChecker.Check(_camera.transform.localToWorldMatrix))
                PopulateMesh();
        }
        protected void BeginRendering(ScriptableRenderContext _context, Camera _camera)
        {
            if (!m_Dirty) return;
            m_Dirty = false;
            
            meshConstructor.PopulateMesh(transform,_camera.transform);
        }
        
        private void OnDrawGizmos()
        {
            if (!m_DrawGizmos) return;
            meshConstructor?.DrawGizmos(transform,Camera.current.transform);
        }

        public void OnValidate()
        {
            meshConstructor.OnValidate();
            PopulateMesh();
        }

    }
}