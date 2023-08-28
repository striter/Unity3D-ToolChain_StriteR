using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Dome
{
    [CreateAssetMenu(fileName = "DomeCamera_ThirdPerson",menuName = "Game/Dome/Camera/ThirdPerson")]
    public class FDomeCameraData_ThirdPerson : ScriptableObject
    {
        public float3 positionOffset = new float3(-3,2,-3);
        public float3 rotationOffset = new float3(15, -15, 0);
        
        [Header("Damper")]
        public Damper m_PositionDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        public Damper m_RotationDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        
        [Header("Clamp")]
        public RangeFloat rotationClamp;
        [Header("Sensitive")]
        public float rotationSensitive = 5f;
    }
}