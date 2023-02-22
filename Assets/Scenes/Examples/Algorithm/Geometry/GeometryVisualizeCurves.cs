using Geometry;
using Geometry.Curves;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeCurves : MonoBehaviour
    {
        [Header("Bezeir")]
        public GBezierCurveQuadratic m_QuadraticCurve;
        public bool quadraticTangents = false;
        public GBezierCurveCubic m_CubicCurve;
        [Header("Spline")]
        public GSpline m_BSpline = GSpline.kDefault;
        public GBezierSplineUniform m_BSplineUniform = GBezierSplineUniform.kDefault;
        
        [Header("Curves")]
        public GChaikinCurve m_ChaikinCurve = GChaikinCurve.kDefault;
        public GDivisionCurve m_DivisionCurve = GDivisionCurve.kDefault;
        

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //Bezeir Curves
            var localToWorldMatrix = transform.localToWorldMatrix;
            Gizmos.matrix = localToWorldMatrix;
            m_QuadraticCurve.DrawGizmos(quadraticTangents);
            Gizmos.color = Color.grey;
            m_QuadraticCurve.GetBoundingBox().DrawGizmos();
            
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(5f,0f,0f));
            m_CubicCurve.DrawGizmos();
            Gizmos.color = Color.grey;
            m_CubicCurve.GetBoundingBox().DrawGizmos();
            
            //BSpline
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(0f,0f,-3f));
            m_BSpline.DrawGizmos(true);
            
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(5f,0f,-3f));
            m_BSplineUniform.DrawGizmos(true);
            
            //Curves
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(0f,0f,-6f));
            m_ChaikinCurve.DrawGizmos();
            
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(5f,0f,-6f));
            m_DivisionCurve.DrawGizmos();
        }
#endif
    }
}