using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.CharacterControl.InverseKinematics
{
    //https://iquilezles.org/articles/simpleik/
    [ExecuteInEditMode]
    public class InverseKinematic_SimpleIK : AInverseKinematic
    {
        public Transform m_Root;
        public Transform m_Joint;
        public float m_RootRadius = 1f;
        public float m_EvaluateRadius = 1f;
        public ETransformAxis m_RootAxis;
        public float3 m_RotationCorrection;
        [Position] public Vector3 m_Evaluate;
        public float3 Forward =>  transform.GetAxis(m_RootAxis);
        public override bool Valid => m_Root != null && m_Joint != null;
        public override void Initialize()
        {
            
        }

        public override void UnInitialize()
        {
        }

        public override void Tick(float _deltaTime)
        {
            var rootPos = (float3)m_Root.position;
            var dir = transform.GetAxis(m_RootAxis);
            var evaluate = (float3)m_Evaluate;
            var jointDesirePos = Solve( rootPos, m_RootRadius, evaluate,m_EvaluateRadius, dir);
            var correction = quaternion.Euler(m_RotationCorrection * kmath.kDeg2Rad);
            var rootRotation = quaternion.LookRotation((jointDesirePos-rootPos).normalize(),dir);
            m_Root.transform.rotation = math.mul(rootRotation,correction);
            
            var jointRotation = quaternion.LookRotation((evaluate-jointDesirePos).normalize(),dir);
            
            m_Joint.transform.position = jointDesirePos;
            m_Joint.transform.rotation = math.mul(jointRotation,correction);
        }

        public override void Reset()
        {
        }

        private void LateUpdate()
        {
            if(Application.isPlaying)
                return;

            if (!Valid)
                return;
            
            Tick(UTime.deltaTime);
        }

        public bool m_DrawGizmos;
        private void OnDrawGizmosSelected()
        {
            if (!m_DrawGizmos || !Valid)
                return;
            
            var rootPos = (float3)m_Root.position;
            var dir = transform.GetAxis(m_RootAxis);
            var jointDesirePos = Solve( rootPos, m_RootRadius, m_Evaluate,m_EvaluateRadius, dir);
            
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireSphere(m_Evaluate,0.1f);
            Gizmos.DrawWireSphere(rootPos,0.1f);
            
            UGizmos.DrawArrow(rootPos,dir,.5f,.1f);
            Gizmos.DrawWireSphere(jointDesirePos,.1f);

            Gizmos.DrawLine(rootPos,jointDesirePos);
            Gizmos.DrawLine(jointDesirePos,m_Evaluate);
            Gizmos.color = Color.white.SetA(.3f);
            new GSphere(rootPos,m_RootRadius).DrawGizmos();
            new GSphere(m_Evaluate,m_EvaluateRadius).DrawGizmos();
        }

        public static float3 Solve(float3 _rootPos, float _rootRadius,float3 _evaluatePos, float _evaluateRadius,float3 _dir)
        {
            var p = (_evaluatePos - _rootPos);
            var q = (_evaluatePos - _rootPos)*( 0.5f + 0.5f*(_rootRadius*_rootRadius-_evaluateRadius*_evaluateRadius)/math.dot(p,p) );
            var s = _rootRadius*_rootRadius - math.dot(q,q);
            s = math.max( s, 0.0f );
            q += math.sqrt(s)*math.normalize(math.cross(p,_dir));
            return _rootPos + q;
        }
    }
}