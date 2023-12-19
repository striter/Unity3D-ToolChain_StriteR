using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Curves;
using Geometry.Curves.Spline;
using Geometry.Curves.LineSegments;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeCurves : MonoBehaviour
    {
        [Header("Curves")]
        public GBezierCurveQuadratic m_QuadraticCurve;
        public bool m_QuadraticTangents = false;
        public GBezierCurveCubic m_CubicCurve;
        public GPolynomialCurve m_PolynomialCurve;
        [Range(0,1)]public float m_BezeirSplit = 0;
        public GHermiteCurve m_HermiteCurve = GHermiteCurve.kDefault;
        public GProjectileCurve m_ProjectileCurve = GProjectileCurve.kDefault;
        
        [Header("Spline")]
        public GFourierSpline m_FourierSpline = GFourierSpline.kBunny;
        public GHermiteSpline m_HermineSpline = GHermiteSpline.kDefault;
        public GCatmullRomSpline m_CatmullRomSpline = GCatmullRomSpline.kDefault;
        public GSpline m_BSpline = GSpline.kDefault;
        public GBezierSplineUniform m_BSplineUniform = GBezierSplineUniform.kDefault;
        
        [Header("Line Segments")]
        public GDivisionCurve m_DivisionCurve = GDivisionCurve.kDefault;
        public GChaikinCurve m_ChaikinCurve = GChaikinCurve.kDefault;
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //Bezeir Curves
            var localToWorldMatrix = transform.localToWorldMatrix;
            Gizmos.matrix = localToWorldMatrix;
            m_QuadraticCurve.DrawGizmos(m_QuadraticTangents);
            Gizmos.color = Color.grey;
            m_QuadraticCurve.GetBoundingBox().DrawGizmos();
            
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(5f,0f,0f));
            if (m_BezeirSplit > 0)
            {
                m_CubicCurve.Split(m_BezeirSplit,out var L,out var R);
                L.DrawGizmos();
                R.DrawGizmos();
            }
            else
            {
                m_CubicCurve.DrawGizmos();
                Gizmos.color = Color.grey;
                m_CubicCurve.GetBoundingBox().DrawGizmos();
            }
            
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(10f,0f,0f));
            m_PolynomialCurve.DrawGizmos();
            
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(15f,0f,0f));
            m_HermiteCurve.DrawGizmos();
            
            //Splines
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(0f,0f,-3f));
            m_FourierSpline.DrawGizmos();
            
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(5f,0f,-3f));
            m_HermineSpline.DrawGizmos();
            
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(10f,0f,-3f));
            m_CatmullRomSpline.DrawGizmos();
            
            //BSpline
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(0f,0f,-6f));
            m_BSpline.DrawGizmos();
            
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(5f,0f,-6f));
            m_BSplineUniform.DrawGizmos();
            
            //Line segments
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(0f,0f,-9f));
            m_DivisionCurve.DrawGizmos();
            Gizmos.matrix = localToWorldMatrix * Matrix4x4.Translate(new Vector3(5f,0f,-9f));
            m_ChaikinCurve.DrawGizmos();
        }
#endif
    }
}