using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Dome
{
    [CreateAssetMenu(fileName = "DomeCamera_Commander",menuName = "Game/Dome/Camera/Commander")]
    public class FDomeCameraData_Commander : ScriptableObject
    {
        [Header("Initial")]
        public float2 initialRotation;
        public float initialZoom;
        
        [Header("Clamp")]
        public RangeFloat rotationClamp;
        public RangeFloat zoomClamp;

        
        [Header("Damper")]
        public Damper m_PositionDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        public Damper m_RotationDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        
        [Header("Sensitive")]
        public float rotationSensitive = 5f;
        public float movementSensitive = 5f;
        public float zoomSensitive = 5f;
    }
}