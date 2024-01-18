
bool Intersect(GBox _box, GRay _ray)
{
    float3 invRayDir = 1.0 / _ray.direction;
    float3 t0 = (_box.boxMin - _ray.origin) * invRayDir;
    float3 t1 = (_box.boxMax - _ray.origin) * invRayDir;
    float3 tmin = min(t0, t1);
    float3 tmax = max(t0, t1);
    return max(tmin) <= min(tmax);
}
