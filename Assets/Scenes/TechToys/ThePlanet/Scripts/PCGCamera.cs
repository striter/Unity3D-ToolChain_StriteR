using System;
using Runtime.Geometry;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace TechToys.ThePlanet
{
    public class PCGCamera:MonoBehaviour,IPCGControl
    {
        private Transform m_CameraRoot;
        public Camera m_Camera { get; private set; }
        public Damper m_PositionDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        public Damper m_RotationDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        public Damper m_ZoomDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        
        public Vector3 m_RootPosition = Vector3.zero;
        [Header("Constant?")] 
        public float m_Fov = 30f;

        public float m_SphericalHeight;
        [Readonly] public float m_SphericalPitch, m_SphericalYaw;
        
        [Header("Zoom")]
        public float m_Zoom = 50f;
        public RangeFloat m_ZoomRange = new RangeFloat(20f, 60f);
        public float m_PitchZoomDelta = 15;
        public float m_FOVZoomDelta = 5f;
        public Vector3 m_ZoomOffsetDelta;
        public AnimationCurve m_ZoomValueCurve;
        
        [Readonly] public float m_PitchZoom=0f;
        [Readonly] public float m_YawZoom=0f;
        [Readonly] public float m_FovZoom = 0f;
        [Readonly] public Vector3 m_OffsetZoom = Vector3.zero;

        public void Init()
        {
            m_Camera = transform.GetComponent<Camera>();
            m_PositionDamper.Initialize(new Vector3(m_SphericalPitch,m_SphericalYaw,0f));
            m_RotationDamper.Initialize(new Vector3(m_PitchZoom, 0 , 0));
            m_ZoomDamper.Initialize(new Vector3(m_Zoom,0f,0f));
            Tick(1f);
        }

        public void Tick(float _deltaTime)
        {
            var baseRotation = Quaternion.Euler( m_PositionDamper.Tick(_deltaTime, new Vector3(m_SphericalPitch, m_SphericalYaw, 0f)));
            var basePosition = m_RootPosition + baseRotation * Vector3.back * (m_SphericalHeight + m_ZoomDamper.Tick(_deltaTime,Vector3.one*m_Zoom).x);

            var rotation = baseRotation * Quaternion.Euler(m_RotationDamper.Tick(_deltaTime, new Vector3(m_PitchZoom, m_YawZoom , 0))) ;
            var position = basePosition + baseRotation * m_OffsetZoom;
            m_Camera.transform.SetPositionAndRotation( position, rotation);
            m_Camera.fieldOfView = m_Fov + m_FovZoom;
        }

        public void Clear()
        {
        }

        public void Dispose()
        {
        }

        public void Drag(float _pitch,float _yaw)
        {
            m_SphericalYaw += _yaw;
            m_SphericalPitch += _pitch;
        }

        public float Pinch(float _delta)
        {
            m_Zoom = Mathf.Clamp(m_Zoom + _delta, m_ZoomRange.start, m_ZoomRange.end);

            float pitchNormalized = (1 - m_ZoomRange.NormalizedAmount(m_Zoom));
            pitchNormalized = m_ZoomValueCurve.Evaluate(pitchNormalized);
            
            m_PitchZoom = pitchNormalized * m_PitchZoomDelta;
            m_FovZoom = pitchNormalized * m_FOVZoomDelta;
            m_OffsetZoom = pitchNormalized * m_ZoomOffsetDelta;
            return pitchNormalized;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_RootPosition,1f );
        }
    }
}

    