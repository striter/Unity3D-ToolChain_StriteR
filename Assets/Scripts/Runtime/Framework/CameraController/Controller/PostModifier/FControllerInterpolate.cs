using Unity.Mathematics;
using UnityEngine;

namespace CameraController.Animation
{
    [CreateAssetMenu(fileName = "Interpolate", menuName = "Camera/PostModifier/Interpolate")]
    public class FControllerInterpolate : AControllerPostModifer
    {
        public AnimationCurve m_PositionCurve;
        public AnimationCurve m_RotationCurve;
        public AnimationCurve m_ExtraInterpolationCurve;
        public override bool Disposable => timeExists >= math.max(m_PositionCurve.length,m_RotationCurve.length);
        public override EControllerPostModiferQueue Queue => EControllerPostModiferQueue.Interpolate;
        private FCameraControllerOutput lastOutput;
        public override void OnBegin(FCameraControllerCore _input)
        {
            lastOutput = _input.m_Output;
;        }

        public override void Tick(float _deltaTime, ref FCameraControllerOutput _output)
        {
            var positionValue = m_PositionCurve.Evaluate(timeExists);
            var rotationValue = m_RotationCurve.Evaluate(timeExists);
            var extraValue = m_ExtraInterpolationCurve.Evaluate(timeExists);

            _output = new FCameraControllerOutput()
            {
                rotation = math.slerp(lastOutput.rotation, _output.rotation, rotationValue),
                anchor = math.lerp(lastOutput.anchor, _output.anchor, positionValue),
                fov = math.lerp(lastOutput.fov, _output.fov, extraValue),
                distance = math.lerp(lastOutput.distance, _output.distance, extraValue),
                viewPort = math.lerp(lastOutput.viewPort, _output.viewPort, extraValue),
            };
        }

    }
}