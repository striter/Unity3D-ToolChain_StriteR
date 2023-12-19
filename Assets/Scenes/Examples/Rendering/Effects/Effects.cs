using System.Linq;
using Geometry;
using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.Effects
{
    public class Effects : MonoBehaviour
    {
        public MeshRenderer m_FocusTarget;
        public float2 pitchYaw;
        public float2 m_ViewportPoint;
        [Range(-.5f,.5f)] public float m_YAnchor = 0f;
        
        private Camera m_Camera;

        public Damper m_RotationDamper = new Damper();
        public Damper m_OriginDistanceDamper = new Damper();

        private void Awake()
        {
            m_Camera = transform.GetComponentInChildren<Camera>();
            m_RotationDamper.Initialize(pitchYaw.to3xy() * kmath.kDeg2Rad);
        }

        private void Update()
        {
            if (!m_FocusTarget)
                return;

            var boundingBoxWS = (GBox)m_FocusTarget.bounds;

            var deltaTime = Time.deltaTime;
            var originAndDistance = m_OriginDistanceDamper.Tick(deltaTime, boundingBoxWS.GetPoint(kfloat3.up*m_YAnchor).to4(-boundingBoxWS.size.magnitude()));
            var rotationWS = math.normalize( m_RotationDamper.Tick(deltaTime, quaternion.Euler(pitchYaw.to3xy() * kmath.kDeg2Rad)));
            
            var frustum = new GFrustum(0,rotationWS ,m_Camera.fieldOfView,m_Camera.aspect,m_Camera.nearClipPlane,m_Camera.farClipPlane);
            var viewportRay = frustum.GetFrustumRays().GetRay(m_ViewportPoint);
            
            m_Camera.transform.SetPositionAndRotation(originAndDistance.xyz+ viewportRay.GetPoint(originAndDistance.w),rotationWS);
        }

        [Button]
        public void PitchYawTest(float _pitch = 30f, float _yaw = 180f)
        {
            pitchYaw = new float2(_pitch, _yaw);
        }
        
    }

}