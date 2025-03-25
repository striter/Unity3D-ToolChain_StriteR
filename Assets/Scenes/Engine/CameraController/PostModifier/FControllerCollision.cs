using CameraController.Inputs;
using Runtime.Geometry;
using UnityEngine;

namespace CameraController.Animation
{

    [CreateAssetMenu(fileName = "ControllerCollisionDefault", menuName = "Camera/Controller/Component/Collision")]
    public class FControllerCollision  : AControllerPostModifer
    {
        public override EControllerPostModiferQueue Queue => EControllerPostModiferQueue.Override;
        public override bool Disposable(bool _reset) => false;
        
        [Range(0, 2f)] public float m_CollisionMin = 0f;
        [Range(0,.5f)] public float m_CollisionForward = .1f;

        [Header("Collider")] 
        [CullingMask] public int m_CullingMask;
        [Range(0,.5f)] public float m_ColliderRadius = .1f;

        [Header("Debug")] public bool m_ForceSyncEveryFrame;
        [Readonly] public GameObject m_LastHitObject;
        [Readonly] public FCameraControllerOutput m_LastOutput;
        public bool CalculateDistance(GRay ray,float distance,out float hitDistance)
        {
            ray = ray.Forward(m_CollisionMin);
            hitDistance = distance;
            distance -= m_CollisionForward;
            
            m_LastHitObject = null;
            if (m_CullingMask == 0)
                return false;

            if(m_ForceSyncEveryFrame)
                Physics.SyncTransforms();
            if (Physics.SphereCast(ray, m_ColliderRadius, out var hit, distance, m_CullingMask))
            {
                m_LastHitObject = hit.collider.gameObject;
                hitDistance =  hit.distance;
                return true;
            }

            return false;
        }

        public override void Tick(float _deltaTime, AControllerInput _input, ref FCameraControllerOutput _output)
        {
            m_LastOutput = _output;
            _output.Evaluate(_input.Camera, out var frustumRays, out var ray);
            CalculateDistance(ray, _output.distance, out _output.distance);
        }

        public override void DrawGizmos(AControllerInput _input)
        {
            base.DrawGizmos(_input);
            m_LastOutput.Evaluate(_input.Camera, out var frustumRays, out var ray);
            var distance = m_LastOutput.distance - m_CollisionMin;
            ray = ray.Forward(m_CollisionMin);
            if (m_CullingMask != 0 && Physics.SphereCast(ray, m_ColliderRadius,out var hit, distance, m_CullingMask))
            {
                m_LastHitObject = hit.collider.gameObject;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(ray.origin,ray.GetPoint(hit.distance - m_CollisionForward));
            } 
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ray.origin,ray.GetPoint(distance));
        }

    }

}