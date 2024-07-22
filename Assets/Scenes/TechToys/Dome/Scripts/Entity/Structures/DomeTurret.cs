using System;
using System.Collections.Generic;
using Dome.Collision;
using Runtime;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity
{
    [Serializable]
    public struct DomeTurretData
    {
        public Damper viewDamper;
        public EntityExtentData extentData;

        public static DomeTurretData kDefault = new DomeTurretData()
        {
            viewDamper = Damper.kDefault,
        };
    }

    public class DomeTurret : MonoBehaviour,IEntityPersistent<DomeTurretData>
    {
        public DomeTurretData m_Data = DomeTurretData.kDefault;
        public DomeTurretData Data => m_Data;
    }
    
    public class FDomeTurret : ADomeEntity<DomeTurret,DomeTurretData>, ITurretModel ,IPitchYawMove,ITeam ,IAim , ILive, ISelection , IWireEnd , IProjectileCaster , IPlayerControl , ICollisionReceiver
    {
        public Damper kViewDamperData => Data.viewDamper;
        public string kProjectileName => "Bullet";
        public float kCastCooldown => .1f;
        public EntityExtentData kExtentData => Data.extentData;
        
        public FDomeEntityInput input { get; set; }
        public string modelPath { get; set; }
        public GameObject modelRoot { get; set; }
        public MeshRenderer[] meshRenderers { get; set; }
        public Material[] restoreMaterials { get; set; }
        public Dictionary<string, Transform> modelNodes { get; set; }
        public Damper viewDamper { get; set; }
        public Transform pitchTransform { get; set; }
        public Transform yawTransform { get; set; }
        public ESelections selectionFlags { get; set; }
        public GameObject hoveringObj { get; set; }
        public Animation hoveringAnimation { get; set; }
        public float kMaxHealth { get; set; }
        public float maxHealth { get; set; }
        public ETeam team { get; set; }
        public Transform wireNode { get; set; }
        public IWireStart connecting { get; set; }
        public RopeRenderer wireRoot { get; set; }
        public IEntity desiredTarget { get; set; }
        public float3 aimDirection { get; set; }
        public float2 desiredRotationLS { get; set; }
        public float pitch { get; set; }
        public float yaw { get; set; }
        public Collider[] colliders { get; set; }
        public int ownerId { get; set; }
        public Transform[] castNodes { get; set; }
        public int castIndex { get; set; }
        public Counter projectileCastCooldown { get; set; }
    }
}