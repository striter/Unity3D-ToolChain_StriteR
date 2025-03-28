using System;
using Runtime.TouchTracker;
using UnityEngine;

namespace CameraController.Demo.Implement
{
    public class CameraControllerSimple : MonoBehaviour
    {
        public ACameraController m_Controller;
        public FControllerInput m_Input;
        [Readonly] public FCameraControllerCore m_Core = new ();

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            m_Core.Switch(m_Controller);
        }

        private void LateUpdate()
        {
            m_Core.Tick(Time.unscaledTime, ref m_Input);
        }
    }
}