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
            var boundingSphereWS = UBounds.GetBoundingSphere(boundingBoxWS.GetPositions().ToArray());
            
            var positionWS = boundingBoxWS.GetPoint(kfloat3.up*m_YAnchor);
            var rotationWS = quaternion.Euler(m_RotationDamper.Tick(Time.deltaTime, pitchYaw.to3xy() * kmath.kDeg2Rad));
            
            var frustum = new GFrustum(0,rotationWS ,m_Camera.fieldOfView,m_Camera.aspect,m_Camera.nearClipPlane,m_Camera.farClipPlane);
            var viewportRay = frustum.GetFrustumRays().GetRay(m_ViewportPoint);
            
            positionWS -= viewportRay.GetPoint(boundingSphereWS.radius * 2f);

            m_Camera.transform.SetPositionAndRotation(positionWS,rotationWS);
        }

        [Button]
        public void PitchYawTest(float _pitch = 30f, float _yaw = 180f)
        {
            pitchYaw = new float2(_pitch, _yaw);
        }
        
    }

}