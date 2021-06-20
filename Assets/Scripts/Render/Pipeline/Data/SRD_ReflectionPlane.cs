using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Rendering.Pipeline
{
    using ImageEffect;
    [ExecuteInEditMode,RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class SRD_ReflectionPlane : MonoBehaviour
    {
        public enum_ReflectionSpace m_ReflectionType = enum_ReflectionSpace.ScreenSpace;
        [Range(1, 4)] public int m_DownSample = 1;
        [Range(-5f, 5f)] public float m_PlaneOffset = 0f;
        [Range(0f, 0.2f)] public float m_NormalDistort = .1f;
        [MFoldout(nameof(m_ReflectionType), enum_ReflectionSpace.ScreenSpace)] [Range(1, 4)] public int m_Sample = 1;
        [MFoldout(nameof(m_ReflectionType), enum_ReflectionSpace.MirrorSpace)]  public bool m_IncludeTransparent=false;

        public bool m_EnableBlur = false;
        [MFoldout(nameof(m_EnableBlur), true)] public PPData_Blurs m_BlurParam = UPipeline.GetDefaultPostProcessData<PPData_Blurs>();
        public static List<SRD_ReflectionPlane> m_ReflectionPlanes { get; private set; } = new List<SRD_ReflectionPlane>();
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
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos_Extend.DrawArrow(Vector3.up*m_PlaneOffset,Quaternion.LookRotation(Vector3.up),.5f,.1f);
            Gizmos.DrawWireCube(Vector3.up * m_PlaneOffset, m_MeshFilter.sharedMesh.bounds.size.SetY(0));
        }
#endif
    }

}