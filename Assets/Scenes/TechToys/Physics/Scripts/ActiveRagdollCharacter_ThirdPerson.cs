using System.Collections;
using System.Collections.Generic;
using TTouchTracker;
using UnityEngine;

namespace Examples.PhysicsScenes
{
    public class ActiveRagdollCharacter_ThirdPerson : PhysicsCharacterBase
    {
        [SerializeField]
        protected Transform m_CameraFollow;
        [SerializeField]
        protected Transform m_CameraAttach;
        public override void OnTakeControl(TPSCameraController _controller)
        {
            base.OnTakeControl(_controller);
            _controller.m_BindRoot = m_CameraAttach;
            _controller.m_BindPosOffset = new Vector3(0, .5f, -4f);
            _controller.m_MoveDamping = .2f;
            _controller.m_RotateDamping = .2f;
        }
        public override void FixedTick(float _deltaTime)
        {
        }

        protected override void Tick(float _deltaTime, ref List<TrackData> _data)
        {
            m_CameraAttach.transform.position = m_CameraFollow.position;
            m_CameraAttach.transform.rotation = TickRotation();
        }
    }
}