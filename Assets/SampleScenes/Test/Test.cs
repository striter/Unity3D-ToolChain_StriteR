using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    public Vector3 m_Euler;
    public Quaternion m_Quaternion;
    public Vector3 m_Forward;
    public float m_Sin;
    public float m_SinResult;
    public float m_Cos;
    public float m_CosResult;
    public Vector3 m_EulerTest;
    public Quaternion m_EulerToQuaternion;
    public Vector3 m_AxisTest;
    public float m_AngleTest;
    public Quaternion m_AxisAngleToQuaternion;
    private void OnDrawGizmos()
    {
        m_EulerToQuaternion = TAlthogrim.EulerToQuaternion(m_EulerTest).normalized;

        m_AxisAngleToQuaternion = TAlthogrim.AngleAxisToQuaternion(m_AngleTest, m_AxisTest);
        m_AxisAngleToQuaternion = Quaternion.AngleAxis(m_AngleTest, m_AxisTest);

        m_Quaternion = Quaternion.Euler(m_Euler);
        m_Forward = Matrix4x4.Rotate(m_Quaternion).MultiplyPoint(Vector3.forward).normalized;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, m_Quaternion, Vector3.one);
        Gizmos.color = Color.green;
        Gizmos_Extend.DrawArrow(transform.position, m_Quaternion, 1f,.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Vector3.zero, Vector3.up);
        m_SinResult = Mathf.Sin(TCommon.AngleToRadin( m_Sin));
        m_CosResult = Mathf.Cos(TCommon.AngleToRadin( m_Cos));
    }
}
