using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    #if UNITY_EDITOR
    public class GeometryVisualizeAngle : MonoBehaviour
    {
        [PostNormalize] public Vector3 m_Position;
        public GTriangle m_Triangle = GTriangle.kDefault;
        public GQuad m_Quad = GQuad.kDefault;
        public float3 m_WeightPosition = float3.zero;
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
            UGizmos.DrawString( yaw.ToString(), Vector3.zero);

            Gizmos.color = Color.white;
            UGizmos.DrawString( umath.toPitchYaw(m_Position).ToString(), Vector3.down * .1f);

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(kfloat3.right * 3f);
            m_Triangle.DrawGizmos();
            UGizmos.DrawString(m_Triangle.GetArea().ToString(), Vector3.zero);
            var weightTriangle = m_Triangle.GetWeightsToPoint(m_Triangle.GetPlane().Projection(m_WeightPosition));
            UGizmos.DrawString(weightTriangle.ToString(), m_WeightPosition);
            Gizmos.DrawSphere(m_WeightPosition,.05f);
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(kfloat3.right * 6f);
            m_Quad.DrawGizmos();
            UGizmos.DrawString(m_Quad.GetArea().ToString(), Vector3.zero);

            Gizmos.DrawSphere(m_WeightPosition,.05f);
            var weightQuad = m_Quad.GetWeightsToPoint(m_WeightPosition);
            UGizmos.DrawString(weightQuad.ToString(), m_WeightPosition);
        }
    }
    #endif
}