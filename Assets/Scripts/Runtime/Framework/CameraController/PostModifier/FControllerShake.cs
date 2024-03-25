using CameraController.Inputs;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController.Animation
{
    [CreateAssetMenu(fileName = "Shake", menuName = "Camera2/Animation/Shake")]
    public class FControllerShake : AControllerPostModifer
    {
        public override EControllerPostModiferQueue Queue => EControllerPostModiferQueue.Additional;

        public float3 shake;
        public float3 euler;
        public float fov;
        
        public Damper damper = new Damper();
        public override bool Disposable => stopping && damper.Working(float3.zero);
            
        private bool stopping;
        public override void OnBegin(AControllerInput _input)
        {
            stopping = false;
            damper.Initialize(0);
        }

        public override void Tick(float _deltaTime, ref FCameraOutput _output)
        {
            var normalizedShake = damper.Tick(_deltaTime,stopping?0f:1f);
            stopping |= damper.lifeTime >= damper.duration || damper.velocity.x < 0;
            
            _output.position += math.mul(_output.rotation, normalizedShake* shake);
            _output.rotation = math.mul(_output.rotation, quaternion.Euler(normalizedShake * euler * kmath.kDeg2Rad));
            _output.fov += fov * normalizedShake;
        }
    }
}