using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dome
{
    [CreateAssetMenu(fileName = "DomeCamera_FreeSpectator",menuName = "Game/Dome/Camera/FreeSpectator")]
    public class FDomeCameraData_FreeSpectator : ScriptableObject
    {
        [Header("Damper")]
        public Damper m_PositionDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        public Damper m_RotationDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        
        [Header("Sensitive")]
        public float rotationSensitive = 5f;
        public float movementSensitive = 5f;
    }
}