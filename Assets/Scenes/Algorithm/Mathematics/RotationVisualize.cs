using Unity.Mathematics;
using UnityEngine;

namespace Examples.Mathematics
{
    public class RotationVisualize : MonoBehaviour
    {
        [Header("Angle Axis")]
        public Vector3 m_SrcVector = Vector3.one;
        public Vector3 m_Axis = Vector3.down;
        public float m_RotateAngle;

        [Header("From To")]
        public Vector3 m_FromVector=Vector3.up;
        public Vector3 m_ToVector = Vector3.forward;
        public Vector3 m_FromToRotateVector = Vector3.up;

        [Header("Slerp")] public Vector3 m_SlerpFrom = kfloat3.left;
        public Vector3 m_SlerpTo = kfloat3.right;
        [Range(0, 1)] public float m_SlerpVal;
        
        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            UGizmos.DrawArrow(Vector3.zero, m_Axis,1f,.1f);

            Gizmos.DrawLine(Vector3.zero, m_SrcVector);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, umath.AngleAxis3x3(m_RotateAngle, m_Axis)*m_SrcVector);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, umath.AngleAxisToQuaternion(m_RotateAngle, m_Axis).mul( m_SrcVector)*.5f);
            //Debug.Log(TVector.SqrMagnitude(m_SrcVector) + " " +  m_SrcVector.sqrMagnitude);
            //Debug.Log(TVector.Dot(m_SrcVector, m_DstVector) + " " + Vector3.Dot(m_SrcVector, m_DstVector));
            //Debug.Log(TVector.Project(m_SrcVector, m_DstVector) + " " + Vector3.Project(m_SrcVector, m_DstVector));
            //Debug.Log(TVector.Cross(m_SrcVector, m_DstVector) + " " + Vector3.Cross(m_SrcVector, m_DstVector));

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right * 2f);
            Gizmos.color = Color.white;
            Vector3 from = m_FromVector.normalized;
            Vector3 to = m_ToVector.normalized;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero,from);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero,to);
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right * 3f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(Vector3.zero,m_FromToRotateVector);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero,umath.FromToQuaternion(from,to).mul(m_FromToRotateVector));
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero,umath.FromTo3x3(from,to)*m_FromToRotateVector*.5f);

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right * 4f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero,m_SlerpFrom);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero,m_SlerpTo);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero,umath.slerp(m_SlerpFrom, m_SlerpTo, m_SlerpVal,kfloat3.up));
            UGizmos.DrawString(umath.angle(m_SlerpFrom,m_SlerpTo).ToString(), Vector3.zero);
            
        }
    }

}