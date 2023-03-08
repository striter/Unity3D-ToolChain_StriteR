using UnityEditor;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    #if UNITY_EDITOR
    public class GeometryVisualizeAngle : MonoBehaviour
    {
        public Vector3 m_Position;
        private void OnDrawGizmos()
        {
            if (SceneView.currentDrawingSceneView == null)
                return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero,Vector3.forward);


            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero,m_Position);

            var rad = umath.GetRadClockWise(Vector2.up,new Vector2(m_Position.x,m_Position.z));
            Gizmos_Extend.DrawString( Vector3.zero,(kmath.kRad2Deg*rad).ToString());
        }
    }
    #endif
}