using System;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.CharacterControl.InverseKinematics
{
    
    public class InverseKinematic_HeadLookAt : AInverseKinematic
    {
        public bool m_Paused = false;
        public Transform m_Target;
        public Transform m_Head;
        public Transform m_Root;
        public ETransformAxis m_RootAxis = ETransformAxis.Forward;
        public RangeFloat m_HeadMaxAngle = new RangeFloat(60f,40f);
        public Damper m_HeadRotationDamper = new() {halfLife = 0.2f,mode = EDamperMode.SpringCritical};
        public Vector3 m_LookAtOffset = new(0,0.08f,0);
        public Vector3 m_HeadExtraRotation = new(0, -90f, -93f);
        public Counter m_HeadFocusCounter = new Counter(1f);
        [Range(0f,1f)] public float m_HeadParentRotationNormalization = 0.6f;
        public override bool Valid => m_Head != null && m_Root != null;
        
        public override void Initialize()
        {
        }
        
        public override void UnInitialize()
        {
        }

        public override void Reset()
        {
        }

        public override void Tick(float _deltaTime)
        {
            if (m_Paused)
                _deltaTime = 0f;
            
            m_HeadFocusCounter.Tick(_deltaTime);
            var srcRotationLS = m_Head.localRotation;
            var headRotationOffsetLS = quaternion.identity;
            var lookAtTarget = m_Target;
            var spineForward = quaternion.LookRotation(m_Root.GetAxis(m_RootAxis), kfloat3.up);
            if (lookAtTarget != null)
            {
                var headPosition = (float3)m_Head.position + math.mul(m_Head.rotation, m_LookAtOffset);
                var targetPosition = (float3)lookAtTarget.position;
                var headLookOffset = targetPosition - headPosition;
                var desireHeadRotationWS = quaternion.LookRotation(headLookOffset.normalize(), kfloat3.up);
                var rotProduct = math.dot(spineForward, desireHeadRotationWS);
                var spineAngleDiff = math.acos(2 * rotProduct * rotProduct - 1f) * kmath.kRad2Deg;
                desireHeadRotationWS = math.slerp(spineForward,desireHeadRotationWS,math.min(spineAngleDiff,m_HeadMaxAngle.start)/spineAngleDiff);
                var headDesireRotationWS = math.mul(desireHeadRotationWS, quaternion.Euler(m_HeadExtraRotation * kmath.kDeg2Rad));
                var desireRotaionLS = math.mul( math.inverse(m_Head.parent.rotation), headDesireRotationWS);
                headRotationOffsetLS =  math.mul( math.inverse(srcRotationLS),desireRotaionLS);

                if (spineAngleDiff < m_HeadMaxAngle.end)
                    m_HeadFocusCounter.Replay();
                
                if (!m_HeadFocusCounter.Playing)
                {
                    if (spineAngleDiff > m_HeadMaxAngle.end)
                        headRotationOffsetLS = quaternion.identity;
                }
            }

            var rotationOffsetLS = m_HeadRotationDamper.Tick(_deltaTime,headRotationOffsetLS);
            m_Head.parent.localRotation = math.mul(m_Head.parent.localRotation, math.slerp(quaternion.identity, rotationOffsetLS,  m_HeadParentRotationNormalization));
            m_Head.localRotation = math.mul(srcRotationLS, math.slerp(quaternion.identity, rotationOffsetLS, 1f - m_HeadParentRotationNormalization));
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;

            if (!Valid)
                return;
            
            var lookAtTarget = m_Target;
            if (lookAtTarget == null)
                return;

            var compareRotation = transform.rotation;
            var targetPosition = (float3)lookAtTarget.position;
            var sourcePosition = (float3)m_Head.position + math.mul(compareRotation, m_LookAtOffset);
            
            var height = (targetPosition - sourcePosition).magnitude();
            var cone = new GCone(sourcePosition , m_Root.GetAxis(m_RootAxis), m_HeadMaxAngle.start);
            cone.DrawGizmos(height);
            cone.angle = m_HeadMaxAngle.end;
            cone.DrawGizmos(height);
            
            Gizmos.DrawLine(sourcePosition,targetPosition);
        }
        
    }
}