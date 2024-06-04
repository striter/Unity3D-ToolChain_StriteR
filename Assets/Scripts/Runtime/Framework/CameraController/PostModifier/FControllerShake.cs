using CameraController.Inputs;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController.Animation
{
    [CreateAssetMenu(fileName = "Shake", menuName = "Camera/PostModifier/Shake")]
    public class FControllerShake : AControllerPostModifer
    {
        public override EControllerPostModiferQueue Queue => EControllerPostModiferQueue.Additional;

        public float3 shake;
        public float3 euler;
        public float fov;
        
        public Damper damper = new Damper();
        public override bool Disposable(bool _reset) => _reset || (stopping && damper.Working(float3.zero));
            
        private bool stopping;
        public override void OnBegin(FCameraControllerCore _input)
        {
            stopping = false;
            damper.Initialize(0);
        }

        public override void Tick(float _deltaTime,AControllerInput _input, ref FCameraControllerOutput _output)
        {
            var normalizedShake = damper.Tick(_deltaTime,stopping?0f:1f);
            stopping |= damper.lifeTime >= damper.duration || damper.velocity.x < 0;
            
            _output.anchor += math.mul(_output.Rotation, normalizedShake* shake);
            _output.euler += normalizedShake * euler * kmath.kDeg2Rad;
            _output.fov += fov * normalizedShake;
        }
    }
}