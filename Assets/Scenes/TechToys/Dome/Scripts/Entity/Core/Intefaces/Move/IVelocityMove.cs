using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity
{
    public interface IVelocityMove : IMove
    {
        public float3 lastPosition { get; set; }
        public float3 velocity { get; set; }
        public float kStartSpeed { get; }
    }

    public static class IVelocityMove_Extension
    {
        public static void OnInitialize(this IVelocityMove _move,EntityInitializeParameters _parameters)
        {
            _move.velocity = math.mul(_move.rotation,kfloat3.forward)*_move.kStartSpeed;
            _move.lastPosition = _move.position;
        }

        public static void Tick(this IVelocityMove _move, float _deltaTime)
        {
            if (_move.velocity.sqrmagnitude() <= 0) return;
            
            _move.lastPosition = _move.position;
            var nextPosition = _move.lastPosition + _deltaTime * _move.velocity;
            _move.SetPositionAndRotation(_move.position + _move.velocity*_deltaTime, quaternion.LookRotation(nextPosition - _move.lastPosition,kfloat3.up));
        }
    }
}