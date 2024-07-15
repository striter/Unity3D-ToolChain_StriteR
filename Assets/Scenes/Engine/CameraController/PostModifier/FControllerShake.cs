using Runtime.CameraController.Inputs;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.CameraController.Animation
{
    public struct FControllerShakeData : IControllerPostModifer
    {
        public Damper damper;
        public float3 shake;
        public float3 euler;
        public float fov;
        private bool stopping;
        public EControllerPostModiferQueue Queue => EControllerPostModiferQueue.Additional;
        public bool Disposable(bool _reset) => _reset || (stopping && damper.Working(float3.zero)) || timeExists > 10f;
        public float timeExists { get; set; }

        public void OnBegin(FCameraControllerCore _input)
        {
            stopping = false;
            damper.Initialize(0);
        }

        public void Tick(float _deltaTime,AControllerInput _input, ref FCameraControllerOutput _output)
        {
            var normalizedShake = damper.Tick(_deltaTime,stopping?0f:1f);
            stopping |= damper.lifeTime >= damper.duration || damper.velocity.x < 0;
            
            _output.anchor += math.mul(_output.Rotation, normalizedShake* shake);
            _output.euler += normalizedShake * euler;
            _output.fov += fov * normalizedShake;
        }

        public void OnFinished() { }
        public void DrawGizmos(AControllerInput _input) { }
    }

    [CreateAssetMenu(fileName = "Shake", menuName = "Camera/PostModifier/Shake")]
    public class FControllerShake : ScriptableObject
    {
        public Damper damper = Damper.kDefault;
        public bool random = true;
        public float3 shake;
        public float3 euler;
        public float fov;
        
        public FControllerShakeData Output() => new FControllerShakeData() {
            damper = this.damper,
            shake = this.shake * (random ? UnityEngine.Random.insideUnitSphere : kfloat3.one),
            euler = this.euler * (random ? UnityEngine.Random.insideUnitSphere : kfloat3.one),
            fov = this.fov
        };
    }
}