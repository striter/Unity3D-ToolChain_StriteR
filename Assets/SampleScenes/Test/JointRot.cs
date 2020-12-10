using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class JointRot : MonoBehaviour
{
    ConfigurableJoint joint;
    public Transform sourceRotation;
    private void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
    }
    private void Update()
    {
        joint.targetRotation = sourceRotation.localRotation;
    }

}
