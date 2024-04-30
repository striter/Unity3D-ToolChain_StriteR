using System;
using System.Collections.Generic;
using CameraController.Component;
using CameraController.Inputs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace CameraController
{
    public abstract class AAnchoredController : ACameraController
    {
        [ScriptableObjectEdit] public AControllerInputProcessor m_InputProcessor;
        [Header("Position Damper")] public FAnchorDamper m_Anchor = new FAnchorDamper();
        [Header("Rotation Damper")] public FRotationDamper m_Rotation = new FRotationDamper();
        [Header("Distance Damper")] public FControllerCollision m_Collision; public Damper m_DistanceDamper = new Damper();
        [Header("Viewport Damper")] public Damper m_ViewportDamper = new Damper();    
        public override IEnumerable<IControllerInputProcessor> InputProcessor { get
            {
                if (m_InputProcessor == null) yield break;
                yield return m_InputProcessor;
            }
        }

        protected abstract AnchoredControllerParameters EvaluateBaseParameters(AControllerInput _input);

        public override void OnEnter(AControllerInput _input)
        {
            OnReset(_input);
        }
        public override void OnReset(AControllerInput _input)
        {
            var baseParameters = EvaluateBaseParameters(_input);
            var playerInputParameters = AnchoredControllerParameters.FormatDelta(_input);
            var parameters = baseParameters + playerInputParameters;
            m_Anchor.Initialize(_input,baseParameters);
            m_Rotation.Initialize(playerInputParameters,baseParameters);
            m_DistanceDamper.Initialize(parameters.distance);
            m_ViewportDamper.Initialize(parameters.viewport.to3xy(parameters.fov));
        }

        public override bool Tick(float _deltaTime, AControllerInput _input,ref FCameraControllerOutput _output)
        {
            var baseParameters = EvaluateBaseParameters(_input);
            var playerInputParameters = AnchoredControllerParameters.FormatDelta(_input);
            var viewportNfov = m_ViewportDamper.Tick(_deltaTime,baseParameters.viewport.to3xy(baseParameters.fov));
            _output = new FCameraControllerOutput()
            {
                anchor = m_Anchor.Tick(_deltaTime, _input, baseParameters) + playerInputParameters.anchor,
                rotation = m_Rotation.Tick(_deltaTime,playerInputParameters,baseParameters),
                viewPort = viewportNfov.xy + playerInputParameters.viewport,
                fov = viewportNfov.z + playerInputParameters.fov,
                distance = m_DistanceDamper.Tick(_deltaTime,baseParameters.distance + playerInputParameters.distance),
            };
            
            _output.Evaluate(_input.Camera, out var frustumRays, out var viewportRay);
            _output.distance = m_Collision.Evaluate(_input.Camera,viewportRay, _output.distance);
            return true;
        }

        public override void OnExit()
        {
        }
        public override void DrawGizmos(AControllerInput _input)
        {
            Gizmos.matrix = Matrix4x4.identity;
            
            var parameters = EvaluateBaseParameters(_input) + AnchoredControllerParameters.FormatDelta(_input);
            var output = new FCameraControllerOutput()
            {
                anchor = m_Anchor.DrawGizmos(_input,parameters),
                rotation = quaternion.Euler(parameters.euler * kmath.kDeg2Rad),
                viewPort = parameters.viewport,
                fov = parameters.fov,
                distance = parameters.distance,
            };
            output.Evaluate(_input.Camera, out var frustumRays, out var viewportRay);
            Gizmos.color = Color.white;
            m_Collision.DrawGizmos(_input.Camera,viewportRay,parameters.distance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(output.anchor,.05f);
        }
    }
}