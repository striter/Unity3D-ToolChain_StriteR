using System;
using Geometry.Voxel;
using UnityEngine;

namespace PCG
{
    public class PCGCamera:MonoBehaviour,IPolyGridControl
    {
        private Transform m_CameraRoot;
        public Camera m_Camera { get; private set; }
        public Damper m_PositionDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        public Damper m_RotationDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = .2f};
        
        public Vector3 m_RootPosition = Vector3.zero;
        [Header("Constant?")] 
        public float m_PitchBase = 45f;
        public float m_YawBase = 145f;
        public float m_Fov = 30f;
        
        [Header("Zoom")]
        public float m_Zoom = 50f;
        public RangeFloat m_ZoomRange = new RangeFloat(20f, 60f);
        public float m_PitchZoomDelta = 15;
        public float m_YawZoomDelta = 90;
        public float m_FOVZoomDelta = 5f;
        public AnimationCurve m_ZoomValueCurve;
        
        [Readonly] public float m_PitchZoom=0f;
        [Readonly] public float m_YawZoom=0f;
        [Readonly] public float m_FovZoom = 0f;

        public void Init()
        {
            m_Camera = transform.GetComponent<Camera>();
            m_PositionDamper.Initialize(m_RootPosition);
            m_RotationDamper.Initialize(new Vector3(m_PitchBase + m_PitchZoom, m_YawBase + m_YawZoom, 0));
            Tick(1f);
        }

        public void Tick(float _deltaTime)
        {
            var desireRotation = new Vector3(m_PitchBase + m_PitchZoom, m_YawBase + m_YawZoom, 0);
            var position = m_RootPosition + Quaternion.Euler(desireRotation) * Vector3.forward * -m_Zoom;
            var rotation = Quaternion.Euler(m_RotationDamper.Tick(_deltaTime, desireRotation));
            m_Camera.transform.SetPositionAndRotation( m_PositionDamper.Tick(_deltaTime,position),rotation);
            m_Camera.fieldOfView = m_Fov + m_FovZoom;
        }

        public void Clear()
        {
        }

        public void Dispose()
        {
        }

        private Vector3 cameraPosition;
        private Vector3 rootOrigin;
        private Vector2 dragOrigin;
        private GFrustumRays frustumRays;
        public void SetDrag(Vector2 _origin, bool _begin)
        {
            if (!_begin)
                return;

            cameraPosition = m_Camera.transform.position;
            frustumRays = new GFrustum(m_Camera).GetFrustumRays();
            rootOrigin = m_RootPosition;
            dragOrigin = _origin;
        }

        public void Drag(Vector2 _position)
        {
            var delta = GetDragPosition(dragOrigin) - GetDragPosition(_position);
            m_RootPosition = rootOrigin + delta;
        }

        Vector3 GetDragPosition(Vector2 _screenPos)
        {
            var u = _screenPos.x / Screen.width;
            var v = 1 - _screenPos.y / Screen.height;
            var ray = new Ray(cameraPosition, UMath.BilinearLerp(frustumRays.topLeft.direction, frustumRays.topRight.direction, frustumRays.bottomRight.direction, frustumRays.bottomLeft.direction, u, v));
            return ray.GetPoint(UGeometryIntersect.RayPlaneDistance(GPlane.kZeroPlane, ray));
        }
        
        public void Rotate(float _pitch, float _yaw)
        {
            m_PitchBase = Mathf.Clamp(m_PitchBase + _pitch, 25, 90);
            m_YawBase += _yaw;
        }
        
        public void Pinch(float _delta)
        {
            m_Zoom = Mathf.Clamp(m_Zoom + _delta, m_ZoomRange.start, m_ZoomRange.end);

            float pitchNormalized = (1 - m_ZoomRange.NormalizedAmount(m_Zoom));
            pitchNormalized = m_ZoomValueCurve.Evaluate(pitchNormalized);
            
            m_PitchZoom = pitchNormalized*-m_PitchZoomDelta;
            m_YawZoom = pitchNormalized * -m_YawZoomDelta;
            m_FovZoom = pitchNormalized * -m_FOVZoomDelta;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_RootPosition,1f );
        }
    }
}

    