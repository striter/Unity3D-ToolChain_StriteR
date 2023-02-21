using Geometry;
using UnityEngine;
using UnityEngine.Serialization;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeSpline : MonoBehaviour
    {
        public GBezierSpline m_Spline = GBezierSpline.kDefault;
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var coord in m_Spline.coordinates)
                Gizmos.DrawSphere(coord,.1f);
            
            m_Spline.DrawGizmos();
        }
    }

}