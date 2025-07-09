﻿using System.Collections.Generic;
using CameraController.Animation;
using CameraController.Component;
using CameraController.Inputs;
using UnityEngine;

namespace CameraController
{
    public abstract class AAnchoredController : ACameraController
    {
        [ScriptableObjectEdit] public AControllerInputProcessor m_InputProcessor;
        [ScriptableObjectEdit] public FControllerCollision m_Collision;
        public FAnchorDamper m_Anchor = new ();
        public FRotationDamper m_Rotation = new ();
        public Damper m_DistanceDamper = Damper.kDefault;
        public Damper m_ViewportDamper = Damper.kDefault;    
        public override IEnumerable<IControllerInputProcessor> InputProcessor
        {
            get
            {   
                if(m_InputProcessor == null) yield break;
                yield return m_InputProcessor;
            }
        }

        public override IEnumerable<IControllerPostModifer> PostModifier { get      //lets handle it with distance damper
            {
                // if (m_Collision == null) yield break;
                yield break;
            }
        }

        protected abstract FCameraControllerOutput EvaluateBaseParameters(AControllerInput _input);

        public override void OnEnter(AControllerInput _input)
        {
            OnReset(_input);
        }
        public override void OnReset(AControllerInput _input)
        {
            var baseParameters = EvaluateBaseParameters(_input);
            var playerInputParameters = FCameraControllerOutput.FormatDelta(_input);
            var parameters = baseParameters + playerInputParameters;
            m_Anchor.Initialize(_input,baseParameters);
            m_Rotation.Initialize(playerInputParameters,baseParameters);
            m_DistanceDamper.Initialize(parameters.distance);
            m_ViewportDamper.Initialize(parameters.viewport.to3xy(parameters.fov));
        }

        public override bool Tick(float _deltaTime, AControllerInput _input,ref FCameraControllerOutput _output)
        {
            var baseParameters = EvaluateBaseParameters(_input);
            var playerInputParameters = FCameraControllerOutput.FormatDelta(_input);
            var viewportNfov = m_ViewportDamper.Tick(_deltaTime,baseParameters.viewport.to3xy(baseParameters.fov));
            _output = new FCameraControllerOutput()
            {
                anchor = m_Anchor.Tick(_deltaTime, _input, baseParameters) + playerInputParameters.anchor,
                euler = m_Rotation.Tick(_deltaTime,playerInputParameters,baseParameters),
                viewport = viewportNfov.xy + playerInputParameters.viewport,
                fov = viewportNfov.z + playerInputParameters.fov,
                distance = baseParameters.distance + playerInputParameters.distance,
            };

            _output.Evaluate(_input.Camera, out _, out var ray);
            var hitDistance = _output.distance;
            if (m_Collision != null && m_Collision.CalculateDistance(ray, hitDistance, out hitDistance))
            {
                if(m_DistanceDamper.value.x > hitDistance)
                    m_DistanceDamper.Initialize(hitDistance);
            }
            
            _output.distance = m_DistanceDamper.Tick(_deltaTime, hitDistance);
            
            return true;
        }

        public override void OnExit()
        {
        }
        public override void DrawGizmos(AControllerInput _input)
        {
            Gizmos.matrix = Matrix4x4.identity;
            
            var parameters = EvaluateBaseParameters(_input) + FCameraControllerOutput.FormatDelta(_input);
            var output = new FCameraControllerOutput()
            {
                anchor = m_Anchor.DrawGizmos(_input,parameters),
                euler = parameters.euler,
                viewport = parameters.viewport,
                fov = parameters.fov,
                distance = parameters.distance,
            };
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(output.anchor,.05f);
        }
    }
}