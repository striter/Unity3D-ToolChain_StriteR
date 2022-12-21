using UnityEngine;

namespace Examples.Algorithm
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
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos_Extend.DrawArrow(Vector3.zero, m_Axis,1f,.1f);

            Gizmos.DrawLine(Vector3.zero, m_SrcVector);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, URotation.AngleAxis3x3(m_RotateAngle, m_Axis)*m_SrcVector);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, URotation.AngleAxisToQuaternion(m_RotateAngle, m_Axis)* m_SrcVector*.5f);
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
            Gizmos.DrawLine(Vector3.zero,URotation.FromToQuaternion(from,to)*m_FromToRotateVector);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero,URotation.FromTo3x3(from,to)*m_FromToRotateVector*.5f);

        }
#endif
    }

}