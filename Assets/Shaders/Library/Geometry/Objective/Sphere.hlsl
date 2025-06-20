struct GSphere
{
    float3 center;
    float radius;
    float SDF(float3 _point) { return length(_point - center) - radius; }
};

GSphere GSphere_Ctor(float3 _center, float _radius)
{
    GSphere sphere;
    sphere.center = _center;
    sphere.radius = _radius;
    return sphere;
}