
struct GCapsule
{
    float3 direction;
    float height;
    float radius;
    float3 top;
    float3 bottom;

    float SDF(float3 _position)
    {
        float3 pa=_position-top;
        float3 ba=bottom-top;
        float h=saturate(dot(pa,ba)/dot(ba,ba));
        return length(pa-ba*h)-radius;
    }
};
GCapsule GCapsule_Ctor(float3 _center,float _radius,float3 _direction,float _height)
{
    GCapsule capsule;
    capsule.direction=_direction;
    capsule.height=_height;
    capsule.radius = _radius;
    float3 offset=_direction*capsule.height*.5;
    capsule.top=_center+offset;
    capsule.bottom=_center-offset;
    return capsule;
}