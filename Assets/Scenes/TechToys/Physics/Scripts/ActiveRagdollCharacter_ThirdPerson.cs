using System.Collections;
using System.Collections.Generic;
using Runtime.TouchTracker;
using UnityEngine;

namespace Examples.PhysicsScenes
{
    public class ActiveRagdollCharacter_ThirdPerson : PhysicsCharacterBase
    {
        [SerializeField]
        protected Transform m_CameraFollow;
        [SerializeField]
        protected Transform m_CameraAttach;
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