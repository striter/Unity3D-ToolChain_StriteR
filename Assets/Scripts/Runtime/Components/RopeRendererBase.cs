using System.Collections.Generic;
using Geometry.Curves;

using UnityEngine;

namespace Runtime
{
    
    public enum ERopePosition
    {
        Constant,
        Transform,
    }

    public class RopeRendererBase : ALineRendererBase
    {
        [Clamp(0)] public float m_Extend = 3;
        public ERopePosition m_RopePosition = ERopePosition.Constant;

        [MFoldout(nameof(m_RopePosition), ERopePosition.Transform)] public Transform m_EndTransform;
        
        [MFoldout(nameof(m_RopePosition), ERopePosition.Constant)] public Vector3 m_EndPosition;
        [MFoldout(nameof(m_RopePosition), ERopePosition.Constant,nameof(m_Billboard),false)] public Vector3 m_EndBiTangent;

        public Damper m_ControlDamper;
        private GBezierCurveQuadratic m_Curve;
        private int kRopeInstanceID = 0;
        protected override string GetInstanceName() => $"Rope - {kRopeInstanceID++}";

        protected override void Awake()
        {
            CalculatePositions(out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control);
            m_ControlDamper.Initialize(control);
            base.Awake();
        }
        
        void CalculatePositions(out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control)
        {
            srcPosition = transform.position;
            srcBiTangent = transform.up;
            dstPosition = default;
            dstBiTangent = default; 
            
            switch (m_RopePosition)
            {
                case ERopePosition.Constant: {
                    dstPosition = m_EndPosition;
                    dstBiTangent = m_EndBiTangent;
                } break;
                case ERopePosition.Transform: {
                    if (!m_EndTransform)
                        break;
                    dstPosition = m_EndTransform.position;
                    dstBiTangent = m_EndTransform.up;
                } break;
            }

            control = (dstPosition + srcPosition) / 2 + Vector3.down * m_Extend;
        }


        protected override void PopulatePositions(List<Vector3> _vertices, List<Vector3> _normals)
        {
            CalculatePositions(out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control);
            control = m_ControlDamper.Tick(UTime.deltaTime, control);
            m_Curve = new GBezierCurveQuadratic(srcPosition, dstPosition, control);

            for (int i = 0; i < 64; i++)
            {
                var evaluate = (float)i/63;
                _vertices.Add(m_Curve.Evaluate(evaluate));
                var tangent = m_Curve.EvaluateTangent(evaluate);
                var biTangent = Vector3.Lerp(srcBiTangent,dstBiTangent,evaluate);
                _normals.Add(Vector3.Cross(tangent,biTangent));
            }
        }

    #if UNITY_EDITOR
        public bool m_DrawGizmos;
        private void OnDrawGizmos()
        {
            if (!m_DrawGizmos)
                return;
            
            CalculatePositions(out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_ControlDamper.x,.2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(control,.2f);
            m_Curve.DrawGizmos();
        }

    #endif
    }
}
