using System;
using System.ComponentModel;
using CameraController.Inputs;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController.Animation
{
    [CreateAssetMenu(fileName = "Interpolate", menuName = "Camera2/Animation/Interpolate")]
    public class FControllerInterpolate : AControllerPostModifer
    {
        public AnimationCurve m_PositionCurve;
        public AnimationCurve m_RotationCurve;
        public AnimationCurve m_InterpolationRadiusCurve;
        public override bool Disposable => timeExists >= math.max(m_PositionCurve.length,m_RotationCurve.length);
        public override EControllerPostModiferQueue Queue => EControllerPostModiferQueue.Interpolate;
        private float3 anchor;
        private quaternion rotation;
        private float fov;
        public override void OnBegin(AControllerInput _input)
        {
            var output = new FCameraOutput(_input.Camera);
            anchor = output.position;
            rotation = output.rotation;
            fov = output.fov;
;        }

        public override void Tick(float _deltaTime, ref FCameraOutput _output)
        {
            var positionValue = m_PositionCurve.Evaluate(timeExists);
            var rotationValue = m_RotationCurve.Evaluate(timeExists);
            
            var finalPosition = math.lerp(anchor,_output.position,positionValue);
            var finalRotation = math.slerp(rotation,_output.rotation,rotationValue);
            
            var radius = (anchor - (anchor + _output.position)/2).magnitude();
            radius *= (math.dot(math.mul(rotation, kfloat3.forward), math.mul(_output.rotation, kfloat3.back)) /2 + .5f);
            
            finalPosition += math.mul(finalRotation, kfloat3.back * radius * m_InterpolationRadiusCurve.Evaluate(timeExists));
            
            _output.rotation = finalRotation;
            _output.position = finalPosition;            
            _output.fov = math.lerp(fov,_output.fov,positionValue);
        }

    }
}