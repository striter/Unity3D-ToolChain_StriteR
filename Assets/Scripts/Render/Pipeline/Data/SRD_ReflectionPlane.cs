using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.Pipeline
{
    using ImageEffect;
    [ExecuteInEditMode,RequireComponent(typeof(MeshRenderer))]
    public class SRD_ReflectionPlane : MonoBehaviour
    {
        public static List<SRD_ReflectionPlane> m_ReflectionPlanes { get; private set; } = new List<SRD_ReflectionPlane>();
        private void OnEnable()
        {
            m_ReflectionPlanes.Add(this);
            m_MeshRenderer = GetComponent<MeshRenderer>();
        }
        private void OnDisable()
        {
            m_ReflectionPlanes.Remove(this);
        }
        public enum_ReflectionSpace m_ReflectionType = enum_ReflectionSpace.ScreenSpace;
        [Range(-5f,5f)]public float m_PlaneOffset = 0f;
        [MFoldout(nameof(m_ReflectionType),enum_ReflectionSpace.ScreenSpace)] [Range(1,4)]public int m_Sample=1;
        public bool m_EnableBlur = false;
        [MFoldout(nameof(m_EnableBlur), true)] public ImageEffectParam_Blurs m_BlurParam = UPipeline.GetDefaultPostProcessData<ImageEffectParam_Blurs>();
        public DistancePlane m_PlaneData => new DistancePlane() { m_Normal = transform.up,m_Distance= UGeometry.PointPlaneDistance( transform.position,transform.up,0)+ m_PlaneOffset };
        public MeshRenderer m_MeshRenderer { get; private set; }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            DistancePlane planeData = m_PlaneData;
            Gizmos_Extend.DrawArrow(transform.position+transform.up * m_PlaneOffset, Quaternion.LookRotation(planeData.m_Normal), 2f, .5f);
        }
#endif
    }

}