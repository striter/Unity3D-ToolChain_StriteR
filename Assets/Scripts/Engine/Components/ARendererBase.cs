using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Runtime
{
    public interface IRuntimeRendererBillboard
    {
        bool Billboard { get; }
    }
    
    [ExecuteInEditMode,RequireComponent(typeof(MeshFilter),typeof(MeshRenderer)),DisallowMultipleComponent]
    public abstract class ARendererBase : MonoBehaviour
    {
        private bool m_Dirty;
        private Dictionary<Type,int> kInstanceID = new Dictionary<Type, int>();
        private static int kInstanceCount = 0;
        private Mesh m_Mesh;
        private void Awake()
        {
            var type = GetType();
            if(!kInstanceID.TryGetValue(type,out var count))
                kInstanceID.Add(type,0);
            kInstanceID[type] = count+1;
            
            m_Mesh = new Mesh {name = $"{type.Name} {count}",hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
            
            OnInitialize();
            OnValidate();
            m_Dirty = true;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            GameObject.DestroyImmediate(m_Mesh);
            OnDispose();
            m_Mesh = null;
        }

        private void OnEnable() => RenderPipelineManager.beginCameraRendering += BeginRendering;

        private void OnDisable() => RenderPipelineManager.beginCameraRendering -= BeginRendering;
        
        public void SetDirty() => m_Dirty = true;

        private ValueChecker<Matrix4x4> m_CameraTRChecker = new ValueChecker<Matrix4x4>();
        protected void BillboardValidator(ScriptableRenderContext _context, Camera _camera)
        {
        }
        protected void BeginRendering(ScriptableRenderContext _context, Camera _camera)
        {
            if (!m_Mesh) 
                return;


            if (this is IRuntimeRendererBillboard { Billboard: true })
            {
                if (m_CameraTRChecker.Check(_camera.transform.localToWorldMatrix))
                    SetDirty();
            }
            
            if (!m_Dirty) return;
            m_Dirty = false;
            
            m_Mesh.Clear();
            PopulateMesh(m_Mesh,_camera.transform);
        }

        public void OnValidate()
        {
            Validate();
            SetDirty();
        }

        protected abstract void PopulateMesh(Mesh _mesh,Transform _viewTransform);
        protected virtual void OnInitialize(){}
        protected virtual void OnDispose(){}
        protected virtual void Tick(float _deltaTime){}
        protected virtual void Validate() {}
        
        private bool m_DrawGizmos;
        [InspectorFoldoutButton(nameof(m_DrawGizmos),false)] public void DrawGizmos() => m_DrawGizmos = !m_DrawGizmos;
        private void OnDrawGizmos()
        {
            if (!m_DrawGizmos) return;
            DrawGizmos(Camera.current.transform);
        }
        public virtual void DrawGizmos(Transform _viewTransform){}

    }
}