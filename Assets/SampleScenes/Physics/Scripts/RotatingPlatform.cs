using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsTest
{
    public class RotatingPlatform : MonoBehaviour
    {
        public Vector3 m_RotateEuler;
        private void LateUpdate() => transform.Rotate(m_RotateEuler*Time.fixedDeltaTime, Space.Self);
    }
}