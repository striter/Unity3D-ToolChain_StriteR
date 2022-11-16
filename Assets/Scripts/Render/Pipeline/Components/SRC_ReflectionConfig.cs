using UnityEngine;
using System.Collections.Generic;
using Geometry;

namespace Rendering.Pipeline
{
    [ExecuteInEditMode,RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class SRC_ReflectionConfig : MonoBehaviour
    {
        public EReflectionGeometry m_Geometry = EReflectionGeometry._PLANE;
        [Range(0f, 0.2f)] public float m_NormalDistort = .1f;
        [MFoldout(nameof(m_Geometry),EReflectionGeometry._PLANE)][Range(-5f, 5f)] public float m_PlaneOffset = 0f;
        [MFoldout(nameof(m_Geometry),EReflectionGeometry._PLANE)]public bool m_Upward = true;

        public static List<SRC_ReflectionConfig> m_Reflections { get; private set; } = new List<SRC_ReflectionConfig>();
        public bool Available => m_MeshRenderer.enabled;
        
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

        public GSphere m_SphereData=>new GSphere(transform.position,transform.localScale.x);

        private void OnEnable()
        {
            m_Reflections.Add(this);
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();
        }
        private void OnDisable()
        {
            m_Reflections.Remove(this);
        }

        static readonly int kReflectionTextureOn = Shader.PropertyToID("_CameraReflectionTextureOn");
        static readonly int kReflectionTextureIndex = Shader.PropertyToID("_CameraReflectionTextureIndex");
        static readonly int kReflectionNormalDistort = Shader.PropertyToID("_CameraReflectionNormalDistort");
        public void SetPropertyBlock(MaterialPropertyBlock _block,int _reflectionIndex)
        {
            _block.SetInt(kReflectionTextureOn, 1);
            _block.SetInt(kReflectionTextureIndex,_reflectionIndex);
            _block.SetFloat(kReflectionNormalDistort, m_NormalDistort);
            m_MeshRenderer.SetPropertyBlock(_block);
            #if UNITY_EDITOR
                  m_Index=_reflectionIndex;
            #endif
        }
#if UNITY_EDITOR
        private int m_Index=-1;
        private void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy||!enabled||!m_MeshFilter.sharedMesh)
                return;
            Gizmos.color = IndexToColor(m_Index);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up * m_PlaneOffset, m_MeshFilter.sharedMesh.bounds.size.SetY(0));
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