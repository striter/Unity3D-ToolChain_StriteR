using CameraController.Inputs;
using UnityEngine;

namespace CameraController
{
    public class FEmptyController : ICameraController
    {
        public IControllerInputProcessor InputProcessor =>  ControllerInputProcessorCore.kDefault;
        public void OnEnter(AControllerInput _input)
        {
            
        }

        public FCameraOutput Tick(float _deltaTime, AControllerInput _input)
        {
            // _output.rotation = math.mul( _input.Anchor.rotation,quaternion.Euler(_input.InputEuler* kmath.kDeg2Rad));
            // var forward = math.mul(_output.rotation, kfloat3.forward);
            // _output.position = (float3)_input.Anchor.position + forward* _input.InputDistance + math.mul(_output.rotation , _input.AnchorOffset);
            // _output.fov = 60f + _input.InputFOV;
            return new FCameraOutput(_input.Camera);
        }


        public void OnExit()
        {
        }

        public void DrawGizmos(AControllerInput _input)
        {
            throw new System.NotImplementedException();
        }

        public void OnReset(AControllerInput _input)
        {
        }


        public static readonly FEmptyController kDefault = new FEmptyController();
    }
}