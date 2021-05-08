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
        [ColorUsage(false)] public Color m_ClearColor = Color.black;
        [Range(0, 1f)] public float m_DitherAmount = 1f;
        public bool m_EnableBlur = false;
        [MFoldout(nameof(m_EnableBlur), true)] public ImageEffectParam_Blurs m_BlurParam = UPipeline.GetDefaultPostProcessData<ImageEffectParam_Blurs>();
        public DistancePlane m_PlaneData => new DistancePlane() { m_Normal = transform.up,m_Distance= UGeometry.PointPlaneDistance( transform.position,transform.up,0) };
        public MeshRenderer m_MeshRenderer { get; private set; }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos_Extend.DrawArrow(m_PlaneData .m_Normal* m_PlaneData.m_Distance, Quaternion.LookRotation(m_PlaneData.m_Normal), 2f, .5f);
        }
    }

}