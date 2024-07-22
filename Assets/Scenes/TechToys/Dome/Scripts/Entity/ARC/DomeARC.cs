using System;
using System.Collections.Generic;
using Dome.Collision;
using Dome.Model;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity
{
    [Serializable]
    public struct FDomeARCData
    {
        public ARCSpeedDamper speedAccel;
        public ARCSpeedDamper angularSpeedAccel;
        public Damper viewDamper;
        public EntityExtentData extents;

        public static FDomeARCData kDefault = new FDomeARCData()
        {
            speedAccel = new ARCSpeedDamper {max=6,accelerate = 4,friction = 8},
            angularSpeedAccel = new ARCSpeedDamper {max=90,accelerate = 180,friction = 360},
            viewDamper = Damper.kDefault,
            extents = EntityExtentData.kDefault,
        };
    }
    
    public class DomeARC : MonoBehaviour , IEntityPersistent<FDomeARCData>
    {
        public FDomeARCData Data => m_Data;
        public FDomeARCData m_Data = FDomeARCData.kDefault;
    }
    
    public class ADomeARC : ADomeEntity<DomeARC,FDomeARCData>, ILive, IARCModel ,IARCMove , IAim , ISelection , IPlayerControl , IProjectileCaster, ICollisionReceiver
    {
        public EntityExtentData kExtentData => Data.extents;
        public ARCSpeedDamper kSpeedDamper => Data.speedAccel;
        public ARCSpeedDamper kAngularSpeedDamper => Data.angularSpeedAccel;
        public Damper kViewDamperData => Data.viewDamper;
        
        public string kProjectileName => "Missile";
        public float kCastCooldown => 1f;
        
        public string modelPath { get; set; }
        public ETeam team { get; set; }
        public FDomeEntityInput input { get; set; }
        public GameObject modelRoot { get; set; }
        public MeshRenderer[] meshRenderers { get; set; }
        public Material[] restoreMaterials { get; set; }
        public Dictionary<string, Transform> modelNodes { get; set; }
        public float2 trackMoves { get; set; }
        public MeshRenderer trackRenderer { get; set; }
        public float angularSpeed { get; set; }
        public float speed { get; set; }
        public float kMaxHealth { get; set; }
        public float maxHealth { get; set; }
        public IEntity desiredTarget { get; set; }
        public float3 aimDirection { get; set; }
        public float2 desiredRotationLS { get; set; }
        public Damper viewDamper { get; set; }
        public Transform pitchTransform { get; set; }
        public Transform yawTransform { get; set; }
        public ESelections selectionFlags { get; set; }
        public GameObject hoveringObj { get; set; }
        public Animation hoveringAnimation { get; set; }
        public float pitch { get; set; }
        public float yaw { get; set; }
        public int ownerId { get; set; }
        public Transform[] castNodes { get; set; }
        public int castIndex { get; set; }
        public Collider[] colliders { get; set; }
        public Counter projectileCastCooldown { get; set; }
    }

}