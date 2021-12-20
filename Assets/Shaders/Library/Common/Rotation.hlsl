#define Deg2Rad 0.017453292519943//PI / 180
#define Rad2Deg 57.295779513082 //180f / PI

float2x2 Rotate2x2(float _angle)
{
    float sinAngle, cosAngle;
    sincos(_angle, sinAngle, cosAngle);
    return float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
}

float3x3 Rotate3x3(float _radin, float3 _axis)
{
    float s, c;
    sincos(_radin, s, c);

    float t = 1 - c;
    float x = _axis.x;
    float y = _axis.y;
    float z = _axis.z;

    return float3x3(t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c);
}

float3 RotateAround(float3 _position,float3 _rotateAround,float _angle,float3 _rotateAxis)
{
    return _rotateAround+mul(Rotate3x3(_angle,_rotateAxis),_position-_rotateAround);
}

