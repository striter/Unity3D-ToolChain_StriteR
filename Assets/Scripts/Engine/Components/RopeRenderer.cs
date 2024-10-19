using System;
using System.Collections.Generic;
using Runtime.Geometry.Curves;
using UnityEngine;

namespace Runtime
{
    
    public enum ERopePosition
    {
        Constant,
        Transform,
    }

    [Serializable]
    public class FRopeRenderer : ALineRendererBase
    { 
        [Clamp(0)] public float m_Extend = 3;
        [Range(4, 64)] public int m_Resolution;
        public ERopePosition m_RopePosition = ERopePosition.Constant;

        [MFoldout(nameof(m_RopePosition), ERopePosition.Transform)] public Transform m_EndTransform;
        [MFoldout(nameof(m_RopePosition), ERopePosition.Constant)] public Vector3 m_EndPosition;
        [MFoldout(nameof(m_RopePosition), ERopePosition.Constant,nameof(m_Billboard),false)] public Vector3 m_EndBiTangent;
        public Damper m_ControlDamper = new Damper();
        
        private GBezierCurveQuadratic m_Curve;
        public override Mesh Initialize(Transform _transform)
        {
            Reset(_transform);
            return base.Initialize(_transform);
        }

        void CalculatePositions(Transform _transform,out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control)
        {
            srcPosition = _transform.position;
            srcBiTangent = _transform.up;
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

        public void Reset(Transform _transform)
        {
            CalculatePositions(_transform,out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control);
            m_ControlDamper.Initialize(control);
        }

        public override void Tick(Transform _transform,float _deltaTime)
        {
            base.Tick(_transform,_deltaTime);
            CalculatePositions(_transform,out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control);
            m_ControlDamper.Tick(UTime.deltaTime, control);
        }

        protected override void PopulatePositions(Transform _transform, List<Vector3> _vertices, List<Vector3> _tangents)
        {
            CalculatePositions(_transform,out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control);
            m_Curve = new GBezierCurveQuadratic(srcPosition, dstPosition, m_ControlDamper.value.xyz);
            for (var i = 0; i < m_Resolution; i++)
            {
                var evaluate = (float)i/(m_Resolution-1);
                _vertices.Add(m_Curve.Evaluate(evaluate));
                var tangent = m_Curve.EvaluateTangent(evaluate);
                var biTangent = Vector3.Lerp(srcBiTangent,dstBiTangent,evaluate);
                _tangents.Add(Vector3.Cross(tangent,biTangent));
            }
        }

#if UNITY_EDITOR
        public override void DrawGizmos(Transform _transform,Transform _viewTransform)
        {
            base.DrawGizmos(_transform,_viewTransform);
            CalculatePositions(_transform,out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_ControlDamper.value.xyz,.2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(control,.2f);
            m_Curve.DrawGizmos();
        }
#endif
    }
    
    public class RopeRenderer : ARuntimeRendererMonoBehaviour<FRopeRenderer>
    {
        public void Initialize() => meshConstructor.Reset(transform);
    }
    
}
