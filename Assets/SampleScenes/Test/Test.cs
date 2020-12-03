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

    public uint m_Unpack1;
    public uint m_Unpack2;
    public Color m_UnpackedColor;
    public uint m_Unpack3;
    public Vector2 m_UnpackedUV;
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

        m_UnpackedColor = UnpackFColorFromUInts(m_Unpack1,m_Unpack2);
        m_UnpackedUV = UnpackUV(m_Unpack3);
    }
    const ushort UShortMax = ushort.MaxValue;
    const float FColorPrecision = 1.0f / 512.0f;
    const float InvFColorPrecision = 1.0f / FColorPrecision;
    public static Vector2 UnpackUV(uint pack)=>new Vector2((float)((pack >> 16) & 0xFFFF) / UShortMax, (float)((pack) & 0xFFFF) / UShortMax);
    public static Color UnpackFColorFromUInts(
        uint pack0, uint pack1)=>new Color((short)((pack0 >> 16) & 0xFFFF) / InvFColorPrecision, (short)((pack0) & 0xFFFF) / InvFColorPrecision, (short)((pack1 >> 16) & 0xFFFF) / InvFColorPrecision, (short)((pack1) & 0xFFFF) / InvFColorPrecision);
}
