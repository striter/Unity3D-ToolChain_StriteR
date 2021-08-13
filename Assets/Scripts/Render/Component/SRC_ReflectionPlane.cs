using UnityEngine;
using System.Collections.Generic;
namespace Rendering.Pipeline
{
    [ExecuteInEditMode,RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class SRC_ReflectionPlane : MonoBehaviour
    {
        [Range(-5f, 5f)] public float m_PlaneOffset = 0f;
        [Range(0f, 0.2f)] public float m_NormalDistort = .1f;

        public static List<SRC_ReflectionPlane> m_ReflectionPlanes { get; private set; } = new List<SRC_ReflectionPlane>();
        public MeshRenderer m_MeshRenderer { get; private set; }
        public MeshFilter m_MeshFilter { get; private set; }
        public GPlane m_PlaneData => new GPlane(transform.up, transform.position + transform.up * m_PlaneOffset);
        private void OnEnable()
        {
            m_ReflectionPlanes.Add(this);
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();
        }
        private void OnDisable()
        {
            m_ReflectionPlanes.Remove(this);
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
                case 0: return Color.white;
                case 1: return Color.blue;
                case 2: return Color.red;
                case 3: return Color.yellow;
            }
        }
#endif
    }

}