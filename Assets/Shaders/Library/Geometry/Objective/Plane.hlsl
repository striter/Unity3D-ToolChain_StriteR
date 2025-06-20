struct GPlane
{
    float3 normal;
    float distance;
    float3 position;
    float SDF(float3 _position)
    {
        return dot(_position,normal) + distance;
    }
};
GPlane GPlane_Ctor(float3 _normal, float _distance)
{
    GPlane plane;
    plane.normal = _normal;
    plane.distance = _distance;
    plane.position = plane.normal * plane.distance;
    return plane;
}
GPlane GPlane_Ctor(float3 _normal, float3 _position)
{
    GPlane plane;
    plane.normal = _normal;
    plane.position = _position;
    plane.distance = dot(_position, _normal);
    return plane;
}

