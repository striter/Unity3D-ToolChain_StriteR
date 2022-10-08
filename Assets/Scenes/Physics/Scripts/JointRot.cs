using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OPhysics;
namespace ExampleScenes.PhysicsScenes
{
    [RequireComponent(typeof(ConfigurableJoint))]
    public class JointRot : MonoBehaviour
    {
        public Transform sourceRotation;
        public ConfigurableJoint_Helper m_Joint;
        private void Start()
        {
            m_Joint = new ConfigurableJoint_Helper(GetComponent<ConfigurableJoint>());
        }
        private void Update()
        {
            sourceRotation.Rotate(45 * Time.deltaTime, 30f * Time.deltaTime, 0f, Space.Self);
            m_Joint.SetTargetPosition(sourceRotation.localPosition);
            m_Joint.SetTargetRotation(sourceRotation.localRotation);
        }
    }
}