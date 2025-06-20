
struct GCone
{
    float3 origin;
    float3 normal;
    float sqrCosA;
    float tanA;
};
GCone GCone_Ctor(float3 _origin, float3 _normal, float _angle)
{
    GCone cone;
    cone.origin = _origin;
    cone.normal = _normal;
    float radianA = _angle / 360. * PI;
    float cosA = cos(radianA);
    cone.sqrCosA = cosA * cosA;
    cone.tanA = tan(radianA);
    return cone;
}

struct GHeightCone
{
    float3 origin;
    float3 normal;
    float sqrCosA;
    float tanA;
    float height;
    float3 bottom;
    float bottomRadius;
    GPlane bottomPlane;
    float GetRadius(float _height)
    {
        return _height * tanA;
    }
};
GHeightCone GHeightCone_Ctor(float3 _origin, float3 _normal, float _angle, float _height)
{
    GHeightCone cone;
    cone.origin = _origin;
    cone.normal = _normal;
    cone.height = _height;
    float radianA = _angle / 360. * PI;
    float cosA = cos(radianA);
    cone.sqrCosA = cosA * cosA;
    cone.tanA = tan(radianA);
    cone.bottom = _origin + _normal * _height;
    cone.bottomRadius = _height *cone. tanA;
    cone.bottomPlane = GPlane_Ctor(_normal, cone.bottom);
    return cone;
}