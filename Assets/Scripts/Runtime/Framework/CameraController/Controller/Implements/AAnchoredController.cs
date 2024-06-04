using System.Collections.Generic;
using System.Linq;
using CameraController.Animation;
using CameraController.Component;
using CameraController.Inputs;
using UnityEngine;
using UnityEngine.Serialization;

namespace CameraController
{
    public abstract class AAnchoredController : ACameraController
    {
        [ScriptableObjectEdit] public AControllerInputProcessor m_InputProcessor;
        [ScriptableObjectEdit] public AControllerPostModifer m_Collision;
        [Header("Position Damper")] public FAnchorDamper m_Anchor = new FAnchorDamper();
        [Header("Rotation Damper")] public FRotationDamper m_Rotation = new FRotationDamper();
        [Header("Distance Damper")]  public Damper m_DistanceDamper = new Damper();
        [Header("Viewport Damper")] public Damper m_ViewportDamper = new Damper();    
        public override IEnumerable<IControllerInputProcessor> InputProcessor { get
            {
                if (m_InputProcessor == null) yield break;
                yield return m_InputProcessor;
            }
        }
        public override IEnumerable<IControllerPostModifer> PostModifier { get
            {
                if (m_Collision == null) yield break;
                yield return m_Collision;
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
                euler = m_Rotation.Tick(_deltaTime,playerInputParameters,baseParameters),
                viewPort = viewportNfov.xy + playerInputParameters.viewport,
                fov = viewportNfov.z + playerInputParameters.fov,
                distance = m_DistanceDamper.Tick(_deltaTime,baseParameters.distance + playerInputParameters.distance),
            };
            
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
                euler = parameters.euler,
                viewPort = parameters.viewport,
                fov = parameters.fov,
                distance = parameters.distance,
            };
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(output.anchor,.05f);
        }
    }
}