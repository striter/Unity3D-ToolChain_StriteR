using System;
using CameraController.Component;
using CameraController.Inputs;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController
{
    [CreateAssetMenu(fileName = "AnchoredController", menuName = "Camera2/Controller/AnchoredController")]
    public class AnchoredController : ACameraController
    {
        [ScriptableObjectEdit] 
        public AControllerInputProcessor m_Constrains;
        
        [Header("Rotation")]
        public float m_Pitch = 0f;
        public float m_Yaw = 0f;
        public bool m_EulerDamper = true;
        public Damper m_RotationDamper = new Damper();

        [Header("Origin")] public string m_ChildName = "";
        [CullingMask] public int m_BoundingBoxCullingMask = 0;
        [Range(0f, 1f)] public float m_BoundingBoxY;
        public float3 m_AnchorOffset = 0;
        public bool m_VerticalDamper = false;
        public Damper m_OriginDamper = new Damper();
        
        [Header("Screen Space Centering")] 
        [Range(-.5f, .5f)] public float m_ViewportX;
        [Range(-.5f, .5f)] public float m_ViewportY;
        [Range(20f, 60f)] public float m_FOV = 50f;
        public Damper m_ViewPointAnchorDamper = new Damper();    

        [Header("Distance")]
        public float m_CameraDistance = 0f;
        public FControllerCollision collision;
        public Damper m_DistanceDamper = new Damper();

        [Header("Pinch")]  
        [MinMaxRange(-10f,10f)] public RangeFloat m_PinchDistanceRange = default;
        [MinMaxRange(-20f,20f)] public RangeFloat m_PinchFovRange = default;
        
        public override IControllerInputProcessor InputProcessor => m_Constrains;
        private ControllerAnchorParameters EvaluateParameters(AControllerInput _input)
        {
            var anchor = _input.Anchor;
            anchor = anchor.Find(m_ChildName)??anchor;
            
            var origin = (float3)anchor.transform.position;
            if (m_BoundingBoxCullingMask != 0 && m_BoundingBoxY != 0)
            {
                var renderers = anchor.GetComponentsInChildren<Renderer>(false);
                var min = kfloat3.max;
                var max = kfloat3.min;
                var valid = false;
                foreach (var renderer in renderers)
                {
                    if ((m_BoundingBoxCullingMask & (1 << renderer.gameObject.layer)) == 0)
                        continue;
                    
                    var bound = renderer.bounds;
                    min = math.min(min, bound.min);
                    max = math.max(max, bound.max);
                    valid = true;
                }

                if (valid)
                    origin = GBox.Minmax(min,max).GetPoint(new float3(0, m_BoundingBoxY - .5f, 0f));
            }
            
            
            var euler = new float3(m_Pitch, m_Yaw,0) + _input.InputEuler;
            
            var viewport = new float2(m_ViewportX, m_ViewportY) + _input.InputViewPort;
            var fov = m_FOV + _input.InputFOV + m_PinchFovRange.Evaluate(_input.Pinch);
            var distance = m_CameraDistance + _input.InputDistance + m_PinchDistanceRange.Evaluate(_input.Pinch);
            
            return new ControllerAnchorParameters()
            {
                anchor = origin,
                euler = euler,
                distance = distance,
                viewport = viewport,
                fov = fov,
            };
        }
        
        public override void OnEnter(AControllerInput _input) => OnReset(_input);
        public override void OnReset(AControllerInput _input)
        {
            var parameters = EvaluateParameters(_input);
            if(m_VerticalDamper)
                m_OriginDamper.Initialize(parameters.anchor.y);
            else
                m_OriginDamper.Initialize(parameters.anchor);
            m_DistanceDamper.Initialize(parameters.distance);
            m_ViewPointAnchorDamper.Initialize(parameters.viewport.to3xy(parameters.fov));
            if(m_EulerDamper)
                m_RotationDamper.Initialize( parameters.euler );
            else
                m_RotationDamper.Initialize(quaternion.Euler(parameters.euler*kmath.kDeg2Rad));
        }

        public override FCameraOutput Tick(float _deltaTime, AControllerInput _input)
        {
            var camera = _input.Camera;
            var parameters = EvaluateParameters(_input);
            var anchorAndDistance = m_VerticalDamper? parameters.anchor.setY(m_OriginDamper.Tick(_deltaTime,parameters.anchor.y)) 
                                                            : m_OriginDamper.Tick(_deltaTime, parameters.anchor);
            var rotationWS = m_EulerDamper ?  quaternion.Euler( m_RotationDamper.Tick(_deltaTime,parameters.euler )* kmath.kDeg2Rad)
                                                : m_RotationDamper.Tick(_deltaTime,quaternion.Euler(parameters.euler*kmath.kDeg2Rad));

            anchorAndDistance.xyz += +math.mul(rotationWS, m_AnchorOffset + _input.AnchorOffset);
            var frustumRays = new GFrustumRays(anchorAndDistance.xyz,rotationWS ,camera.fieldOfView,camera.aspect,camera.nearClipPlane,camera.farClipPlane);
            
            var viewportNfov = m_ViewPointAnchorDamper.Tick(_deltaTime,parameters.viewport.to3xy(parameters.fov));

            var viewportRay = frustumRays.GetRay(viewportNfov.xy + .5f);        //remap from -0.5-0.5 to 0-1
        
            viewportRay.origin = viewportRay.GetPoint(-camera.nearClipPlane);
            var distance = m_DistanceDamper.Tick(_deltaTime,collision.Evaluate(viewportRay.Inverse(), parameters.distance));
            return new FCameraOutput()
            {
                position = viewportRay.GetPoint(-distance),
                rotation = rotationWS,
                fov = viewportNfov.z
            };
        }

        public override void OnExit()
        {
        }

        public override void DrawGizmos(AControllerInput _input)
        {
            var camera = _input.Camera;
            var parameters = EvaluateParameters(_input);
            Gizmos.matrix = Matrix4x4.identity;
            var frustumRays = new GFrustumRays(parameters.anchor,quaternion.Euler(parameters.euler* kmath.kDeg2Rad) ,camera.fieldOfView,camera.aspect,camera.nearClipPlane,camera.farClipPlane);
            var viewportRay = frustumRays.GetRay(parameters.viewport);
            viewportRay.origin = viewportRay.GetPoint(-camera.nearClipPlane);
            
            collision.DrawGizmos(viewportRay.Inverse(),parameters.distance + camera.nearClipPlane);            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(parameters.anchor,.1f);
        }
    }
    
    
    [Serializable]
    public struct ControllerAnchorParameters
    {
        public float3 anchor;
        public float3 euler;
        public float2 viewport;
        public float distance;
        public float fov;
        
        public static readonly ControllerAnchorParameters kDefault = default;

        public static ControllerAnchorParameters operator +(ControllerAnchorParameters _a, ControllerAnchorParameters _b) => new() {
            anchor = _a.anchor + _b.anchor,
            euler = _a.euler + _b.euler,
            viewport = _a.viewport + _b.viewport,
            distance = _a.distance + _b.distance,
            fov = _a.fov + _b.fov,
        };
        
        public static ControllerAnchorParameters operator -(ControllerAnchorParameters _a, ControllerAnchorParameters _b) => new() {
            anchor = _a.anchor - _b.anchor,
            euler = _a.euler - _b.euler,
            viewport = _a.viewport - _b.viewport,
            distance = _a.distance - _b.distance,
            fov = _a.fov - _b.fov,
        };
        
        public static ControllerAnchorParameters operator *(ControllerAnchorParameters _a, float _b) => new() {
            anchor = _a.anchor * _b,
            euler = _a.euler * _b,
            viewport = _a.viewport * _b,
            distance = _a.distance * _b,
            fov = _a.fov * _b,
        };
        
        public static ControllerAnchorParameters operator /(ControllerAnchorParameters _a, float _b) => new() {
            anchor = _a.anchor / _b,
            euler = _a.euler / _b,
            viewport = _a.viewport / _b,
            distance = _a.distance / _b,
            fov = _a.fov / _b,
        };
        
        public static ControllerAnchorParameters FormatDelta(AControllerInput _input) => new ControllerAnchorParameters()
        {
            euler = _input.InputEuler,
            distance = _input.InputDistance,
            viewport = _input.InputViewPort,
            fov = _input.InputFOV,
            anchor = _input.AnchorOffset,
        };
        
        public static ControllerAnchorParameters Lerp( ControllerAnchorParameters _a, ControllerAnchorParameters _b, float _t)=>_a + (_b - _a) * _t;

        public void DrawGizmos()
        {
            
        }
    }
}