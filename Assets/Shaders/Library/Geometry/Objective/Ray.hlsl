﻿struct GRay
{
    float3 origin;
    float3 direction;
    float3 GetPoint(float _distance)  {  return origin + direction * _distance;  }
    float3 SDF(float3 _point) {
        float3 pointToStart = _point - origin;
        return length(cross(direction, pointToStart));
    }
};
GRay GRay_Ctor(float3 _origin, float3 _direction)
{
    GRay ray;
    ray.origin = _origin;
    ray.direction = _direction;
    return ray;
}

GRay GRay_StartEnd(float3 _origin, float3 _end)
{
    GRay ray;
    ray.origin = _origin;
    ray.direction =  normalize(_end - _origin);
    return ray;
}