
struct GRay
{
    float3 origin;
    float3 direction;
};
GRay GetRay(float3 _origin, float3 _direction)
{
    GRay ray;
    ray.origin = _origin;
    ray.direction = _direction;
    return ray;
}
float3 GetPoint(GRay _ray, float _distance)
{
    return _ray.origin + _ray.direction * _distance;
}

struct GPlane
{
    float3 normal;
    float distance;
};

GPlane GetPlane(float3 _normal, float _distance)
{
    GPlane plane;
    plane.normal = _normal;
    plane.distance = _distance;
    return plane;
}

struct GBox
{
    float3 boxMin;
    float3 boxMax;
};
GBox GetBox(float3 _min, float3 _max)
{
    GBox box;
    box.boxMin = _min;
    box.boxMax = _max;
    return box;
}

struct GSphere
{
    float3 center;
    float radius;
};
GSphere GetSphere(float3 _center, float _radius)
{
    GSphere sphere;
    sphere.center = _center;
    sphere.radius = _radius;
    return sphere;
}

struct GHeightCone
{
    float3 origin;
    float3 normal;
    float sqrCosA;
    float height;
    GSphere bottomSphere;
};

GHeightCone GetHeightCone(float3 _origin, float3 _normal, float _angle, float _height)
{
    GHeightCone cone;
    cone.origin = _origin;
    cone.normal = _normal;
    cone.height = _height;
    float radinA = _angle / 360. * PI;
    float cosA = cos(radinA);
    cone.sqrCosA = cosA * cosA;
    cone.bottomSphere = GetSphere(_origin + _normal * _height, _height * tan(radinA));
    return cone;
}
