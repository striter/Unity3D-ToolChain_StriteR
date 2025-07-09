using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.CharacterControl.InverseKinematics
{
    public class InverseKinematic_EyeLookAt : AInverseKinematic
    {
        public override bool Valid => m_Eye != null && m_Root != null;
        public bool m_Paused;
        public Transform m_LookAtTarget;

        public Transform m_Eye;
        public Transform m_Root;
        public ETransformAxis m_RootAxis;
        public RangeFloat m_MaxClampAngle = new RangeFloat(50f,40f);
        public Damper m_Damper = Damper.kDefault;
        public Vector3 m_RotationCorrection = new(0, 0,0);
        private quaternion m_SrcLookRotation;

        public override void Initialize()
        {
            m_SrcLookRotation = m_Eye.localRotation;
            m_Eye.localRotation = m_SrcLookRotation;
            m_Damper.Initialize(m_SrcLookRotation);
        }

        public override void UnInitialize()
        {
            m_Eye.localRotation = m_SrcLookRotation;
        }
        
        
        public override void Reset()
        {
            m_Damper.Initialize(m_SrcLookRotation);
            m_Eye.localRotation = m_SrcLookRotation;    
        }
        
        public override void Tick(float _deltaTime)
        {
            if (m_Paused)
                _deltaTime = 0f;
            
            var desireRotationLS =  m_SrcLookRotation;
            if (m_LookAtTarget != null)
            {
                var root = m_Root;
                var desireForwardWS = root.GetAxis(m_RootAxis);
                var desireUpWS = root.GetAxis(m_RootAxis.GetCrossAxis());
                var lookDirectionWS = (m_LookAtTarget.position - m_Eye.position).normalized;

                var angleDiff = umath.radBetween(desireForwardWS,lookDirectionWS) * kmath.kRad2Deg;
                desireForwardWS = umath.slerp(desireForwardWS,lookDirectionWS,math.min(angleDiff,m_MaxClampAngle.start)/angleDiff,desireUpWS);

                var desireRotationWS = quaternion.LookRotation(desireForwardWS,desireUpWS);
                desireRotationWS = math.mul(desireRotationWS,quaternion.Euler(m_RotationCorrection * kmath.kDeg2Rad));
                desireRotationLS = math.mul(math.inverse(m_Eye.parent.rotation),desireRotationWS);
                
                if (angleDiff > m_MaxClampAngle.end)
                    desireRotationLS = m_SrcLookRotation;
            }
            
            m_Eye.localRotation = m_Damper.Tick(_deltaTime,desireRotationLS);
        }

        public void OnDrawGizmosSelected()
        {
            if (!Valid)
                return;
            
            if (m_LookAtTarget == null)
                return;
            var gizmosColor = Color.blue;
            gizmosColor.a = 0.5f;
            Gizmos.color = gizmosColor;
            Gizmos.DrawLine(m_Eye.position, m_LookAtTarget.position);
            var height = (m_LookAtTarget.position - m_Eye.position).magnitude;
            var cone = new GCone(m_Eye.position,m_Root.GetAxis(m_RootAxis),m_MaxClampAngle.start);
            cone.DrawGizmos(height);
            cone.angle = m_MaxClampAngle.end;
            cone.DrawGizmos(height);
            var up = m_Root.GetAxis(m_RootAxis.GetCrossAxis());
            Gizmos.color = Color.green;
            Gizmos.DrawLine(m_Eye.position, m_Eye.position + up);
        }
    }
}