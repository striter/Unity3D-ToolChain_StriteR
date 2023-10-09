
//Point&Line
float PointRayProjection(GRay _ray, float3 _point)
{
    return dot(_point - _ray.origin, _ray.direction);
}
float3 PointRayProjection(GLine _line, float3 _point)
{
    return clamp(PointRayProjection(_line.ToRay(), _point), 0., _line.length);
}

float2 RayRayProjection(GRay _ray1, GRay _ray2)
{
    float3 diff = _ray2.origin - _ray1.origin;
    float a01 = -dot(_ray2.direction, _ray1.direction);
    float b0 = dot(diff, _ray2.direction);
    float b1 = -dot(diff, _ray1.direction);
    float det = 1. - a01 * a01;
    return float2((a01 * b0 - b1) / det, (a01 * b1 - b0) / det);
}

float2 LineRayProjection(GLine _line, GRay _ray)
{
    float2 distances = RayRayProjection(_line.ToRay(), _ray);
    distances.x = clamp(distances.x, 0., _line.length);
    distances.y = PointRayProjection(_ray, _line.GetPoint(distances.x));
    return distances;
}
//Plane
float PlanePointDistance(GPlane _plane, float3 _point)
{
    float nr = _point.x * _plane.normal.x + _point.y * _plane.normal.y + _point.z * _plane.normal.z + _plane.distance;
    return nr / length(_plane.normal);
}
float PlaneRayDistance(GPlane _plane, GRay _ray)
{
    float nrO = dot(_plane.normal, _ray.origin);
    float nrD = dot(_plane.normal, _ray.direction);
    return (_plane.distance - nrO) / nrD;
}

//Axis Aligned Bounding Box
bool AABBPositionInside(GBox _box, float3 _pos)
{
    return _box.boxMin.x <= _pos.x && _pos.x <= _box.boxMax.x 
        && _box.boxMin.y <= _pos.y && _pos.y <= _box.boxMax.y 
        && _box.boxMin.z <= _pos.z && _pos.z <= _box.boxMax.z;
}
bool AABBRayIntersect(GBox _box, GRay _ray)
{
    float3 invRayDir = 1.0 / _ray.direction;
    float3 t0 = (_box.boxMin - _ray.origin) * invRayDir;
    float3 t1 = (_box.boxMax - _ray.origin) * invRayDir;
    float3 tmin = min(t0, t1);
    float3 tmax = max(t0, t1);
    return max(tmin) <= min(tmax);
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
    return max(0,float2(t0, t1));// * step(0., discriminant) * step(dotOffsetDirection, 0.);
}

 //Heighted Cone 
float2 ConeRayDistance(GHeightCone _cone, GRay _ray)
{
    float3 offset = _ray.origin - _cone.origin;

    float RDV = dot(_ray.direction, _cone.normal);
    float ODN = dot(offset, _cone.normal);

    float a = RDV * RDV - _cone.sqrCosA;
    float b = 2. * (RDV * ODN - dot(_ray.direction, offset) * _cone.sqrCosA);
    float c = ODN * ODN - dot(offset, offset) * _cone.sqrCosA;
    float det = b * b - 4. * a * c;
    if (det < 0.) return 0;
    float sqrtDet = sqrt(det);
    float t0 = (-b + sqrtDet) *rcp (2. * a); 
    float t1 = (-b - sqrtDet) *rcp (2. * a);

    float t = t1;
    if (t < 0. || t1 > 0. && t1 < t) t = t1;
    // if (t < 0.) return 0;
    
    float bpDistance = PlaneRayDistance(_cone.bottomPlane, _ray);
    float sqrRadius = _cone.bottomRadius * _cone.bottomRadius;
    if (sqrDistance(_cone.bottom - _ray.GetPoint(bpDistance)) > sqrRadius)
        bpDistance = -1;
    float surfaceDst = dot(_cone.normal, _ray.GetPoint(t0) - _cone.origin);
    if (surfaceDst < 0 || surfaceDst > _cone.height)
        t0 = bpDistance;
    
    surfaceDst = dot(_cone.normal, _ray.GetPoint(t1) - _cone.origin);
    if (surfaceDst < 0 || surfaceDst > _cone.height)
        t1 = bpDistance;

    return float2(t0, t1);
}
