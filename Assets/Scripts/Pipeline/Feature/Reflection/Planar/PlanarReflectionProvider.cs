using System;
using UnityEngine;
using System.Collections.Generic;
using Runtime.Geometry;

namespace Rendering.Pipeline
{
    [ExecuteInEditMode,RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class PlanarReflectionProvider : MonoBehaviour
    {
        [Header("Shape")]
        public EPlanarReflectionGeometry m_Geometry = EPlanarReflectionGeometry._PLANE;
        [Range(0f, 0.2f)] public float m_NormalDistort = .1f;
        [Foldout(nameof(m_Geometry),EPlanarReflectionGeometry._PLANE)][Range(-5f, 5f)] public float m_PlaneOffset = 0f;
        [Foldout(nameof(m_Geometry),EPlanarReflectionGeometry._PLANE)]public bool m_Upward = true;
        public PlanarReflectionData m_Data = PlanarReflectionData.kDefault;
        
        public static List<PlanarReflectionProvider> m_Reflections { get; private set; } = new List<PlanarReflectionProvider>();
        
        private MeshRenderer m_MeshRenderer;
        private MeshFilter m_MeshFilter;

        public GPlane m_PlaneData
        {
            get
            {
                var upward = m_Upward ? transform.up : transform.forward;
                return new GPlane(upward,transform.position + upward * m_PlaneOffset);
            }
        }

        public GSphere m_SphereData => new GSphere(transform.position,transform.localScale.x);

        private MaterialPropertyBlock m_PropertyBlock;
        private int m_Index=-1;
        private void OnEnable()
        {
            m_Reflections.Add(this);
        }
        private void OnDisable()
        {
            m_Reflections.Remove(this);
            m_Index = -1;
            OnValidate();
        }

        static readonly int kReflectionTextureOn = Shader.PropertyToID("_CameraReflectionTextureOn");
        static readonly int kReflectionTextureIndex = Shader.PropertyToID("_CameraReflectionTextureIndex");
        static readonly int kReflectionNormalDistort = Shader.PropertyToID("_CameraReflectionNormalDistort");
        public void ApplyIndex(int _reflectionIndex)
        {
            if (m_Index == _reflectionIndex) 
                return;
            m_Index=_reflectionIndex;
            OnValidate();
        }

        private void OnValidate()
        {
            m_MeshFilter??= GetComponent<MeshFilter>();
            m_MeshRenderer??= GetComponent<MeshRenderer>();
            m_PropertyBlock??= new MaterialPropertyBlock();
            m_PropertyBlock.SetInt(kReflectionTextureOn, m_Index != -1 ? 1 : 0);
            m_PropertyBlock.SetInt(kReflectionTextureIndex,m_Index);
            m_PropertyBlock.SetFloat(kReflectionNormalDistort, m_NormalDistort);
            m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy||!enabled||!m_MeshFilter.sharedMesh)
                return;
            Gizmos.color = IndexToColor(m_Index);
            switch (m_Geometry)
            {
                case EPlanarReflectionGeometry._PLANE:
                    m_PlaneData.DrawGizmos();
                    break;
                case EPlanarReflectionGeometry._SPHERE:
                    Gizmos.DrawWireSphere(m_SphereData.center,m_SphereData.radius);
                    break;
            }
        }
        private Color IndexToColor(int index)
        {
            switch (index)
            {
                default: return Color.magenta;
                case 0: return Color.green;
                case 1: return Color.blue;
                case 2: return Color.red;
                case 3: return Color.yellow;
                case 4: return Color.white;
            }
        }
#endif
    }

}