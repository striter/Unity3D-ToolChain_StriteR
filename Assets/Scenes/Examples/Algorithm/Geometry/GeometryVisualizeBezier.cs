using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeBezier : MonoBehaviour
    {
        [Header("Quadratic")] 
        public GBezierCurveQuadratic m_QuadraticCurve;
        public bool quadraticTangents = false;
        [Header("Cubic")] 
        public GBezierCurveCubic m_CubicCurve;
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            m_QuadraticCurve.DrawGizmos(quadraticTangents);
            Gizmos.color = Color.grey;
            m_QuadraticCurve.GetBoundingBox().DrawGizmos();
            
            m_CubicCurve.DrawGizmos();
            Gizmos.color = Color.grey;
            m_CubicCurve.GetBoundingBox().DrawGizmos();
        }
        #endif
    }

}