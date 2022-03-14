using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Bezier;
using UnityEngine;

namespace ExampleScenes.Algorithm.Geometry
{
    public class GeometryVisualize_Bezier : MonoBehaviour
    {
        [Header("Quadratic")] 
        public FBezierCurveQuadratic m_QuadraticCurve;
        [Header("Cubic")] 
        public FBezierCurveCubic m_CubicCurve;
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            m_QuadraticCurve.DrawGizmos();
            Gizmos.color = Color.grey;
            m_QuadraticCurve.GetBoundingBox().DrawGizmos();
            
            m_CubicCurve.DrawGizmos();
            Gizmos.color = Color.grey;
            m_CubicCurve.GetBoundingBox().DrawGizmos();
        }
        #endif
    }

}