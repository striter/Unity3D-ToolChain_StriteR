using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Bezier;
using UnityEngine;

namespace ExampleScenes.Rendering.GeometryVisualize
{
    public class GeometryVisualize_Bezier : MonoBehaviour
    {
        [Header("Quadratic")] 
        public FBezierCurveQuadratic m_QuadraticCurve;
        [Header("Cubic")] 
        public FBezierCurveCubic m_CubicCurve;
        
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
    }

}