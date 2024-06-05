using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    #if UNITY_EDITOR
    public class GeometryVisualizeAngle : MonoBehaviour
    {
        [PostNormalize] public Vector3 m_Position;
        private void OnDrawGizmos()
        {
            if (SceneView.currentDrawingSceneView == null)
                return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero,Vector3.forward);


            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero,m_Position.normalized); 

            var yaw = umath.toPitchYaw(m_Position);
            UGizmos.DrawString( Vector3.zero,yaw.ToString());

            UGizmos.DrawString( Vector3.down * .1f,umath.toPitchYaw(m_Position).ToString());
        }
    }
    #endif
}