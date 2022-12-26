using Geometry;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeChaikin : MonoBehaviour
    {
        public GChaikinCurve m_ChaikinCurve = GChaikinCurve.kDefault;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            m_ChaikinCurve.DrawGizmos();
        }
    }
}