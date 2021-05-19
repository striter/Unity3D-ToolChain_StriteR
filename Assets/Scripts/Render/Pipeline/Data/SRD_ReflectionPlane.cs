using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.Pipeline
{
    using ImageEffect;
    [ExecuteInEditMode,RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class SRD_ReflectionPlane : MonoBehaviour
    {
        public MeshRenderer m_MeshRenderer { get; private set; }
        public MeshFilter m_MeshFilter { get; private set; }
        public static List<SRD_ReflectionPlane> m_ReflectionPlanes { get; private set; } = new List<SRD_ReflectionPlane>();
        public enum_ReflectionSpace m_ReflectionType = enum_ReflectionSpace.ScreenSpace;
        [Range(-5f, 5f)] public float m_PlaneOffset = 0f;
        [MFoldout(nameof(m_ReflectionType), enum_ReflectionSpace.ScreenSpace)] [Range(1, 4)] public int m_Sample = 1;
        public bool m_EnableBlur = false;
        [MFoldout(nameof(m_EnableBlur), true)] public ImageEffectParam_Blurs m_BlurParam = UPipeline.GetDefaultPostProcessData<ImageEffectParam_Blurs>();
        public GPlane m_PlaneData => new GPlane() { m_Normal = transform.up, m_Distance = UGeometry.PointPlaneDistance(transform.position, transform.up, 0) + m_PlaneOffset };
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