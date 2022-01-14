using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExampleScenes.Rendering.GeometryVisualize
{
    public class GeometryVisualize_Bezier : MonoBehaviour
    {
        [Header("Quadratic")]
        public Vector3 m_QuadraticSource;

        public Vector3 m_QuadraticControl;
        public Vector3 m_QuadraticDestination;
        public int m_Desntiy=5;

        [Header("Cubic")] 
        public Vector3 m_CubicSource;
        public Vector3 m_CubicDestination;
        public Vector3 m_CubicSourceControl;
        public Vector3 m_CubicDestinationControl;
        public int m_CubicDensity;
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(m_QuadraticSource,.05f);
            Gizmos.DrawLine(m_QuadraticSource,m_QuadraticControl);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(m_QuadraticDestination,.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_QuadraticControl,.05f);
            Gizmos.color = Color.white;
            Vector3[] points = new Vector3[m_Desntiy + 1];
            for (int i = 0; i < m_Desntiy+1; i++)
            {
                float alpha = i / (float)m_Desntiy;
                points[i] = UMath.QuadraticBezierCurve(m_QuadraticSource,m_QuadraticDestination,m_QuadraticControl,alpha);
            }
            Gizmos_Extend.DrawLines(points);
        }
    }

}