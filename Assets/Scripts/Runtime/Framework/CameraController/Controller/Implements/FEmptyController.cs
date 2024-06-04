using System.Collections.Generic;
using CameraController.Animation;
using CameraController.Inputs;

namespace CameraController
{
    public class FEmptyController : ICameraController
    {
        public IEnumerable<IControllerInputProcessor> InputProcessor
        {
            get
            {
                yield break;
            }
        }

        public IEnumerable<IControllerPostModifer> PostModifier
        {
            get
            {
                yield break;
            }
        }

        public void OnEnter(AControllerInput _input)
        {
            
        }

        public bool Tick(float _deltaTime, AControllerInput _input, ref FCameraControllerOutput _output)
        {
            return false;
        }

        public void OnReset(AControllerInput _input)
        {
        }

        public void OnExit()
        {
        }

        public void DrawGizmos(AControllerInput _input)
        {
        }
        
        public static FEmptyController kDefault = new FEmptyController();
    }
}