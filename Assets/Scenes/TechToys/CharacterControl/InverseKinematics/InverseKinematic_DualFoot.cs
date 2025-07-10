using System;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.CharacterControl.InverseKinematics
{
    public class InverseKinematic_DualFoot : AInverseKinematic
    {
        [Range(0f, 1f)] public float m_Weight = 1f;
        public Damper m_PelvisDamper = Damper.kDefault;
        public Transform m_Pelvis;
        public FootSolver m_LeftFoot;
        public FootSolver m_RightFoot;
        public override bool Valid => m_Pelvis != null && m_LeftFoot.Valid && m_RightFoot.Valid;
        public override void Initialize()
        {
            m_LeftFoot.Initialize();
            m_RightFoot.Initialize();
        }

        public override void UnInitialize()
        {
        }

        public override void Tick(float _deltaTime)
        {
            var offset = 0f;
            if(m_LeftFoot.Validate(out var lOffset))
                offset = lOffset;
            if(m_RightFoot.Validate(out var rOffset))
                offset = math.max(offset, rOffset);
            offset = math.lerp(0, offset, m_Weight);
            m_Pelvis.localPosition -= new Vector3(0f, m_PelvisDamper.Tick(_deltaTime,offset), 0f);
                
            m_LeftFoot.Tick(_deltaTime);
            m_RightFoot.Tick(_deltaTime);
        }

        public override void Reset()
        {
            m_PelvisDamper.Initialize(0f);
            m_LeftFoot.Reset();
            m_RightFoot.Reset();
        }

        private void OnDrawGizmosSelected()
        {
            m_LeftFoot.DrawGizmos();
            m_RightFoot.DrawGizmos();
        }
    }
    
    [Serializable]
    public class FootSolver
    {
        public Transform m_Foot;
        public Transform m_Calf;
        public Transform m_Thigh;
        public ETransformAxis m_ThighRightAxis = ETransformAxis.Right;
        
        [Header("Ray Cast")]
        public RangeFloat m_RayLength = new RangeFloat(-.1f, .2f);
        public CullingMask m_CullingMask = -1;
        
        [Header("Damping")]
        public Damper m_RotationDamper = Damper.kDefault;
        public Damper m_PositionDamper = Damper.kDefault;
        [Header("Helper & Debug")]
        public float3 m_RotationCorrection = 0f;
        public float3 m_PositionCorrection = 0f;
        [Readonly] public float3 m_DesirePositionWS = float3.zero;
        [Readonly] public quaternion m_DesireRotationWS = quaternion.identity;
        [Readonly] public float m_RadiusCT, m_RadiusCF;
        public bool Valid => m_Foot != null && m_Calf != null && m_Thigh != null;

        private static RaycastHit kHitInfo;

        public void Initialize()
        {
            m_RadiusCT = math.distance(m_Calf.position, m_Thigh.position);
            m_RadiusCF = math.distance(m_Calf.position, m_Foot.position);
        }
        
        public void Reset()
        {
            m_DesirePositionWS = m_Foot.position;
            m_DesireRotationWS = m_Foot.rotation;
            m_RotationDamper.Initialize(m_Foot.localPosition);
            m_PositionDamper.Initialize(m_Foot.localRotation);
        }
        
        public bool Validate(out float _heightOffset)
        {
            var feetPositionWS = (float3)m_Foot.position;
            var thighRight = m_Thigh.GetAxis(m_ThighRightAxis);
            var feetRay = new GRay(feetPositionWS - kfloat3.up * m_RayLength.start,Vector3.down);
            _heightOffset = 0f;
            m_DesirePositionWS = m_Foot.position;
            m_DesireRotationWS = m_Foot.rotation;
            if (!Physics.Raycast(feetRay, out kHitInfo, m_RayLength.length, m_CullingMask.value)) 
                return false;
            var desireRotationWS = math.mul(quaternion.LookRotation( math.cross(thighRight,kHitInfo.normal),kHitInfo.normal),
                quaternion.Euler(m_RotationCorrection * kmath.kDeg2Rad));
            var desirePositionWS = (float3)kHitInfo.point + math.mul(desireRotationWS,m_PositionCorrection);
            
            m_DesireRotationWS = desireRotationWS;
            m_DesirePositionWS = desirePositionWS;
            _heightOffset = (feetPositionWS - desirePositionWS).y;
            return true;

        }
        
        public void Tick(float _deltaTime)
        {
            m_Foot.transform.localPosition = m_PositionDamper.Tick(_deltaTime, m_Foot.parent.worldToLocalMatrix.MultiplyPoint(m_DesirePositionWS));
            m_Foot.transform.localRotation = m_RotationDamper.Tick(_deltaTime,math.mul(math.inverse(m_Foot.parent.rotation),m_DesireRotationWS));
        }

        public void DrawGizmos()
        {
            if (!Valid)
                return;
            
            Gizmos.matrix = Matrix4x4.identity;
            var thighRight = (float3)m_Thigh.GetAxis(m_ThighRightAxis);
            var thighPosition = (float3)m_Thigh.position;
            var feetPosition = (float3)m_Foot.position;
            var feetRay = new GRay(feetPosition - kfloat3.up * m_RayLength.start,Vector3.down);
            feetRay.DrawGizmos(m_RayLength.length);
            Gizmos.DrawWireSphere(feetPosition,0.1f);
            Gizmos.DrawLine(m_Foot.position,feetPosition);
            UGizmos.DrawArrow(thighPosition,thighRight,.5f,.1f);
        }
        
    }
}