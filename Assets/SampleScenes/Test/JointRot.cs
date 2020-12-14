using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TPhysics;
[RequireComponent(typeof(ConfigurableJoint))]
public class JointRot : MonoBehaviour
{
    public Transform sourceRotation;
    public ConfigurableJoint m_ConfigurableJoint { get; private set; }

    Quaternion m_InvStartLocalRotation;
    Quaternion m_LocalToJointSpace;
    private void Start()
    {
        m_ConfigurableJoint = GetComponent<ConfigurableJoint>();
        OnValidate();
    }
    private void OnValidate()
    {
        if (m_ConfigurableJoint == null)
            return;
        Vector3 right = m_ConfigurableJoint.axis;
        Vector3 up = m_ConfigurableJoint.secondaryAxis;
        Vector3 forward = Vector3.Cross(right, up).normalized;
        m_LocalToJointSpace = Quaternion.LookRotation(forward, up);
        m_InvStartLocalRotation = Quaternion.Inverse(m_ConfigurableJoint.connectedBody.transform.localRotation);
    }
    private void Update()
    {
        //sourceRotation.Rotate(15f*Time.deltaTime, 30f * Time.deltaTime, 0f, Space.Self);
        SetTargetRotation(sourceRotation.localRotation);
    }
    public void SetTargetRotation(Quaternion _localRotation)
    {
        m_ConfigurableJoint.targetRotation = m_LocalToJointSpace* _localRotation * m_InvStartLocalRotation;
        Debug.Log(m_InvStartLocalRotation.eulerAngles + "," + m_LocalToJointSpace.eulerAngles + "\n" + _localRotation.eulerAngles + "," + m_ConfigurableJoint.targetRotation.eulerAngles);
    }
    private void OnDrawGizmos()
    {
        if (m_ConfigurableJoint == null)
            return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, m_ConfigurableJoint.axis);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.zero, m_ConfigurableJoint.secondaryAxis);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, Vector3.Cross(m_ConfigurableJoint.axis, m_ConfigurableJoint.secondaryAxis).normalized);
    }
}
