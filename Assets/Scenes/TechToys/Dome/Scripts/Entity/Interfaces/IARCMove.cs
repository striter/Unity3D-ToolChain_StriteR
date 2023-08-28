using System;
using Unity.Mathematics;

namespace Dome.Entity
{
    [Serializable]
    public struct ARCSpeedDamper        //But it could be replaced as a damper? why am i writing these
    {
        [Clamp(0)] public float max;
        [Clamp(0)] public float accelerate;
        [Clamp(0)] public float friction;

        public float Tick(float _current,float _estimate,float _deltaTime)
        {
            var running = math.abs(_estimate) > 0;
            
            var estimateSign = math.sign(_estimate);
            //Calculate acceleration
            float acceleration;
            RangeFloat clamp = new RangeFloat(-max,max*2);
            //Clamp it
            if (running)
            {
                var currentSign = (_current >= 0 ? 1 : -1);
                acceleration = estimateSign * accelerate;
                if (Math.Abs(currentSign - estimateSign) > 0.05f)
                    acceleration = -currentSign * friction;
                _current = currentSign * math.min(math.abs(_current), max);
            }
            else
            {
                acceleration = -math.sign(_current) * friction;
                var currentSign = math.sign(_current);
                if (currentSign > 0)
                    clamp = new RangeFloat(0,_current);
                else if (currentSign < 0)
                    clamp = new RangeFloat(_current,-_current);
            }
            
            return clamp.Clamp(_current+ acceleration * _deltaTime);
        }
    }
    
    public interface IARCMove : IPitchYawMove
    {
        public FDomeEntityInput input { get; set; }
        
        public float angularSpeed { get; set; }
        public ARCSpeedDamper kAngularSpeedDamper { get; }
        
        public float speed { get; set; }
        public ARCSpeedDamper kSpeedDamper { get; }
    }

    public static class IARCMove_Extension
    {
        public static void OnInitialize(this IARCMove _move,EntityInitializeParameters _parameters)
        {
            _move.speed = 0;
        }
        
        public static void Tick(this IARCMove _move,float _deltaTime)
        {
            var input = _move.input;
            var moveInput = input.move;
            if (moveInput.sqrmagnitude() > 0.05f)    //...
                moveInput = moveInput.normalize();
            
            var forwarding = _move.speed >= 0 ? 1:-1;
            
            _move.speed = _move.kSpeedDamper.Tick(_move.speed, moveInput.y, _deltaTime);
            _move.angularSpeed = _move.kAngularSpeedDamper.Tick(_move.angularSpeed, forwarding * moveInput.x, _deltaTime);
            
            _move.yaw += _move.angularSpeed * _deltaTime;

            var rotation = quaternion.Euler(0, _move.yaw * kmath.kDeg2Rad, 0);
            var forward = math.mul(rotation, kfloat3.forward);

            var position =  _move.position;
            position += forward * _move.speed * _deltaTime;
            _move.SetPositionAndRotation(position,_move.pitch,_move.yaw);
        }
    }

}