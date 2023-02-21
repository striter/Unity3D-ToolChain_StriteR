using Geometry;
using UnityEngine;
using UnityEngine.Serialization;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeSpline : MonoBehaviour
    {
        public GSpline m_Spline = GSpline.kDefault;
        public GBezierSplineUniform m_SplineUniform = GBezierSplineUniform.kDefault;
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            m_Spline.DrawGizmos(true);
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(new Vector3(0f,0f,-5f));
            m_SplineUniform.DrawGizmos(true);
        }
    }

}