using UnityEngine;
using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;

namespace Rendering.Pipeline
{
    [ExecuteInEditMode,RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class SRC_ReflectionBehaviour : MonoBehaviour
    {
        [Range(-5f, 5f)] public float m_PlaneOffset = 0f;
        [Range(0f, 0.2f)] public float m_NormalDistort = .1f;

        public static List<SRC_ReflectionBehaviour> m_Reflections { get; private set; } = new List<SRC_ReflectionBehaviour>();
        public bool Available => m_MeshRenderer.enabled;
        
        private MeshRenderer m_MeshRenderer;
        private MeshFilter m_MeshFilter;
        public GPlane m_PlaneData => new GPlane(transform.up,  transform.position + transform.up*m_PlaneOffset);
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

        static readonly int ID_ReflectionTextureOn = Shader.PropertyToID("_CameraReflectionTextureOn");
        static readonly int ID_ReflectionTextureIndex = Shader.PropertyToID("_CameraReflectionTextureIndex");
        static readonly int ID_ReflectionNormalDistort = Shader.PropertyToID("_CameraReflectionNormalDistort");
        public void SetPropertyBlock(MaterialPropertyBlock _block,int _reflectionIndex)
        {
            _block.SetInt(ID_ReflectionTextureOn, 1);
            _block.SetInt(ID_ReflectionTextureIndex,_reflectionIndex);
            _block.SetFloat(ID_ReflectionNormalDistort, m_NormalDistort);
            m_MeshRenderer.SetPropertyBlock(_block);
            #if UNITY_EDITOR
                  m_Index=_reflectionIndex;
            #endif
        }
#if UNITY_EDITOR
        private int m_Index=-1;
        public void EditorApplyIndex(int _index) => m_Index = _index;
        private void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy||!enabled)
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