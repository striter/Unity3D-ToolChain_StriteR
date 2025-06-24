struct GCylinder
{
    float3 center;
    float radius;

    float SDF(float3 _position)
    {
        return length((center-_position).xy)-radius;
    }
};
GCylinder GCylinder_Ctor(float3 _center,float _radius)
{
    GCylinder cylinder;
    cylinder.center=_center;
    cylinder.radius=_radius;
    return cylinder;
}
struct GCylinderCapped
{
    GCylinder cylinder;
    float height;

    float SDF(float3 _position)
    {
        float3 p=_position;
        float2 d=abs(float2(length((p-cylinder.center).xz),p.y))-float2(cylinder.radius,height);
        return min(max(d),0)+length(max(d,0));
    }
};
GCylinderCapped GCylinderCapped_Ctor(float3 _center,float _radius,float _height)
{
    GCylinderCapped cylinderCapped;
    cylinderCapped.cylinder=GCylinder_Ctor(_center,_radius);
    cylinderCapped.height=_height;
    return cylinderCapped;
}
struct GCylinderRound
{
    GCylinder cylinder;
    float height;
    float roundRadius;
    float SDF(float3 _position)
    {
        float3 p=_position-cylinder.center;
        float2 d= float2(length(p.xz)-2.0*cylinder.radius+roundRadius,abs(p.y)-height);
        return min(max(d),0)+length(max(d,0))-roundRadius;
    }
};
GCylinderRound GCylinderRound_Ctor(float3 _center,float _radius,float _height,float _roundRadius)
{
    GCylinderRound cylinderRound;
    cylinderRound.cylinder=GCylinder_Ctor(_center,_radius);
    cylinderRound.height=_height;
    cylinderRound.roundRadius=_roundRadius;
    return cylinderRound;
}