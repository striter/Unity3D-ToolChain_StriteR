using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsTest
{
    public class ActiveRagdollCharacter_ThirdPerson : PhysicsCharacterBase
    {
        [SerializeField]
        protected Transform m_CameraFollow;
        [SerializeField]
        protected Transform m_CameraAttach;
        public override void OnTakeControl()
        {
            base.OnTakeControl();
            TPSCameraController.Instance.m_BindRoot = m_CameraAttach;
            TPSCameraController.Instance.m_BindPosOffset = new Vector3(0, .5f, -4f);
            TPSCameraController.Instance.m_MoveDamping = .2f;
            TPSCameraController.Instance.m_RotateDamping = .2f;
        }
        public override void FixedTick(float _deltaTime)
        {
        }

        public override void Tick(float _deltaTime)
        {
            m_CameraAttach.transform.position = m_CameraFollow.position;
            m_CameraAttach.transform.rotation = TickRotation();
        }
    }
}