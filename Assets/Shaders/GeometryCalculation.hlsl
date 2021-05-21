#include "GeometryInput.hlsl"
//Point
float PointRayProjectDistance(GRay _ray,float3 _point)
{
    return dot(_point-_ray.origin, _ray.direction);
}

//Line
float3 PointLineProjectionDistance(GLine _line,float3 _point)
{
    return clamp(dot(_point - _line.origin, _line.direction),0,_line.length);
}
float2 LineRayProjectionDistance(GLine _line,GRay _ray)      
{
    float3 diff = _line.origin - _ray.origin;
    float a01 = -dot(_line.direction, _ray.direction);
    float b0 = dot(diff, _line.direction);
    float b1 = -dot(diff, _ray.direction);
    float det = 1. - a01 * a01;
    
    float s0 = 0., s1 = 0.;
    s0 = (a01 * b1 - b0) / det;
    s1 = (a01 * b0 - b1)/det;
    return float2(s0, s1);
}

//Plane
float PlanePointDistance(GPlane _plane,float3 _point)
{
    float nr = _point.x * _plane.normal.x + _point.y * _plane.normal.y + _point.z * _plane.normal.z + _plane.distance;
    return nr / length(_plane.normal);
}
float PlaneRayDistance(GPlane _plane,GRay _ray)
{
    float nrO = dot(_plane.normal, _ray.origin);
    float nrD = dot(_plane.normal, _ray.direction);
    return (_plane.distance-nrO)/nrD;
}
float PlaneRayDistance(GPlanePos _plane,GRay _ray)
{   
    float s = dot(_plane.position,_plane.normal);
    float nrO = dot(_plane.normal, _ray.origin);
    float nrD = dot(_plane.normal, _ray.direction);
    return (s - nrO) / nrD;
}

//Axis Aligned Bounding Box
bool AABBPositionInside(GBox _box,float3 _pos)
{
    return _box.boxMin.x <= _pos.x && _pos.x <= _box.boxMax.x 
        && _box.boxMin.y <= _pos.y && _pos.y <= _box.boxMax.y 
        && _box.boxMin.z <= _pos.z && _pos.z <= _box.boxMax.z;
}
bool AABBRayIntersect(GBox _box,GRay _ray)
{
    float3 invRayDir = 1 / _ray.direction;
    float3 t0 = (_box.boxMin- _ray.origin) * invRayDir;
    float3 t1 = (_box.boxMax- _ray.origin) * invRayDir;
    float3 tmin=min(t0,t1);
    float3 tmax=max(t0,t1);
    return max(tmin)<=min(tmax);
}
float2 AABBRayDistance(GBox _box, GRay _ray)    //X: Dst To Box , Y:Dst In Side Box
{
    float3 invRayDir = 1. / _ray.direction;
    float3 t0 = (_box.boxMin - _ray.origin) * invRayDir;
    float3 t1 = (_box.boxMax - _ray.origin) * invRayDir;
    float3 tmin = min(t0, t1);
    float3 tmax = max(t0, t1);

    float dstA = max(max(tmin.x, tmin.y), tmin.z);
    float dstB = min(tmax.x, min(tmax.y, tmax.z));

    float dstToBox = max(0., dstA);
    float dstInsideBox = max(0., dstB - dstToBox);
    return float2(dstToBox, dstInsideBox);
}

//Bounding Sphere 
float2 SphereRayDistance(GSphere _sphere, GRay _ray)
{
    float3 offset = _ray.origin - _sphere.center;
    float dotOffsetDirection = dot(_ray.direction, offset);
    float dotOffset = dot(offset, offset);
    float sqrRadius = _sphere.radius * _sphere.radius;
    float discriminant = dotOffsetDirection * dotOffsetDirection - dotOffset + sqrRadius;
    discriminant = sqrt(discriminant);
    float t0 = -dotOffsetDirection - discriminant;
    float t1 = -dotOffsetDirection + discriminant;
    return float2(t0, t1) * step(0., discriminant) * step(dotOffsetDirection, 0.);
}
