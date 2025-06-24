struct GDisk
{
    float3 center;
    float radius;

    float SDF(float3 _position)
    {
        _position -= center;
        float2 q = float2(length(_position.xz)-radius,_position.y);
        return length(q);
    }
};

GDisk GDisk_Ctor(float3 _center,float _radius)
{
    GDisk torus;
    torus.center=_center;
    torus.radius=_radius;
    return torus;
}
