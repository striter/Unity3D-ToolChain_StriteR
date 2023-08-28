using Unity.Mathematics;

namespace Dome
{
    public interface IPitchYawMove : IMove
    {
        public float pitch { get; set; }    //Pitch and Yaw
        public float yaw { get; set; }
    }

    public static class IPitchYawMove_Extension
    {
        public static float2 eulerAngles(this IPitchYawMove _move)=> new float2(_move.pitch, _move.yaw);
        public static void OnInitialize(this IPitchYawMove _move,EntityInitializeParameters _parameters)
        {
            var py = umath.closestPitchYaw(_parameters.transformTR.rotation);
            _move.SetPositionAndRotation(_parameters.transformTR.position,py.x,py.y);
        }
        
        public static void SetPositionAndRotation(this IPitchYawMove _move,float3 _position,float _pitch,float _yaw)
        {
            _move.position = _position;
            _move.pitch = _pitch;
            _move.yaw = _yaw;
            _move.SetPositionAndRotation(_move.position,quaternion.Euler(new float3(_pitch,_yaw,0)*kmath.kDeg2Rad));
        }
    }
}